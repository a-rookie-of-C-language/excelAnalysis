using System.Collections.ObjectModel;
using System.Windows.Controls;
using excel.entity;
using excel.ViewModels;

namespace excel.Views;

public partial class StudentInfoView : UserControl{
    private ObservableCollection<Student> Students { get; } = StudentState.Students;

    public StudentInfoView() {
        InitializeComponent();

        // 示例数据用于UI预览
        StudentGrid.ItemsSource = Students;
    }
}

