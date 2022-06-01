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

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var main = new MainWindow();
            main.Show();
            this.Close();
        }
    }
}