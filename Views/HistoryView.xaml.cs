using System.Collections.Generic;
using System.Windows.Controls;
using excel.DAO;
using excel.entity;

namespace excel.Views;

public partial class HistoryView : UserControl {
    public HistoryView() {
        InitializeComponent();
        this.Loaded += (_, __) => LoadHistory();
    }

    private void LoadHistory() {
        var dao = new ImportHistoryDao();
        List<ImportHistory> list;
        try {
            list = dao.GetAll();
        } catch {
            list = new List<ImportHistory>();
        }
        HistoryGrid.ItemsSource = list;
    }
}