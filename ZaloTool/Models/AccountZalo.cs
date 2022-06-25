using System;
using Prism.Mvvm;

namespace ZaloTool.Models;

public class AccountZalo : BindableBase
{
    private string _running;
    private string _status;
    private int _friendNumber;
    private string _zaloName;
    private string _phoneNumber;
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string PhoneNumber
    {
        get => _phoneNumber;
        set => SetProperty(ref _phoneNumber, value);
    }

    public string ZaloName
    {
        get => _zaloName;
        set => SetProperty(ref _zaloName, value);
    }

    public int FriendNumber
    {
        get => _friendNumber;
        set => SetProperty(ref _friendNumber, value);
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public string Running
    {
        get => _running;
        set => SetProperty(ref _running, value);
    }

    public string Cookie { get; set; }
    public string PathChromeProfile { get; set; }
    /// <summary>
    /// Kiểm tra xem đã nhập đầy đủ thông tin zalo chưa
    /// </summary>
    /// <returns></returns>
    public bool CheckNullAccountZalo()
    {
        if (string.IsNullOrWhiteSpace(PhoneNumber)) return false;
        if (string.IsNullOrWhiteSpace(ZaloName)) return false;
        return true;
    }
    /// <summary>
    /// tạo đường dẫn chrome profile cho từng tài khoản zalo
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public string CreateNameFolder(string path)
    {
        return path + "\\" + ZaloName + "_" + PhoneNumber;
    }
}