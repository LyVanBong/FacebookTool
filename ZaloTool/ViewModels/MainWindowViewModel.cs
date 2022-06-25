using Prism.Mvvm;

namespace ZaloTool.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Zalo Marketing Online";

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public MainWindowViewModel()
        {
           
        }
    }
}