using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Prism.Commands;
using Prism.Mvvm;
using RestSharp;

namespace FacebookTool.ViewModels
{
    public class ScanUidWindowViewModel : BindableBase
    {
        private string _url;
        private bool _isRunApp;
        private string _uids;

        public string Url
        {
            get => _url;
            set => SetProperty(ref _url, value);
        }

        public bool IsRunApp
        {
            get => _isRunApp;
            set => SetProperty(ref _isRunApp, value);
        }

        public string Uids
        {
            get => _uids;
            set => SetProperty(ref _uids, value);
        }
        private DelegateCommand _scanUidCommand;

        public DelegateCommand ScanUidCommand =>
            _scanUidCommand ?? (_scanUidCommand = new DelegateCommand(() => ExecuteScanUidCommand()));

        public ScanUidWindowViewModel()
        {
        }

        private string _facebook = "https://d.facebook.com/";
        private string _controllerGroup = "browse/group/members/?id=";
        private List<string> _lsUrl;
        private int _num;
        private async Task ExecuteScanUidCommand()
        {
            if (IsRunApp) return;
            IsRunApp = true;

            if (Url == null)
            {
                MessageBox.Show("Cần bổ sung đường dẫn cần quét uid");
            }
            else
            {
                _lsUrl = new List<string>() { _facebook + _controllerGroup + Url };
                _num = 0;
                while (true)
                {
                    await Task.Delay(1000);
                    await RequestSite(_lsUrl[_num]);
                    _num++;
                    if (_num == _lsUrl.Count)
                    {
                        break;
                    }
                }
                MessageBox.Show("Quét uid thành công");
            }

            IsRunApp = false;
        }
        private async Task RequestSite(string url)
        {
            try
            {
                var client = new RestClient(url);
                var request = new RestRequest();
                request.Method = Method.Get;
                request.AddHeader("Cookie", App.Coookie);
                var response = await client.ExecuteAsync(request);
                var content = response?.Content;
                if (!string.IsNullOrWhiteSpace(content))
                {
                    _ = ScanUid(content);
                    if (content.Contains("m_more_item"))
                    {
                        await ScanUrl(content);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private async Task ScanUid(string? content)
        {
            throw new NotImplementedException();
        }

        private async Task ScanUrl(string? content)
        {
            var regex = "m_more_item\" >< a href = \"/(.*?)\"";
            var machs = Regex.Match(content, regex);
            var url = machs?.Groups[1]?.Value;
            if (!string.IsNullOrWhiteSpace(url))
            {
                _lsUrl.Add(_facebook + url);
            }
        }
    }
}