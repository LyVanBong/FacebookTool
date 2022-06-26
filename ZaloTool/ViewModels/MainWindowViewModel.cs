using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Prism.Mvvm;
using RestSharp;

namespace ZaloTool.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Zalo Marketing Online";
        private string _pathChromeProfileDefault = Directory.GetCurrentDirectory() + "\\ChromeProfile";
        private bool _isBusy;

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public MainWindowViewModel()
        {
            _ = CreateDefaultData();
        }

        private async Task CreateDefaultData()
        {

            try
            {
                if (!Directory.Exists(_pathChromeProfileDefault))
                {
                    IsBusy = false;
                    Directory.CreateDirectory(_pathChromeProfileDefault);
                    var client = new RestClient();
                    var stream = await client.DownloadDataAsync(new RestRequest(
                        "https://drive.google.com/u/2/uc?id=1pDd1N3VqO_f-UuC-73h8NOsWEjqhV5FS&export=download&confirm=t"));
                    if (stream != null)
                    {
                        await File.WriteAllBytesAsync(_pathChromeProfileDefault + "\\ChromeProfileDefault.zip", stream);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error: " + e.Message);
            }

            IsBusy = true;
        }
    }
}