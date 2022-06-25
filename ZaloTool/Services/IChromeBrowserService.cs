using System.Threading.Tasks;

namespace ZaloTool.Services;

public interface IChromeBrowserService
{
    Task<bool> LoginZalo(string pathChromeProfile);
}