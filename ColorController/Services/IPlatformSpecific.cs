using System;
using System.Collections.Generic;
using System.Text;

namespace ColorController.Services
{
    public interface IPlatformSpecific
    {
        bool GetBluetoothStatus();
        string GetCurrentWiFi();
        bool IsActivityFinishing();
    }
}
