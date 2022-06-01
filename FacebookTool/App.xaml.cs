using FacebookTool.Views;
using Prism.Ioc;
using System.Windows;

namespace FacebookTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {

        public static string Coookie { get; set; }
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
        }
    }
}