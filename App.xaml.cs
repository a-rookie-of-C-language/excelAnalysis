using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using excel.DAO;
using excel.ViewModels;

namespace excel;

public partial class App : Application {
    protected override void OnStartup(StartupEventArgs e) {
        base.OnStartup(e);
        // 初始化数据库结构
        DbInitializer.Initialize();

        // 如果已有历史记录，加载学生数据并默认跳转到学生信息
        TryLoadExistingDataAndNavigate();
    }

    private static void TryLoadExistingDataAndNavigate() {
        try {
            // 优先使用最新导入历史对应的分表
            var hisDao = new ImportHistoryDao();
            var latest = hisDao.GetLatest();
            var students = new List<excel.entity.Student>();
            if (latest != null && !string.IsNullOrWhiteSpace(latest.TableName)) {
                using var conn = excel.DAO.Connection.CreateConnection();
                conn.Open();
                using var cmd = new Microsoft.Data.Sqlite.SqliteCommand($"SELECT id, name, class, college, major, grade FROM {latest.TableName}", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read()) {
                    students.Add(new excel.entity.Student {
                        Id = reader.GetString(reader.GetOrdinal("id")),
                        Name = reader.GetString(reader.GetOrdinal("name")),
                        Class = reader.IsDBNull(reader.GetOrdinal("class")) ? string.Empty : reader.GetString(reader.GetOrdinal("class")),
                        College = reader.IsDBNull(reader.GetOrdinal("college")) ? string.Empty : reader.GetString(reader.GetOrdinal("college")),
                        Major = reader.IsDBNull(reader.GetOrdinal("major")) ? string.Empty : reader.GetString(reader.GetOrdinal("major")),
                        Grade = reader.IsDBNull(reader.GetOrdinal("grade")) ? string.Empty : reader.GetString(reader.GetOrdinal("grade"))
                    });
                }
                conn.Close();
            } else {
                var exec = new SqlExec();
                students = exec.GetAllStudents();
            }
            if (students.Count > 0) {
                foreach (var s in students) StudentState.Students.Add(s);
                // 找到主窗口并设置默认导航到学生信息
                Current.Dispatcher.Invoke(() => {
                    var main = Current.MainWindow;
                    if (main?.DataContext is MainWindowViewModel vm) {
                        var target = vm.NavigationItems.FirstOrDefault(n => n.Name == "学生信息");
                        if (target != null) vm.SelectedItem = target;
                    }
                });
            }
        }
        catch {
            // 忽略加载异常，保持应用可用
        }
    }
}