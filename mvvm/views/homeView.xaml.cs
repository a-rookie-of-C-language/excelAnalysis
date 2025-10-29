using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using OfficeOpenXml;
using excel.mvvm.model;
using excel.Services;

namespace excel.mvvm.views{
    public partial class homeView : UserControl, INotifyPropertyChanged{
        public event PropertyChangedEventHandler? PropertyChanged;

        // 支持的文件类型
        private readonly string[] _supportedExtensions = { ".xlsx", ".xls" };

        // 绑定属性
        private bool _showDefaultPanel = true;
        private bool _showFileInfo = false;
        private bool _showProgress = false;
        private bool _showStatus = false;
        private bool _isDragOver = false;
        private string _selectedFileName = "";
        private string _selectedFileSize = "";
        private double _uploadProgress = 0;
        private string _uploadProgressText = "";
        private string _statusMessage = "";
        private Brush _statusColor = Brushes.Green;
        private string _selectedFilePath = "";

        public bool ShowDefaultPanel {
            get => _showDefaultPanel;
            set {
                _showDefaultPanel = value;
                OnPropertyChanged(nameof(ShowDefaultPanel));
            }
        }

        public bool ShowFileInfo {
            get => _showFileInfo;
            set {
                _showFileInfo = value;
                OnPropertyChanged(nameof(ShowFileInfo));
            }
        }

        public bool ShowProgress {
            get => _showProgress;
            set {
                _showProgress = value;
                OnPropertyChanged(nameof(ShowProgress));
            }
        }

        public bool ShowStatus {
            get => _showStatus;
            set {
                _showStatus = value;
                OnPropertyChanged(nameof(ShowStatus));
            }
        }

        public bool IsDragOver {
            get => _isDragOver;
            set {
                _isDragOver = value;
                OnPropertyChanged(nameof(IsDragOver));
            }
        }

        public string SelectedFileName {
            get => _selectedFileName;
            set {
                _selectedFileName = value;
                OnPropertyChanged(nameof(SelectedFileName));
            }
        }

        public string SelectedFileSize {
            get => _selectedFileSize;
            set {
                _selectedFileSize = value;
                OnPropertyChanged(nameof(SelectedFileSize));
            }
        }

        public double UploadProgress {
            get => _uploadProgress;
            set {
                _uploadProgress = value;
                OnPropertyChanged(nameof(UploadProgress));
            }
        }

        public string UploadProgressText {
            get => _uploadProgressText;
            set {
                _uploadProgressText = value;
                OnPropertyChanged(nameof(UploadProgressText));
            }
        }

        public string StatusMessage {
            get => _statusMessage;
            set {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public Brush StatusColor {
            get => _statusColor;
            set {
                _statusColor = value;
                OnPropertyChanged(nameof(StatusColor));
            }
        }

        public homeView() {
            InitializeComponent();
            DataContext = this;
        }

        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // 拖拽进入事件
        private void UploadArea_DragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0 && IsValidFile(files[0])) {
                    e.Effects = DragDropEffects.Copy;
                    IsDragOver = true;
                }
                else {
                    e.Effects = DragDropEffects.None;
                }
            }
            else {
                e.Effects = DragDropEffects.None;
            }
        }

        // 拖拽离开事件
        private void UploadArea_DragLeave(object sender, DragEventArgs e) {
            IsDragOver = false;
        }

        // 拖拽悬停事件
        private void UploadArea_DragOver(object sender, DragEventArgs e) {
            e.Handled = true;
        }

        // 拖拽放下事件
        private void UploadArea_Drop(object sender, DragEventArgs e) {
            IsDragOver = false;

            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0) {
                    var filePath = files[0];
                    if (IsValidFile(filePath)) {
                        SelectFile(filePath);
                    }
                    else {
                        ShowStatusMessage("不支持的文件类型，请选择 Excel 文件 (.xlsx, .xls)", Brushes.Red);
                    }
                }
            }
        }

        // 点击上传区域事件
        private void UploadArea_Click(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (ShowDefaultPanel) {
                SelectFile_Click(sender, new RoutedEventArgs());
            }
        }

        // 选择文件按钮点击事件
        private void SelectFile_Click(object sender, RoutedEventArgs e) {
            var openFileDialog = new OpenFileDialog {
                Title = "选择 Excel 文件",
                Filter = "Excel 文件|*.xlsx;*.xls|所有文件|*.*",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == true) {
                if (IsValidFile(openFileDialog.FileName)) {
                    SelectFile(openFileDialog.FileName);
                }
                else {
                    ShowStatusMessage("不支持的文件类型，请选择 Excel 文件 (.xlsx, .xls)", Brushes.Red);
                }
            }
        }

        // 开始上传按钮点击事件
        private async void StartUpload_Click(object sender, RoutedEventArgs e) {
            if (string.IsNullOrEmpty(_selectedFilePath)) {
                ShowStatusMessage("请先选择文件", Brushes.Red);
                return;
            }

            await StartUploadProcess();
        }

        // 验证文件类型
        private bool IsValidFile(string filePath) {
            if (string.IsNullOrEmpty(filePath))
                return false;

            var extension = Path.GetExtension(filePath).ToLower();
            return _supportedExtensions.Contains(extension);
        }

        // 选择文件
        private void SelectFile(string filePath) {
            try {
                var fileInfo = new FileInfo(filePath);
                _selectedFilePath = filePath;
                SelectedFileName = fileInfo.Name;
                SelectedFileSize = FormatFileSize(fileInfo.Length);

                ShowDefaultPanel = false;
                ShowFileInfo = true;
                ShowProgress = false;
                ShowStatus = false;

                ShowStatusMessage("文件选择成功", Brushes.Green);
            }
            catch (Exception ex) {
                ShowStatusMessage($"文件选择失败: {ex.Message}", Brushes.Red);
            }
        }

        // 开始上传过程
        private async Task StartUploadProcess() {
            try {
                ShowDefaultPanel = false;
                ShowFileInfo = false;
                ShowProgress = true;
                ShowStatus = false;

                // 模拟上传进度
                for (int i = 0; i <= 100; i += 5) {
                    UploadProgress = i;
                    UploadProgressText = $"{i}% 完成";
                    await Task.Delay(100); // 模拟上传时间
                }

                // 这里可以添加实际的文件处理逻辑
                await ProcessExcelFile(_selectedFilePath);

                // 上传完成
                ShowProgress = false;
                ShowStatusMessage("文件上传成功！", Brushes.Green);

                // 3秒后重置界面
                await Task.Delay(3000);
                ResetUploadArea();
            }
            catch (Exception ex) {
                ShowProgress = false;
                ShowStatusMessage($"上传失败: {ex.Message}", Brushes.Red);
            }
        }

        // 处理Excel文件：解析关键列并入库
        private async Task ProcessExcelFile(string filePath) {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                throw new FileNotFoundException("文件不存在", filePath);

            await Task.Run(() => {
                // EPPlus 8 许可设置（非商业个人用途）
                ExcelPackage.License.SetNonCommercialPersonal("Trae User");

                using var package = new ExcelPackage(new FileInfo(filePath));
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                    throw new InvalidOperationException("Excel 中未找到工作表");

                // 构建表头索引（第一行作为表头）
                var headerRow = 1;
                var endCol = worksheet.Dimension.End.Column;
                var headerIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                for (int col = 1; col <= endCol; col++) {
                    var header = worksheet.Cells[headerRow, col].Text?.Trim();
                    if (!string.IsNullOrEmpty(header) && !headerIndex.ContainsKey(header))
                        headerIndex[header] = col;
                }

                // 需要的列（仅解析实体类中存在的）
                int GetCol(string name) => headerIndex.TryGetValue(name, out var idx) ? idx : -1;
                var colClazz = GetCol("班级");
                var colId = GetCol("学号");
                var colName = GetCol("姓名");
                var colCourse = GetCol("课程名称");
                var colScore = GetCol("成绩");
                var colGrade = GetCol("年级");
                var colMajor = GetCol("专业");
                var colTeacher = GetCol("任课教师");

                // 校验至少存在学号与姓名列
                if (colId < 0 || colName < 0)
                    throw new InvalidOperationException("Excel 缺少必要的列：学号 或 姓名");

                static string ReadCell(ExcelWorksheet ws, int row, int col) {
                    if (col < 0) return string.Empty;
                    return ws.Cells[row, col].Text?.Trim() ?? string.Empty;
                }

                var students = new List<StudentInfo>();
                for (int row = headerRow + 1; row <= worksheet.Dimension.End.Row; row++) {
                    var id = ReadCell(worksheet, row, colId);
                    var name = ReadCell(worksheet, row, colName);
                    if (string.IsNullOrWhiteSpace(id) && string.IsNullOrWhiteSpace(name))
                        continue; // 跳过空行

                    var s = new StudentInfo {
                        Id = id,
                        Name = name,
                        Clazz = ReadCell(worksheet, row, colClazz),
                        Course = ReadCell(worksheet, row, colCourse),
                        Score = ReadCell(worksheet, row, colScore),
                        Grade = ReadCell(worksheet, row, colGrade),
                        Major = ReadCell(worksheet, row, colMajor),
                        TeacherId = ReadCell(worksheet, row, colTeacher)
                    };
                    students.Add(s);
                }

                // 保存到数据库
                StudentService.Initialize();
                StudentService.BulkUpsertStudents(students);

                // 记录日志
                var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 导入 {students.Count} 条学生记录: {Path.GetFileName(filePath)}\n";
                File.AppendAllText("log.txt", logMessage);
            });
        }

        // 显示状态消息
        private void ShowStatusMessage(string message, Brush color) {
            StatusMessage = message;
            StatusColor = color;
            ShowStatus = true;
        }

        // 重置上传区域
        private void ResetUploadArea() {
            ShowDefaultPanel = true;
            ShowFileInfo = false;
            ShowProgress = false;
            ShowStatus = false;
            IsDragOver = false;

            SelectedFileName = "";
            SelectedFileSize = "";
            UploadProgress = 0;
            UploadProgressText = "";
            StatusMessage = "";
            _selectedFilePath = "";
        }

        // 格式化文件大小
        private string FormatFileSize(long bytes) {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1) {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        // 公共方法：外部调用重置
        public void Reset() {
            ResetUploadArea();
        }

        // 公共方法：获取选中的文件路径
        public string GetSelectedFilePath() {
            return _selectedFilePath;
        }

        // 公共事件：文件上传完成
        public event EventHandler<string>? FileUploaded;

        protected virtual void OnFileUploaded(string filePath) {
            FileUploaded?.Invoke(this, filePath);
        }
    }
}