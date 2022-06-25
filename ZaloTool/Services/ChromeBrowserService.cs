using System.IO;
using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;

namespace ZaloTool.Services;

public class ChromeBrowserService : IChromeBrowserService
{
    private string binaryLocationChrome = "C:\\Program Files\\Google\\Chrome\\Application\\Chrome.exe";
    public ChromeBrowserService()
    {

    }
    public Task<bool> LoginZalo(string pathChromeProfile)
    {
        ChromeOptions chromeOptions = new ChromeOptions();
        chromeOptions.BinaryLocation = binaryLocationChrome;
        chromeOptions.AddExcludedArgument("enable-automation");
        chromeOptions.AddUserProfilePreference("credentials_enable_service", false);
        chromeOptions.AddUserProfilePreference("profile.password_manager_enabled", false);
        chromeOptions.AddArgument(@"user-data-dir=" + pathChromeProfile);
        ChromeDriverService chromeDriverService = ChromeDriverService.CreateDefaultService();
        chromeDriverService.HideCommandPromptWindow = true;
        ChromeDriver driver = new ChromeDriver(chromeDriverService, chromeOptions);

        driver.Navigate().GoToUrl("https://id.zalo.me/");

        while (true)
        {
            var cookies = driver.Manage().Cookies.GetCookieNamed("zlogin_session");
            if (cookies == null)
            {
                driver.Close();
                return Task.FromResult(true);
            }
        }
    }
}