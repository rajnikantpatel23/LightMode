using ColorController.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColorController.Services
{
    public interface IWifiConnector
    {
        Task<List<WifiNetwork>> WifiList();
    }
}
