using Ookii.Dialogs.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace ScannerTool.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Scan Email And Phone Number Tool";

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private DelegateCommand _runCommand;
        private string _urlScanData;
        private ObservableCollection<Item> _emails = new ObservableCollection<Item>();
        private ObservableCollection<Item> _phoneNums = new ObservableCollection<Item>();

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
                            var pathEmail = selectedPaths[0] + "\\email_" + name.Authority.Replace(".", "_") + "_" + DateTime.Now.Ticks + ".txt";
                            var pathPhone = selectedPaths[0] + "\\phone_" + DateTime.Now.Ticks + ".txt";
                            await File.WriteAllTextAsync(pathEmail, string.Join("\n", Emails.Select(e => e.Title)));
                            await File.WriteAllTextAsync(pathPhone, string.Join("\n", PhoneNums.Select(e => e.Title)));
                            Process.Start(pathPhone);
                            Process.Start(pathEmail);
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
                var response = await client.ExecuteGetAsync(request);
                var content = response?.Content;
                if (!string.IsNullOrWhiteSpace(content))
                {
                    _ = ScanEmail(content);
                    _ = ScanPhoneNum(content);
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

        private List<string> _listUrl = new List<string>();
        private int _num;

        private async Task ExecuteRunCommand()
        {
            try
            {
                IsBusy = false;
                Emails.Clear();
                PhoneNums.Clear();
                Logs.Clear();
                if (string.IsNullOrWhiteSpace(UrlScanData))
                {
                    MessageBox.Show("Chưa nhập url cần quét email số điện thoại");
                }
                else
                {
                    UrlScanData = UrlScanData[UrlScanData.Length - 1] == '/' ? UrlScanData : UrlScanData + "/";
                    _listUrl.Add(UrlScanData);
                    while (true)
                    {
                        await RequestSite(_listUrl[_num]);
                        _num++;

                        if (_num == _listUrl.Count) break;
                    }

                    MessageBox.Show("Quét dữ liệu xong");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Notification", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = true;
            }
        }

        private int _numScanUrl = 1;

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