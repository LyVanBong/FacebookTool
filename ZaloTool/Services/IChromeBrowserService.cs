using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;

namespace ZaloTool.Services;

public interface IChromeBrowserService
{
    Task<bool> LoginZalo(string pathChromeProfile);
    Task<ChromeDriver> OpenChrome(string pathChromeProfile);
}