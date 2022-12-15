using ColorController.Enums;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ColorController.Helpers
{
    public class CommandHelper
    {
        public static async Task SendCommandToController(string command, bool executeStartUpdate = true)
        {
            if (App.Characteristic != null && App.ConnectionState == ConnectionButtonState.ShowDisconnect)
            {
                var characteristic = App.Characteristic;
                var commandArray = Encoding.ASCII.GetBytes(command);

                if (MainThread.IsMainThread)
                {
                    if (executeStartUpdate)
                    {
                        await characteristic.StartUpdatesAsync();
                    }
                    var result = await characteristic.WriteAsync(commandArray);
                }
                else
                {
                    Debug.WriteLine($"====================CommandHelper MainThread.InvokeOnMainThreadAsync : {command}===============================");

                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        if (executeStartUpdate)
                        {
                            await characteristic.StartUpdatesAsync();
                        }
                        var result = await characteristic.WriteAsync(commandArray);
                    });
                }
            }
        }

        public static async Task SendCommandToUpdateFirmware(string command, bool executeStartUpdate = true, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    CommonUtils.WriteLog($"Sending Command: {command}");

                    cancellationToken.ThrowIfCancellationRequested();
                    if (App.Characteristic != null && App.ConnectionState == ConnectionButtonState.ShowDisconnect)
                    {
                        var characteristic = App.Characteristic;
                        var commandArray = Encoding.ASCII.GetBytes(command);

                        if (MainThread.IsMainThread)
                        {
                            if (executeStartUpdate)
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                await characteristic.StartUpdatesAsync();
                            }

                            cancellationToken.ThrowIfCancellationRequested();
                            var result = await characteristic.WriteAsync(commandArray, cancellationToken);
                        }
                        else
                        {
                            CommonUtils.WriteLog($"====================CommandHelper MainThread.InvokeOnMainThreadAsync : {command}===============================");

                            await MainThread.InvokeOnMainThreadAsync(async () =>
                            {
                                if (executeStartUpdate)
                                {
                                    cancellationToken.ThrowIfCancellationRequested();
                                    await characteristic.StartUpdatesAsync();
                                }

                                cancellationToken.ThrowIfCancellationRequested();
                                var result = await characteristic.WriteAsync(commandArray);
                            });
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                CommonUtils.WriteLog($"SendCommandToController(): {command} command : {ex.Message}");
                throw;
            }
        }

        public static string GetXXXHueValue(double hue)
        {
            double value = 0;
            if (hue < 0.5)
            {
                value = (hue) * 256 + 128;
            }
            if (hue >= 0.5)
            {
                value = (hue) * 256 - 128;
            }
            value = Math.Round(value);
            return value.ToString();
        }

        public static double GetSaturationValue(double luminosity)
        {
            double value = 0;
            if (luminosity > 0 && luminosity <= 0.525)
            {
                value = 1;
            }
            if (luminosity > 0.525 && luminosity <= 0.550)
            {
                value = 0.95;
            }
            if (luminosity > 0.550 && luminosity <= 0.575)
            {
                value = 0.90;
            }
            if (luminosity > 0.575 && luminosity <= 0.600)
            {
                value = 0.85;
            }
            if (luminosity > 0.600 && luminosity <= 0.625)
            {
                value = 0.80;
            }
            if (luminosity > 0.625 && luminosity <= 0.650)
            {
                value = 0.75;
            }
            if (luminosity > 0.650 && luminosity <= 0.675)
            {
                value = 0.70;
            }
            if (luminosity > 0.675 && luminosity <= 0.700)
            {
                value = 0.65;
            }
            if (luminosity > 0.700 && luminosity <= 0.725)
            {
                value = 0.60;
            }
            if (luminosity > 0.725 && luminosity <= 0.750)
            {
                value = 0.55;
            }
            if (luminosity > 0.750 && luminosity <= 0.775)
            {
                value = 0.50;
            }
            if (luminosity > 0.775 && luminosity <= 0.800)
            {
                value = 0.45;
            }
            if (luminosity > 0.800 && luminosity <= 0.825)
            {
                value = 0.40;
            }
            if (luminosity > 0.825 && luminosity <= 0.850)
            {
                value = 0.35;
            }
            if (luminosity > 0.850 && luminosity <= 0.875)
            {
                value = 0.30;
            }
            if (luminosity > 0.875 && luminosity <= 0.900)
            {
                value = 0.25;
            }
            if (luminosity > 0.900 && luminosity <= 0.925)
            {
                value = 0.20;
            }
            if (luminosity > 0.925 && luminosity <= 0.950)
            {
                value = 0.15;
            }
            if (luminosity > 0.950 && luminosity <= 0.975)
            {
                value = 0.10;
            }
            if (luminosity > 0.975 && luminosity <= 1)
            {
                value = 0;
            }

            return value;
        }

        public static async Task SendHueSaturationToController(Color color)
        {
            if (App.Characteristic != null && App.ConnectionState == ConnectionButtonState.ShowDisconnect)
            {
                var hueXXX = GetXXXHueValue(color.Hue);
                if (hueXXX.Length == 1)
                {
                    hueXXX = $"00{hueXXX}";
                }
                if (hueXXX.Length == 2)
                {
                    hueXXX = $"0{hueXXX}";
                }

                var saturationValue = GetSaturationValue(color.Luminosity);
                var saturationXXX = $"{saturationValue * 100}";
                if (saturationXXX.Length == 1)
                {
                    saturationXXX = $"00{saturationXXX}";
                }
                if (saturationXXX.Length == 2)
                {
                    saturationXXX = $"0{saturationXXX}";
                }

                await SendCommandToController($"OPTS {hueXXX}{saturationXXX}", false);
            }
        }
    }
}
