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
using System.Net;
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
        private List<string> _urls;

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
        private int _nums;
        private bool _isBusy = true;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }
        private DelegateCommand _exportDataCommand;
        public DelegateCommand ExportDataCommand =>
            _exportDataCommand ?? (_exportDataCommand = new DelegateCommand(() => ExecuteExportDataCommand()));

        private List<string> _requestUrl = new List<string>();
        private List<string> _lsEmail = new List<string>();
        private List<string> _lsPhone = new List<string>();
        private List<string> _lsHtml;
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
        private async Task<string> RequestSite(string url)
        {
            if (url != null)
            {
                if (!_requestUrl.Contains(url))
                {
                    _requestUrl.Add(url);
                    var client = new RestClient(url);
                    var request = new RestRequest();
                    request.Method = Method.Get;
                    var response = await client.ExecuteGetAsync(request);
                    if (response.IsSuccessful && !string.IsNullOrWhiteSpace(response?.Content))
                    {
                        _ = ScanEmail(response.Content);
                        _ = ScanPhoneNum(response.Content);
                        _ = ScanUrl(response.Content);
                        return response.Content;
                    }
                }
            }

            return null;
        }

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
                            if (!_lsPhone.Contains(s))
                            {
                                _lsPhone.Add(s);
                                PhoneNums.Add(new Item(s));
                            }
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }

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
                            if (!_lsEmail.Contains(s))
                            {
                                _lsEmail.Add(s);
                                Emails.Add(new Item(s));
                            }
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }
        private async Task ExecuteRunCommand()
        {
            try
            {
                IsBusy = false;
                _nums = 1;
                Emails.Clear();
                PhoneNums.Clear();
                Logs.Clear();
                var time = new Stopwatch();
                time.Start();
                var htmls = await GetHtml();
                if (htmls != null)
                {
                    foreach (var html in htmls)
                    {
                        var u = await ScanUrl(html);
                        if (u.Any())
                        {
                            _ = Task.WhenAny(u.Select(x => RequestSite(x)));
                            await Task.Delay(3000);
                        }
                    }
                }
                time.Stop();
                MessageBox.Show("Scan data done: " + time.ElapsedMilliseconds / 1000 + " (S)", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                MessageBox.Show(e.Message, "Notification", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = true;
            }
        }

        private async Task<List<string>> GetHtml()
        {
            var htmls = new List<string>();
            htmls.Add(await RequestSite(UrlScanData));
            var urls = (await ScanUrl(htmls[0])).Distinct().ToList();
            var num = 1;
            foreach (var url in urls)
            {
                htmls.Add(await RequestSite(url));
                num++;
                Logs.Insert(0, new Item($"[{DateTime.Now.ToString("G")}] " + url));
            }
            return htmls;
        }

        private Task<List<string>> ScanUrl(string content)
        {
            var urls = new List<string>();
            if (content != null)
            {
                string patter = "http(s*)://(.*?)\"";

                var matches = Regex.Matches(content, patter);
                if (matches.Any())
                {

                    foreach (Match m in matches)
                    {
                        var s = m.Groups[0].Value;
                        var a = s.TrimEnd('"');
                        _nums++;
                        if (!_requestUrl.Contains(a))
                        {
                            urls.Add(a);
                        }
                        Logs.Insert(0, new Item($"[{DateTime.Now.ToString("G")}] " + a));
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
                        _nums++;
                        var u = a.Contains("html") ? a : UrlScanData + a;
                        if (!_requestUrl.Contains(a))
                        {
                            urls.Add(u);
                        }
                        Logs.Insert(0, new Item($"[{DateTime.Now.ToString("G")}] " + u));
                    }
                }
                return Task.FromResult(urls.Distinct().ToList());
            }

            return Task.FromResult(urls);
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