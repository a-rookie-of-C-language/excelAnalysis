using excel.mvvm.views;

namespace excel.mvvm.viewModels;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

public class MainWindowViewModel : INotifyPropertyChanged{
    public ObservableCollection<NavigationItem> NavigationItems { get; } = new();

    private NavigationItem? _selectedItem;

    public NavigationItem? SelectedItem {
        get => _selectedItem;
        set {
            if (_selectedItem == value) return;
            _selectedItem = value;
            OnPropertyChanged();
        }
    }

    public MainWindowViewModel() {
        NavigationItems.Add(new NavigationItem(
            "首页", 
            new homeView(),
            "svg/home.svg"
        ));
        
        NavigationItems.Add(new NavigationItem(
            "学生信息",
            new studentInfo(),
            "svg/student.svg"
        ));
        
        NavigationItems.Add(new NavigationItem(
            "学生成绩分析",
            new StudentGradeAnalysisView(),
            "svg/analysis.svg"
        ));
        
        NavigationItems.Add(new NavigationItem(
            "教师教学分析",
            new TeacherAnalysisView(),
            "svg/teacher.svg"
        ));
        
        NavigationItems.Add(new NavigationItem(
            "挂科统计",
            new FailStatisticsView(),
            "svg/fail.svg"
        ));

        SelectedItem = NavigationItems.Count > 0 ? NavigationItems[0] : null;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public record NavigationItem(string Name, UserControl View, string IconPath);