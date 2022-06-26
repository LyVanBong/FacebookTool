using System;
using Prism.Mvvm;

namespace ZaloTool.Models;

public class PhoneZalo : BindableBase
{
    private string _phoneNumber;
    private string _zaloName;
    private string _status;
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

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public PhoneZalo(string phoneNumber)
    {
        PhoneNumber = phoneNumber;
    }
}