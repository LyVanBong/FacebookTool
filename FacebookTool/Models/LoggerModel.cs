using Prism.Mvvm;

namespace FacebookTool.Models;

public class LoggerModel : BindableBase
{
    private string _logMessage;

    public string LogMessage
    {
        get => _logMessage;
        set => SetProperty(ref _logMessage, value);
    }

    public LoggerModel(string time, string uid, bool isPost, string status)
    {
        var post = isPost ? "Post" : "Message";
        LogMessage = $"[{time}] UID:{uid} {post} {status}";
    }
}