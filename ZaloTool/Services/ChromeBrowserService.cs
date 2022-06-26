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
    public async Task<bool> LoginZalo(string pathChromeProfile)
    {
        ChromeDriver driver = await OpenChrome(pathChromeProfile);

        driver.Navigate().GoToUrl("https://id.zalo.me/");

        while (true)
        {
            var cookies = driver.Manage().Cookies.GetCookieNamed("zlogin_session");
            if (cookies == null)
            {
                driver.Close();
                return true;
            }
        }
    }

    public Task<ChromeDriver> OpenChrome(string pathChromeProfile)
    {
        ChromeOptions chromeOptions = new ChromeOptions();
        chromeOptions.BinaryLocation = binaryLocationChrome;
        chromeOptions.AddExcludedArgument("enable-automation");
        chromeOptions.AddUserProfilePreference("credentials_enable_service", false);
        chromeOptions.AddUserProfilePreference("profile.password_manager_enabled", false);
        chromeOptions.AddArgument(@"user-data-dir=" + pathChromeProfile);
        ChromeDriverService chromeDriverService = ChromeDriverService.CreateDefaultService();
        chromeDriverService.HideCommandPromptWindow = true;
        return Task.FromResult(new ChromeDriver(chromeDriverService, chromeOptions));
    }
}