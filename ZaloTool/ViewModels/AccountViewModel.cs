using Microsoft.EntityFrameworkCore;
using Prism.Commands;
using Prism.Mvvm;
using RestSharp;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ZaloTool.Database;
using ZaloTool.Models;
using ZaloTool.Services;

namespace ZaloTool.ViewModels
{
    public class AccountViewModel : BindableBase
    {
        private string _pathChromeProfileDefault = Directory.GetCurrentDirectory() + "/ChromeProfile";
        private IChromeBrowserService _chromeBrowserService;
        private ZaloToolContext _dbZalo = new ZaloToolContext();
        private DelegateCommand _loginZaloCommand;
        private bool _isBusy = true;
        private ObservableCollection<AccountZalo> _accountZalos = new ObservableCollection<AccountZalo>();

        public DelegateCommand LoginZaloCommand =>
            _loginZaloCommand ?? (_loginZaloCommand = new DelegateCommand(() => ExecuteLoginZaloCommand()));

        public ObservableCollection<AccountZalo> AccountZalos
        {
            get => _accountZalos;
            set => SetProperty(ref _accountZalos, value);
        }

        public AccountZalo AccountZalo { get; set; } = new AccountZalo();

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public AccountViewModel(IChromeBrowserService chromeBrowserService)
        {
            _chromeBrowserService = chromeBrowserService;
            _ = CreateDefaultData();
        }
        /// <summary>
        /// Khởi tạo dữ liệu ban đầu
        /// </summary>
        /// <returns></returns>
        private async Task CreateDefaultData()
        {
            try
            {
                _dbZalo.Database.EnsureCreated();
                _dbZalo.AccountZalos.Load();
                if (_dbZalo.AccountZalos.Any())
                {
                    AccountZalos.AddRange(_dbZalo.AccountZalos);
                }

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
                        //ZipFile.ExtractToDirectory(_pathChromeProfileDefault + "\\ChromeProfileDefault.zip", _pathChromeProfileDefault + "\\ChromeProfileDefault", true);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error");
            }

            IsBusy = true;
        }

        private async Task ExecuteLoginZaloCommand()
        {
            IsBusy = false;
            try
            {
                if (AccountZalo.CheckNullAccountZalo())
                {
                    _dbZalo.AccountZalos.Load();
                    if (_dbZalo.AccountZalos.Any(x => x.PhoneNumber == AccountZalo.PhoneNumber))
                    {
                        MessageBox.Show("Tài khoản zalo này đã tồn tại");
                    }
                    else
                    {
                        AccountZalo.PathChromeProfile = AccountZalo.CreateNameFolder(_pathChromeProfileDefault);
                        ZipFile.ExtractToDirectory(_pathChromeProfileDefault + "\\ChromeProfileDefault.zip", AccountZalo.PathChromeProfile, Encoding.UTF8, true);
                        await _chromeBrowserService.LoginZalo(AccountZalo.PathChromeProfile);
                        _dbZalo.Add(AccountZalo);
                        _dbZalo.SaveChanges();
                        AccountZalos.Add(AccountZalo);
                        AccountZalo = new AccountZalo();
                        MessageBox.Show("Đăng nhập thành công");
                    }
                }
                else
                {
                    MessageBox.Show("Số điện thoại hoặc tên zalo bị trống");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error");
            }
            IsBusy = true;
        }
    }
}