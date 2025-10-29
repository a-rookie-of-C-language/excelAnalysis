using System.Runtime.InteropServices;
using System.Windows.Controls;

namespace excel.mvvm.views{
    /// <summary>
    /// SidebarView.xaml 的交互逻辑
    /// </summary>
    public partial class SidebarView : UserControl{
        private const string IconPath  = "image/image.png";

        public SidebarView() {
            InitializeComponent();
        }

        public string iconPath {
            get => IconPath;
        }

    }
}