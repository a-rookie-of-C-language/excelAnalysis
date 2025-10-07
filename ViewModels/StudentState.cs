namespace excel.ViewModels;

using System.Collections.ObjectModel;
using excel.entity;

public static class StudentState {
    public static ObservableCollection<Student> Students { get; } = new();
}