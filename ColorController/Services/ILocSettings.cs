using System.Threading.Tasks;

namespace ColorController.Services
{
    public interface ILocSettings
    {
        void OpenSettings();

        Task<bool> IsGpsAvailable();
    }
}
