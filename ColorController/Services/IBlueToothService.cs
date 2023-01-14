using ColorController.Models;
using Plugin.BLE;
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
        Task ScanAndConnectDevice(CancellationToken token = default);

        Task<List<ICharacteristic>> GetCharacteristics();

        Task SendCommandToController(string command, bool executeStartUpdate = true);

        Task SendHueSaturationToController(Color color);

        double GetSaturationValue(double luminosity);

        Task SendCommandToUpdateFirmware(string command, bool executeStartUpdate = true, CancellationToken cancellationToken = default);

        Guid ServiceId { get; }

        Guid CharacteristicsId { get; }

        bool IsAppConnectedWithDevice { get; }

        int ConnectedDeviceCount { get; }

        Task<bool> ConnectToKnownDeviceInBackground(Controller controller);

        IReadOnlyList<IDevice> GetConnectedDevices();

        Task<bool> AreAllSavedDevicesConnected();

        Task<bool> IsBluetoothON();

        Task<bool> IsLocationPermissionAllowed();

        IAdapter Adapter { get; }












        Task Disconnect(bool executeCommand = true);

        Task StopScanning();

        void SendMessageToDisplayConnectButton();
        void SendMessageToDisplayDisconnectButton();
        void SendMessageToDisplayConnectingButton();


        Task ScanAndConnectDevice_2(CancellationToken cancellationToken = default);
    }
}
