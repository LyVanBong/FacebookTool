using Prism.Mvvm;

namespace ZaloTool.Models;

public class SettingSendMessagePhoneNumber : BindableBase
{
    private bool _isAutoAddFriend = true;
    public bool IsAutoAddFriend
    {
        get { return _isAutoAddFriend; }
        set { SetProperty(ref _isAutoAddFriend, value); }
    }
    private int _amountMessage = 50;
    public int AmountMessage
    {
        get { return _amountMessage; }
        set { SetProperty(ref _amountMessage, value); }
    }
    private int _timeSleep = 60;
    public int TimeSleep
    {
        get { return _timeSleep; }
        set { SetProperty(ref _timeSleep, value); }
    }
    private string _pathMediaMessage;
    public string PathMediaMessage
    {
        get { return _pathMediaMessage; }
        set { SetProperty(ref _pathMediaMessage, value); }
    }
    private string _pathPhoneNumber;
    public string PathPhoneNumber
    {
        get { return _pathPhoneNumber; }
        set { SetProperty(ref _pathPhoneNumber, value); }
    }
    private string _status = "Đang tạm dừng";
    public string Status
    {
        get { return _status; }
        set { SetProperty(ref _status, value); }
    }
    private int _amountDone;
    public int AmountDone
    {
        get { return _amountDone; }
        set { SetProperty(ref _amountDone, value); }
    }
    private int _totaltMessage;
    public int TotalMessage
    {
        get { return _totaltMessage; }
        set { SetProperty(ref _totaltMessage, value); }
    }
    private string _message = "Xin chao anh/chi !";
    public string Message
    {
        get { return _message; }
        set { SetProperty(ref _message, value); }
    }
    private bool _isShowBrowser = true;
    public bool IsShowBrowser
    {
        get { return _isShowBrowser; }
        set { SetProperty(ref _isShowBrowser, value); }
    }
    private int _rogressBar;
    public int ProgressBar
    {
        get { return _rogressBar; }
        set { SetProperty(ref _rogressBar, value); }
    }
}