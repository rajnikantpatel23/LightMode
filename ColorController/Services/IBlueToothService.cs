using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ColorController.Services
{
    public interface IBlueToothService
    {
        Task ScanAndConnectDevice();

        Task<List<ICharacteristic>> GetCharacteristics();

        Task SendCommandToController(string command, bool executeStartUpdate = true);

        Task SendHueSaturationToController(Color color);

        double GetSaturationValue(double luminosity);

        Task SendCommandToUpdateFirmware(string command, bool executeStartUpdate = true, CancellationToken cancellationToken = default);

        Guid ServiceId { get; }

        Guid CharacteristicsId { get; }
    }
}
