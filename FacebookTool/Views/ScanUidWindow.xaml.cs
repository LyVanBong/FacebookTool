using System.ComponentModel;
using System.Windows;

namespace FacebookTool.Views
{
    /// <summary>
    /// Interaction logic for ScanUidWindow.xaml
    /// </summary>
    public partial class ScanUidWindow : Window
    {
        public ScanUidWindow()
        {
            InitializeComponent();
        }

        private void ScanUidWindow_OnClosing(object sender, CancelEventArgs e)
        {
            MainWindow main = new MainWindow();
            main.Show();
        }
    }
}