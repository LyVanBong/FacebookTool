using FacebookTool.Helpers;
using FacebookTool.Models;
using OpenQA.Selenium.Chrome;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace FacebookTool.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = @"Sent Message Facebook Tool";
        private string _cookies;
        private string _uids;
        private string _message;
        private double _timeDelays = 1;
        private bool _isRunApp;
        private ObservableCollection<LoggerModel> _logs = new ObservableCollection<LoggerModel>();
        private List<Facebooks> _facebooks;

        public string StrCookie
        {
            get => _strCookie;
            set => SetProperty(ref _strCookie, value);
        }

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        /// <summary>
        /// Danh sách cookie tài khoản facebook
        /// </summary>
        public string Cookies
        {
            get => _cookies;
            set => SetProperty(ref _cookies, value);
        }

        /// <summary>
        /// Id tài khoản facebook
        /// </summary>
        public string Uids
        {
            get => _uids;
            set => SetProperty(ref _uids, value);
        }

        /// <summary>
        /// Id các bài viết
        /// </summary>
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        /// <summary>
        /// Thời gian dừng giữa các lần chạy
        /// </summary>
        public double TimeDelays
        {
            get => _timeDelays;
            set => SetProperty(ref _timeDelays, value);
        }

        /// <summary>
        /// Trạng thái ứng dụng có đang chạy không
        /// </summary>
        public bool IsRunApp
        {
            get => _isRunApp;
            set => SetProperty(ref _isRunApp, value);
        }

        /// <summary>
        /// Lấy cookie
        /// </summary>
        public ICommand GetCookieCommand { get; private set; }

        /// <summary>
        /// Chạy ứng dụng
        /// </summary>
        public ICommand RunAppCommand { get; private set; }

        public ObservableCollection<LoggerModel> Logs
        {
            get => _logs;
            set => SetProperty(ref _logs, value);
        }

        private BackgroundWorker _backgroundWorker;
        private string _strCookie = "Đăng nhập facebook";

        public MainWindowViewModel()
        {
            GetCookieCommand = new DelegateCommand(async () => await GetCookieCommandExcute());
            RunAppCommand = new DelegateCommand(async () => await RunAppCommandExcute());
            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.WorkerSupportsCancellation = true;
            _backgroundWorker.DoWork += worker_DoWork;
            _backgroundWorker.ProgressChanged += worker_ProgressChanged;
            _backgroundWorker.RunWorkerCompleted += worker_RunWorkerCompleted;
        }

        private void worker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
        }

        private void worker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            var progressPercentage = e.ProgressPercentage;
            var obj = e.UserState as Facebooks;
            if (obj != null)
            {
                if (e.ProgressPercentage == 1)
                {
                    Logs.Add(new LoggerModel(DateTime.Now.ToString("G"), obj.id, false, "Gửi tin nhắn thành công"));
                }
                else if (e.ProgressPercentage == 0)
                {
                    Logs.Add(new LoggerModel(DateTime.Now.ToString("G"), obj.id, false, "Gửi tin nhắn lỗi"));
                }
            }
            else
            {
                Logs.Add(new LoggerModel(DateTime.Now.ToString("G"), "", false, "Lỗi phát sinh"));
            }
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (_facebooks.Any())
            {
                var data = _facebooks.FirstOrDefault(x => !x.Status);
                if (data != null)
                {
                    data.Status = true;
                    var para = FacebookHelper.GetParaFacebook(data).Result;
                    if (para != null)
                    {
                        if (FacebookHelper.SendMessage(para).Result)
                        {
                            (sender as BackgroundWorker).ReportProgress(1, para);
                        }
                        else
                        {
                            (sender as BackgroundWorker).ReportProgress(0, para);
                        }
                    }
                }
                else
                {
                    e.Result = true;
                    _backgroundWorker.CancelAsync();
                    IsRunApp = !IsRunApp;
                    Uids = null;
                }
            }
        }

        private async Task RunAppCommandExcute()
        {
            IsRunApp = !IsRunApp;
            _facebooks = new List<Facebooks>();

            if (string.IsNullOrEmpty(Cookies) || string.IsNullOrEmpty(Uids) || string.IsNullOrEmpty(Message))
            {
                MessageBox.Show("Data not found", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                Logs.Add(new LoggerModel(DateTime.Now.ToString("G"), "", false, "Bắt đầu gửi tin nhăn spam"));
                var lsUid = Uids.Split('\n');
                var lsCookie = Cookies.Split('\n');
                if (lsUid.Any() && lsCookie.Any())
                {
                    int countCookie = lsCookie.Length;
                    int num = countCookie;
                    foreach (var u in lsUid)
                    {
                        num--;

                        _facebooks.Add(new Facebooks()
                        {
                            Cookie = lsCookie[num],
                            id = u,
                            body = Message,
                        });

                        if (num == 0)
                        {
                            num = countCookie;
                        }
                    }
                    // thực hiện gửi tin nhắn ở đây
                    if (_facebooks.Any())
                    {
                        foreach (var facebook in _facebooks)
                        {
                            var para = await FacebookHelper.GetParaFacebook(facebook);
                            if (para != null)
                            {
                                if (await FacebookHelper.SendMessage(facebook))
                                {
                                    Logs.Add(new LoggerModel(DateTime.Now.ToString("G"), facebook.id, false, "Gửi tin nhắn thành công"));
                                }
                                else
                                {
                                    Logs.Add(new LoggerModel(DateTime.Now.ToString("G"), facebook.id, false, "Gửi tin nhắn thành công"));
                                }
                            }
                            else
                            {
                                Logs.Add(new LoggerModel(DateTime.Now.ToString("G"), "", false, "Lỗi phát sinh"));
                            }
                            Logs.Add(new LoggerModel(DateTime.Now.ToString("G"), "", false, "Sessufull"));
                            await Task.Delay(TimeSpan.FromMinutes(TimeDelays));
                        }
                        IsRunApp = !IsRunApp;
                        Uids = null;
                    }
                }
                else
                {
                    MessageBox.Show("Data not found", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private ChromeDriver driver;
        private bool _isGetCookie;

        public bool IsGetCookie
        {
            get => _isGetCookie;
            set => SetProperty(ref _isGetCookie, value);
        }

        private async Task GetCookieCommandExcute()
        {
            if (IsGetCookie)
            {
                var cookies = driver.Manage().Cookies.AllCookies;
                if (cookies.Any())
                {
                    IsGetCookie = false;
                    string ck = string.Empty;
                    foreach (var c in cookies)
                    {
                        if (string.IsNullOrEmpty(ck)) ck = c.Name + "=" + c.Value;
                        else ck += ";" + c.Name + "=" + c.Value;
                    }

                    if (string.IsNullOrWhiteSpace(Cookies)) Cookies = ck;
                    else Cookies += "\n" + ck;
                    StrCookie = "Đăng nhập facebook";
                }
                driver.Quit();
            }
            else
            {
                IsGetCookie = true;
                Random rd = new Random();
                var driverService = ChromeDriverService.CreateDefaultService();
                driverService.HideCommandPromptWindow = true;
                ChromeOptions option = new ChromeOptions();
                driver = new ChromeDriver(driverService, option);
                driver.Manage().Window.Size = new Size(250, 844);
                driver.Manage().Window.Position = new Point(0, 0);
                driver.Navigate().GoToUrl("https://m.facebook.com/");
                StrCookie = "Lấy cookie";
            }
        }
    }
}