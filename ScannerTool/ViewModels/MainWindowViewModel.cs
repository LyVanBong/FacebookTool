using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Mvvm;
using RestSharp;

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
        private string _search;
        private string _emails;
        private string _phoneNums;
        private List<string> _urls;

        public DelegateCommand RunCommand =>
            _runCommand ?? (_runCommand = new DelegateCommand(() => ExecuteRunCommand()));

        public string Search
        {
            get => _search;
            set => SetProperty(ref _search, value);
        }

        public string Emails
        {
            get => _emails;
            set => SetProperty(ref _emails, value);
        }

        public string PhoneNums
        {
            get => _phoneNums;
            set => SetProperty(ref _phoneNums, value);
        }

        public MainWindowViewModel()
        {

        }

        private async Task<string> RequestSite(string url)
        {
            var client = new RestClient(url);
            var request = new RestRequest();
            request.Method = Method.Get;
            var response = await client.ExecuteGetAsync(request);
            if (response.IsSuccessful && !string.IsNullOrWhiteSpace(response?.Content))
            {
                return response.Content;
            }
            return null;
        }

        private async Task ScanPhoneNum()
        {

        }
        private async Task ScanEmail()
        {

        }
        private async Task ExecuteRunCommand()
        {

            string patter = "http(.*?)\"";
            var str = await RequestSite(Search);
            var matches = Regex.Matches(str, patter);
            if (matches.Any())
            {
                _urls = new List<string>();
                int i = 1;
                foreach (Match m in matches)
                {
                    var s = m.Groups[0].Value;
                    var a = s.TrimEnd('"');
                    Debug.WriteLine(i + ": " + a);
                    i++;
                    _urls.Add(a);
                }
            }
        }
    }
}