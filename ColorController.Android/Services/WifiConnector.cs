using Android.Content;
using Android.Net.Wifi;
using ColorController.Droid.Services;
using ColorController.Models;
using ColorController.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(WifiConnector))]
namespace ColorController.Droid.Services
{
    public class WifiConnector : IWifiConnector
    {
        public async Task<List<WifiNetwork>> WifiList()
        {
            var wifiDisponiveis = new List<WifiNetwork>();
           
            try
            {
                WifiManager wifiManager = (WifiManager)Android.App.Application.Context.GetSystemService(Context.WifiService);
                //var scanStarted = wifiManager.StartScan();
                var rssi = wifiManager.ConnectionInfo.Rssi;
                var level = WifiManager.CalculateSignalLevel(rssi, 10);
                //if (scanStarted)
                //{
                    var networks = wifiManager.ScanResults;
                    foreach (var network in networks)
                    {
                        wifiDisponiveis.Add(new WifiNetwork
                        {
                            Name = network.Ssid,
                            Strength = network.Level
                        });
                    }
                //}
                //var result = wifiManager.IsScanAlwaysAvailable;

            }
            catch (Exception)
            {
                 
            }
                
            return wifiDisponiveis;
        }
    }
}