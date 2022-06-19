using Ookii.Dialogs.Wpf;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Prism.Commands;
using Prism.Mvvm;
using RestSharp;
using ScannerTool.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace ScannerTool.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Scan Email And Phone Number Tool";
        private string _facebook = "https://www.facebook.com/";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private DelegateCommand _runCommand;
        private string _urlScanData ;
        private ObservableCollection<Item> _emails = new ObservableCollection<Item>();
        private ObservableCollection<Item> _phoneNums = new ObservableCollection<Item>();
        private int _numScanUrl = 1;
        private string _textScan = "Scan";
        private List<string> _listUrl = new List<string>();
        private int _num;
        private string _cookie ;
        private ChromeDriver _chromeDriver;
        public string Cookie
        {
            get => _cookie;
            set => SetProperty(ref _cookie, value);
        }

        public string TextScan
        {
            get => _textScan;
            set => SetProperty(ref _textScan, value);
        }

        public DelegateCommand RunCommand =>
            _runCommand ?? (_runCommand = new DelegateCommand(() => ExecuteRunCommand()));

        public ObservableCollection<Item> Logs
        {
            get => _logs;
            set => SetProperty(ref _logs, value);
        }

        public string UrlScanData
        {
            get => _urlScanData;
            set => SetProperty(ref _urlScanData, value);
        }

        public ObservableCollection<Item> Emails
        {
            get => _emails;
            set => SetProperty(ref _emails, value);
        }

        public ObservableCollection<Item> PhoneNums
        {
            get => _phoneNums;
            set => SetProperty(ref _phoneNums, value);
        }

        private bool _isBusy = true;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private DelegateCommand _exportDataCommand;

        public DelegateCommand ExportDataCommand =>
            _exportDataCommand ?? (_exportDataCommand = new DelegateCommand(() => ExecuteExportDataCommand()));

        private ObservableCollection<Item> _logs = new ObservableCollection<Item>();
        public TypeFunctionModel TypeFunctionModel { get; set; } = new TypeFunctionModel();
        public MainWindowViewModel()
        {
        }

        private async Task ExecuteExportDataCommand()
        {
            try
            {
                if (!IsBusy)
                {
                    MessageBox.Show("Tool đang quét dữ liệu không thể xuất file");
                    return;
                }

                if (Emails.Any() || PhoneNums.Any())
                {
                    var dialog = new VistaFolderBrowserDialog();
                    dialog.Multiselect = false;
                    dialog.Description = "Please select a folder.";
                    dialog.UseDescriptionForTitle = true;

                    if (!VistaFolderBrowserDialog.IsVistaFolderDialogSupported)
                    {
                        MessageBox.Show(
                            "Because you are not using Windows Vista or later, the regular folder browser dialog will be used. Please use Windows Vista to see the new dialog.",
                            "Sample folder browser dialog");
                    }

                    if (dialog.ShowDialog() == true)
                    {
                        var selectedPaths = dialog.SelectedPaths;
                        if (selectedPaths != null && selectedPaths[0] != null)
                        {
                            var name = new Uri(UrlScanData);

                            if (Emails.Any())
                            {
                                var pathEmail = selectedPaths[0] + "\\email_" + name.Authority.Replace(".", "_") + "_" + DateTime.Now.Ticks + ".txt";
                                await File.WriteAllTextAsync(pathEmail, string.Join("\n", Emails.Select(e => e.Title)));
                                //Process.Start(pathEmail, @"C:\Windows\win.ini");
                            }

                            if (PhoneNums.Any())
                            {
                                var pathPhone = selectedPaths[0] + "\\phone_" + DateTime.Now.Ticks + ".txt";
                                await File.WriteAllTextAsync(pathPhone, string.Join("\n", PhoneNums.Select(e => e.Title)));
                                //Process.Start(pathPhone, @"C:\Windows\win.ini");
                            }
                            MessageBox.Show("Save file successful");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Data empty");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                MessageBox.Show(e.Message, "Notification", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RequestSite(string url)
        {
            try
            {
                var client = new RestClient(url);
                var request = new RestRequest();
                request.Method = Method.Get;
                if (!string.IsNullOrWhiteSpace(Cookie))
                    request.AddHeader("Cookie", Cookie);
                var response = await client.ExecuteGetAsync(request);
                var content = response?.Content;
                if (!string.IsNullOrWhiteSpace(content))
                {
                    _ = ScanEmail(content);
                    _ = ScanPhoneNum(content);
                    if (TypeFunctionModel.CheckTypeFunction() == 1)
                        await ScanUrl(content);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        /// <summary>
        /// Sacn phone
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private Task ScanPhoneNum(string content)
        {
            if (content != null)
            {
                var patterPhone = "(84|0[0-9])+([0-9]{8})";
                var matche = Regex.Matches(content, patterPhone);
                if (matche.Any())
                {
                    foreach (Match o in matche)
                    {
                        var s = o.Groups[0].Value;
                        if (!string.IsNullOrWhiteSpace(s))
                        {
                            if (PhoneNums.Count(x => x.Title == s) == 0)
                                PhoneNums.Insert(0, new Item(s));
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// scan email
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private Task ScanEmail(string content)
        {
            if (content != null)
            {
                var patterEmail = "[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}";
                var matche = Regex.Matches(content, patterEmail);
                if (matche.Any())
                {
                    foreach (Match o in matche)
                    {
                        var s = o.Groups[0].Value;
                        if (!string.IsNullOrWhiteSpace(s))
                        {
                            if (Emails.Count(x => x.Title == s) == 0)
                                Emails.Insert(0, new Item(s));
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }

        private CancellationTokenSource _cancel = new CancellationTokenSource();
        private async Task ExecuteRunCommand()
        {
            try
            {
                if (!IsBusy)
                {
                    _cancel.Cancel();
                    return;
                }
                IsBusy = false;

                if (string.IsNullOrWhiteSpace(UrlScanData))
                {
                    MessageBox.Show("Chưa nhập url cần quét email số điện thoại");
                }
                else
                {
                    UrlScanData = UrlScanData[UrlScanData.Length - 1] == '/' ? UrlScanData : UrlScanData + "/";
                    TextScan = "Cancel";
                    Emails.Clear();
                    PhoneNums.Clear();
                    Logs.Clear();
                    switch (TypeFunctionModel.CheckTypeFunction())
                    {
                        case 1:
                            await ScanDataWebsite();
                            MessageBox.Show("Quét dữ liệu xong");
                            break;
                        case 2:
                            MessageBox.Show("Quét dữ liệu xong");
                            break;
                        case 3:
                            await ScanDatGroup();
                            break;
                        default:
                            MessageBox.Show("Xẩy ra lỗi vui lòng thử lại !");
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Notification", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            TextScan = "Scan";
            IsBusy = true;
        }

        private async Task ScanDatGroup()
        {
            if (Cookie == null)
            {
                MessageBox.Show("Tính năng này cần đăng nhập facebook! vui long cung cấp cookie facebook!");
                return;
            }

            Cookie = Cookie.Replace(" ", "");

            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            ChromeOptions option = new ChromeOptions();
            _chromeDriver = new ChromeDriver(driverService, option);
            _chromeDriver.Manage().Window.Size = new Size(250, 844);
            _chromeDriver.Manage().Window.Position = new Point(0, 0);
            _chromeDriver.Navigate().GoToUrl(_facebook);

            var ck = Cookie.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var c in ck)
            {
                var value = c.Split('=', StringSplitOptions.RemoveEmptyEntries);
                _chromeDriver.Manage().Cookies.AddCookie(new Cookie(value[0], value[1]));
            }

            _chromeDriver.Navigate().GoToUrl(UrlScanData + "members");
            var source = "<html";
            while (true)
            {
                if (_cancel.IsCancellationRequested) break;
                await Task.Delay(Random.Shared.Next(4000, 5000));
                var pageSource = _chromeDriver.PageSource;
                //new Actions(_chromeDriver).SendKeys(keysToSend: Keys.PageDown).Build().Perform();

                var js = (IJavaScriptExecutor)_chromeDriver;
                js.ExecuteScript("window.scrollBy(0, 10000)");

                if (source == pageSource)
                {
                    break;
                }

                await ScanUid(pageSource.Replace(source, ""));
                source = pageSource;
            }

            if (_lsUid.Count > 0)
                foreach (var u in _lsUid)
                {
                    if (_cancel.IsCancellationRequested) break;
                    var about = $"https://www.facebook.com/{u}/about";
                    _chromeDriver.Navigate().GoToUrl(about);
                    await Task.Delay(1000);
                    var pageSource = _chromeDriver.PageSource;
                    if (pageSource != null)
                    {
                        _ = ScanEmail(pageSource);
                        _ = ScanPhoneNum(pageSource);
                    }

                    Logs.Insert(0, new Item($"{_numScanUrl} : [{DateTime.Now.ToString("G")}] " + about));
                    _numScanUrl++;
                }

            MessageBox.Show("Quét dữ liệu xong");
            _chromeDriver.Close();
            _chromeDriver.Quit();
        }

        private List<string> _lsUid = new List<string>();
        private Task ScanUid(string content)
        {
            _ = ScanEmail(content);
            _ = ScanPhoneNum(content);
            var pt = @"/user/(.*?)/";
            var machs = Regex.Matches(content, pt);
            if (machs != null & machs.Count > 0)
            {
                foreach (Match mach in machs)
                {
                    var uid = mach?.Groups["1"]?.Value;
                    if (uid != null && !_lsUid.Contains(uid))
                    {
                        //var about = $"https://www.facebook.com/{uid}/about";

                        _lsUid.Add(uid);
                        Logs.Insert(0, new Item($"{_numScanUrl} : [{DateTime.Now.ToString("G")}] " + uid));
                        _numScanUrl++;

                        //var html = RequestSite(about);
                    }
                }
            }
            return Task.CompletedTask;
        }

        private async Task ScanDataWebsite()
        {
            _listUrl.Add(UrlScanData);
            while (true)
            {
                if (_cancel.IsCancellationRequested) break;
                await RequestSite(_listUrl[_num]);
                _num++;

                if (_num == _listUrl.Count) break;
            }
        }

        /// <summary>
        /// sacn url
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private Task ScanUrl(string content)
        {
            var host = (new Uri(UrlScanData)).Authority;

            string patter = "http(s*)://(.*?)\"";
            var matches = Regex.Matches(content, patter);
            if (matches.Any())
            {
                foreach (Match m in matches)
                {
                    var s = m.Groups[0].Value;
                    var a = s.TrimEnd('"');

                    if (a.Contains(host))
                        if (!_listUrl.Contains(a))
                            _listUrl.Add(a);

                    Logs.Insert(0, new Item($"{_numScanUrl} : [{DateTime.Now.ToString("G")}] " + a));
                    _numScanUrl++;
                }
            }

            var patterHref = "href=\"/(.*?)\"";
            var matchesHref = Regex.Matches(content, patterHref);
            if (matchesHref.Any())
            {
                foreach (Match mh in matchesHref)
                {
                    var s = mh.Groups[1].Value;
                    var a = s.TrimEnd('"');
                    var u = a.Contains("html") ? a : UrlScanData + a;

                    if (u.Contains(host))
                        if (!_listUrl.Contains(u))
                            _listUrl.Add(u);

                    Logs.Insert(0, new Item($"{_numScanUrl} : [{DateTime.Now.ToString("G")}] " + u));
                    _numScanUrl++;
                }
            }
            return Task.CompletedTask;
        }
    }

    public class Item : BindableBase
    {
        private string _title;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public Item(string title)
        {
            _title = title;
        }
    }
}