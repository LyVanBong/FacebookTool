using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace FacebookTool.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void UIElement_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (bCookie?.Text == null)
            {
                MessageBox.Show("Bạn cần có cookie trước khi quét uid");
            }
            else
            {
                App.Coookie = bCookie.Text;
                var scanUidWindow = new ScanUidWindow();
                scanUidWindow.Show();
                this.Close();
            }
        }
    }
}