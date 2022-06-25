using Prism.Ioc;
using System.Windows;
using ZaloTool.Services;
using ZaloTool.Views;

namespace ZaloTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IChromeBrowserService, ChromeBrowserService>();
        }
    }
}