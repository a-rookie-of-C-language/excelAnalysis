using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Concurrent;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using excel.DAO;
using excel.entity;
using excel.ViewModels;

namespace excel.Views;

public partial class HomeView : UserControl{
    private static readonly string[] AllowedExtensions = [".xlsx", ".xls", ".csv"];

    public HomeView() {
        InitializeComponent();
    }

    private void SelectExcelButton_OnClick(object sender, RoutedEventArgs e) {
        var dlg = new OpenFileDialog {
            Filter = "Excel/CSV 文件|*.xlsx;*.xls;*.csv|所有文件|*.*",
            Multiselect = false
        };

        if (dlg.ShowDialog() == true) {
            HandleSelectedFile(dlg.FileName);
        }
    }

    private static bool IsAllowedFile(string path) {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return AllowedExtensions.Contains(ext);
    }

    private void HandleSelectedFile(string path) {
        if (!IsAllowedFile(path)) {
            MessageBox.Show("文件类型不支持，请选择 xlsx/xls/csv。",
                "提示", MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try {
            var sheet = GetSheet(path);
            var students = GetStudentInfo(sheet);
            // 依据文件名生成分表名：StudentInfo_<安全文件名>_<哈希前8位>
            var rawName = System.IO.Path.GetFileNameWithoutExtension(path);
            var safeName = new string(rawName.Select(ch =>
                char.IsLetterOrDigit(ch) ? ch : '_').ToArray());
            if (string.IsNullOrWhiteSpace(safeName) || !(char.IsLetter(safeName[0]) || safeName[0] == '_'))
                safeName = "f" + (safeName ?? "");
            var hash8 = ComputeFileHash(path)[..8];
            var tableName = $"StudentInfo_{safeName}_{hash8}";
            DAO.StudentTableDao.EnsureTableExists(tableName);
            var insertedCount = BulkInsertStudents(students, tableName);

            // 记录导入历史
            try {
                var dao = new ImportHistoryDao();
                var history = new ImportHistory {
                    FilePath = path,
                    FileName = System.IO.Path.GetFileName(path),
                    FileHash = ComputeFileHash(path),
                    TableName = tableName,
                    ImportedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    StudentCount = insertedCount,
                    Note = "批量导入"
                };
                dao.Insert(history);
            }
            catch { /* 忽略历史记录写入失败 */ }

            NavigateToStudentInfo();
        }
        catch (Exception ex) {
            MessageBox.Show($"解析失败：{ex.Message}",
                "错误", MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private int BulkInsertStudents(List<StudentBasicInfo> students, string? tableName = null) {
        if (students.Count == 0) return -1;

        // 将基本信息映射到实体
        var entities = students.Select(s => new Student {
            Id = s.Id,
            Name = s.Name,
            Class = s.Class,
            College = s.College,
            Major = s.Major,
            Grade = s.Grade
        }).ToList();

        // 分批处理，降低单次事务压力
        const int batchSize = 500;
        var batches = Partitions(entities, batchSize);

        var inserted = new ConcurrentBag<Student>();

        Parallel.ForEach(batches, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, batch => {
            using var conn = Connection.CreateConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();
            try {
                var exec = new SqlExecPerConn(conn, tx, tableName);
                foreach (var s in batch) {
                    var affected = exec.InsertOrIgnore(s);
                    if (affected > 0) inserted.Add(s);
                }
                tx.Commit();
            }
            catch {
                try { tx.Rollback(); } catch { }
                throw;
            }
            finally {
                conn.Close();
            }
        });

        // 将新插入的学生同步到 UI 集合（确保在 UI 线程执行）
        Application.Current.Dispatcher.Invoke(() => {
            foreach (var s in inserted) {
                StudentState.Students.Add(s);
            }
        });
        return inserted.Count;
    }

    private static IEnumerable<List<T>> Partitions<T>(List<T> source, int size) {
        for (int i = 0; i < source.Count; i += size) {
            yield return source.GetRange(i, Math.Min(size, source.Count - i));
        }
    }

    private ISheet GetSheet(string path) {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        IWorkbook workbook;
        var ext = Path.GetExtension(path).ToLowerInvariant();
        if (ext == ".xls")
            workbook = new HSSFWorkbook(fs);
        else
            workbook = new XSSFWorkbook(fs);
        return workbook.GetSheetAt(0);
    }

    private static string ComputeFileHash(string path) {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var sha = System.Security.Cryptography.SHA256.Create();
        var hash = sha.ComputeHash(fs);
        return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
    }

    private class HeaderMap{
        public int ClassIdx { get; init; } = -1;
        public int IdIdx { get; init; } = -1;
        public int NameIdx { get; init; } = -1;
        public int CollegeIdx { get; init; } = -1;
        public int MajorIdx { get; init; } = -1;
        public int GradeIdx { get; init; } = -1;
        public bool IsValid => new[] { ClassIdx, IdIdx, NameIdx, CollegeIdx, MajorIdx, GradeIdx }.All(i => i >= 0);
    }

    public record StudentBasicInfo(string Class, string Id, string Name, string College, string Major, string Grade);

    private static string GetCell(IRow row, int idx, DataFormatter fmt) {
        if (idx < 0) return string.Empty;
        var cell = row.GetCell(idx);
        return (fmt.FormatCellValue(cell) ?? string.Empty).Trim();
    }

    private static int FindIndex(IRow header, string target, DataFormatter fmt) {
        for (int c = header.FirstCellNum; c < header.LastCellNum; c++) {
            var text = (fmt.FormatCellValue(header.GetCell(c)) ?? string.Empty).Trim();
            if (string.Equals(text, target, StringComparison.OrdinalIgnoreCase))
                return c;
        }

        return -1;
    }

    private static HeaderMap BuildHeader(IRow headerRow, DataFormatter fmt) {
        return new HeaderMap {
            ClassIdx = FindIndex(headerRow, "班级", fmt),
            IdIdx = FindIndex(headerRow, "学号", fmt),
            NameIdx = FindIndex(headerRow, "姓名", fmt),
            CollegeIdx = FindIndex(headerRow, "学院", fmt),
            MajorIdx = FindIndex(headerRow, "专业", fmt),
            GradeIdx = FindIndex(headerRow, "年级", fmt)
        };
    }

    private List<StudentBasicInfo> GetStudentInfo(ISheet sheet) {
        var fmt = new DataFormatter();
        // 默认首行作为表头
        var headerRowIndex = sheet.FirstRowNum;
        var headerRow = sheet.GetRow(headerRowIndex) ?? sheet.GetRow(sheet.FirstRowNum);
        var map = BuildHeader(headerRow, fmt);
        if (!map.IsValid)
            throw new InvalidOperationException("未找到必要的列：班级/学号/姓名/学院/专业/年级");

        var set = new HashSet<string>();
        var list = new List<StudentBasicInfo>();
        for (int r = headerRowIndex + 1; r <= sheet.LastRowNum; r++) {
            var row = sheet.GetRow(r);
            if (row == null) continue;

            var cls = GetCell(row, map.ClassIdx, fmt);
            var id = GetCell(row, map.IdIdx, fmt);
            var name = GetCell(row, map.NameIdx, fmt);
            var college = GetCell(row, map.CollegeIdx, fmt);
            var major = GetCell(row, map.MajorIdx, fmt);
            var grade = GetCell(row, map.GradeIdx, fmt);

            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name))
                continue;

            var key = string.Join("|", new[] { cls, id, name, college, major, grade });
            if (set.Add(key))
                list.Add(new StudentBasicInfo(cls, id, name, college, major, grade));
        }

        return list;
    }
     
    private void NavigateToStudentInfo() {
        var window = Window.GetWindow(this);
        if (window?.DataContext is MainWindowViewModel vm) {
            var target = vm.NavigationItems.FirstOrDefault(n => n.Name == "学生信息");
            if (target != null) vm.SelectedItem = target;
        }
    }
    
}