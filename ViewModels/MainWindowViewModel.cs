using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using excel.Views;

namespace excel.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public ObservableCollection<NavigationItem> NavigationItems { get; } = new();

    private NavigationItem? _selectedItem;
    public NavigationItem? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (_selectedItem == value) return;
            _selectedItem = value;
            OnPropertyChanged();
        }
    }

    public MainWindowViewModel()
    {
        // 初始化导航项，后续可轻松扩展
        NavigationItems.Add(new NavigationItem("主界面", new HomeView()));
        NavigationItems.Add(new NavigationItem("学生信息", new StudentInfoView()));
        NavigationItems.Add(new NavigationItem("成绩统计", new GradeStatsView()));
        NavigationItems.Add(new NavigationItem("报告生成", new ReportGeneratorView()));
        NavigationItems.Add(new NavigationItem("历史记录", new HistoryView()));

        SelectedItem = NavigationItems.Count > 0 ? NavigationItems[0] : null;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public record NavigationItem(string Name, UserControl View);