using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using excel.mvvm.model;
using excel.Services;

namespace excel.mvvm.views;

public partial class studentInfo : UserControl{
    public ObservableCollection<StudentInfo> Students { get; } = new();

    public studentInfo() {
        InitializeComponent();
        DataContext = this;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e) {
        try {
            StudentService.Initialize();
            var items = StudentService.GetStudents();
            Students.Clear();
            foreach (var s in items) Students.Add(s);
        }
        catch (Exception ex) {
            Console.WriteLine($"加载学生信息失败: {ex.Message}");
        }
    }
}