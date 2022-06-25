using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace FacebookTool.ViewModels
{
    public class ScanUidWindowViewModel : BindableBase
    {
        private string _url;
        private bool _isRunApp;
        private string _uids = "";

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
                App.Coookie = App.Coookie.Replace(" ", "");

                var driverService = ChromeDriverService.CreateDefaultService();
                driverService.HideCommandPromptWindow = true;
                ChromeOptions option = new ChromeOptions();
                _chromeDriver = new ChromeDriver(driverService, option);
                _chromeDriver.Manage().Window.Size = new Size(250, 844);
                _chromeDriver.Manage().Window.Position = new Point(0, 0);
                _chromeDriver.Navigate().GoToUrl(_facebook);

                var ck = App.Coookie.Split(';', StringSplitOptions.RemoveEmptyEntries);
                var lsCookie = new List<Cookie>();
                foreach (var c in ck)
                {
                    var value = c.Split('=', StringSplitOptions.RemoveEmptyEntries);
                    _chromeDriver.Manage().Cookies.AddCookie(new Cookie(value[0], value[1]));
                }
                _chromeDriver.Navigate().GoToUrl(_facebook);
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
                MessageBox.Show("Quét uid thành công: " + Uids.Split("\n").Length);
            }
            _chromeDriver.Close();
            IsRunApp = false;
        }

        private ChromeDriver _chromeDriver;

        private async Task RequestSite(string url)
        {
            try
            {
                _chromeDriver.Navigate().GoToUrl(url);
                await Task.Delay(1500);
                var content = _chromeDriver.PageSource;
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

        private async Task ScanUid(string content)
        {
            var pattern = @"id=""member_(.*?)""";
            var maches = Regex.Matches(content, pattern);
            if (maches.Any())
            {
                foreach (Match m in maches)
                {
                    var id = m.Groups[1]?.Value;
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        if (!Uids.Contains(id))
                        {
                            Uids = id + "\n" + Uids;
                        }
                    }
                }
            }
        }

        private async Task ScanUrl(string content)
        {
            var regex = @"""m_more_item""><a href=""/(.*?)""";
            var machs = Regex.Match(content, regex);
            var url = machs?.Groups[1]?.Value;
            if (!string.IsNullOrWhiteSpace(url))
            {
                if (!_lsUrl.Contains(url))
                {
                    _lsUrl.Add(_facebook + HttpUtility.HtmlDecode(url));
                }
            }
        }
    }
}