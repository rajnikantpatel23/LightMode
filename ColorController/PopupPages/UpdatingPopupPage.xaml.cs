using Acr.UserDialogs;
using ColorController.Helpers;
using ColorController.StringResources;
using Rg.Plugins.Popup.Services;
using System.Threading.Tasks;
using System;
using Xamarin.Essentials;
using Xamarin.Forms.Xaml;
using System.Text;
using System.Threading;
using ColorController.Enums;
using ColorController.Services;
using Plugin.BLE;
using Xamarin.Forms;
using ColorController.Models;
using ColorController.Views;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Plugin.BLE.Abstractions.Contracts;

namespace ColorController.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UpdatingPopupPage : BasePopupPage
    {
        public IBlueToothService BlueToothService => DependencyService.Get<IBlueToothService>();

        CancellationTokenSource _cancellationTokenSource;

        private double _ProgressValue;
        public double ProgressValue
        {
            get { return _ProgressValue; }
            set { _ProgressValue = value; OnPropertyChanged(nameof(ProgressValue)); }
        }

        private string _upperTextMessage;
        public string UpperTextMessage
        {
            get { return _upperTextMessage; }
            set { _upperTextMessage = value; OnPropertyChanged(nameof(UpperTextMessage)); }
        }
         
        private string _ssid;
        private string _password;
        private INavigation _navigation;
        private bool _isLoaded;

        /// <summary>
        /// Constructor
        /// </summary>
        public UpdatingPopupPage(string ssid, string password, INavigation navigation)
        {
            InitializeComponent();
            BindingContext = this;
            _cancellationTokenSource = new CancellationTokenSource();
            _ssid = ssid;
            _password = password;
            _navigation = navigation;
            UpperTextMessage = "Please wait...";
            ProgressValue = 0;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();             
            if (!_isLoaded)
            {
                _isLoaded = true;
                UpdateFirmware(_ssid, _password);
            }
        }

        public async void UpdateFirmware(string ssid, string password)
        {
            try
            {               
                var delay = 2000;
                var url = Constants.BinaryUrl;

                //Navigate to 4th Tab to fix Navigation Bar UI issues
                await _navigation.PopToRootAsync();

                Preferences.Set(StringResource.SendDFUQCommand, true);

                //Stop fetching battery power status
                App.ContinueFetchingBatteryDetail = false;

                await SendCommand("OFF-");
                await Task.Delay(delay);

                await StoreLMDFilesInController();

                UpperTextMessage = "Updating your controller's firmware";

                await SendCommand($"URLS {url.Length}");
                await Task.Delay(delay);
                await SendCommand(url);
                await Task.Delay(delay);
                await SendCommand("DONE\r\n");
                await Task.Delay(delay);
                await SendCommand("SSID");
                await Task.Delay(delay);
                await SendCommand(ssid);
                await Task.Delay(delay);
                await SendCommand("DONE\r\n");
                await Task.Delay(delay);
                await SendCommand("PASS");
                await Task.Delay(delay);
                await SendCommand(password);
                await Task.Delay(delay);
                await SendCommand("DONE\r\n");
                await Task.Delay(delay);
                await SendCommand("DFUU");
                UserDialogs.Instance.HideLoading();
                await Task.Delay(delay);
                BlueToothService.SendMessageToDisplayConnectButton();

                StartAutoConnectingInBackground();
            }
            catch (OperationCanceledException ex)
            {
                await SendCommand("ON--");
                CommonUtils.WriteLog($"Exception: {ex.Message}");
                UserDialogs.Instance.HideLoading();
                await CommonUtils.ClosePopup();
            }
            catch (Exception ex)
            {
                await SendCommand("ON--");
                CommonUtils.WriteLog($"Exception: {ex.Message}");
                UserDialogs.Instance.HideLoading();
                await CommonUtils.ClosePopup();
                await CommonUtils.OpenPopupPage(new AlertPopupPage(ex.Message));
            }
        } 

        /// <summary>
        /// This will be called to store each LMD file in the controller
        /// </summary>
        /// <returns></returns>
        private async Task StoreLMDFilesInController()
        {
            try
            {
                var characteristics = await BlueToothService.GetCharacteristics();
                var characteristic = characteristics.FirstOrDefault();

                int totalProgress = 100;
                int newAnimationsToUpload = 0;
                var completedPercentages = 0;

                List<AnimationModel> animationsToUpload = new List<AnimationModel>();
                var animationsWithLMD = Constants.GetAnimations().Where(x => x.FileName != null);
                foreach (var animation in animationsWithLMD)
                {
                    var lmd = await App.Database.GetLMDStatus(App.ConnectedController.Id, animation.Id);
                    var itemControllerVersion = new Version(animation.ControllerVersion);
                    var connectedControllerVersion = new Version(App.ConnectedControllerVersion);
                    if (lmd == null && itemControllerVersion > connectedControllerVersion)
                    {
                        animationsToUpload.Add(animation);
                        newAnimationsToUpload++;
                    }
                }

                var singleSectionPercentages = totalProgress / (newAnimationsToUpload + 1);

                if (animationsToUpload != null && animationsToUpload.Any())
                {
                    UpperTextMessage = "Uploading new animations to your controller";

                    foreach (var animation in animationsToUpload)
                    {
                        _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                        //Check whether LMD has stored or not
                        //var isAlreadyUploaded = await App.Database.IsLMDAlreadyUploaded(App.ConnectedController.Id, animation.Id);

                        var itemControllerVersion = new Version(animation.ControllerVersion);
                        var connectedControllerVersion = new Version(App.ConnectedControllerVersion);

                        //UserDialogs.Instance.ShowLoading($"Sending file...{item.FileName}");

                        //await CommonCodeToPlayAnimation(item.FileName, item.Command);

                        //Line:1=>UserDialogs.Instance.ShowLoading("Sending...");
                        _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                        var stream = GetFileStream(animation.FileName);
                        if (stream != null)
                        {
                            var fileSize = stream.Length;

                            //WriteLog($"Sending Command: STOR {animation.Code} {fileSize}");

                            _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                            await characteristic.StartUpdatesAsync();

                            var commandArray = Encoding.ASCII.GetBytes($"STOR {animation.Code} {fileSize}");
                            _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                            var response = await characteristic.WriteAsync(commandArray);

                            if (Device.RuntimePlatform == Device.Android)
                            {
                                _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                                await Task.Delay(500);
                            }


                            //var startingTime = DateTime.Now;
                            //WriteLog($"Sending Chunks : {startingTime}");

                            //await characteristic.StartUpdatesAsync();
                            int start = 0;
                            var byteArray = GetFileByteArray(stream);
                            while (start < byteArray.Length)
                            {
                                int chunkLength = Math.Min(App.SIZE, byteArray.Length - start);
                                byte[] chunk = new byte[chunkLength];
                                Array.Copy(byteArray, start, chunk, 0, chunkLength);
                                if (Device.RuntimePlatform == Device.Android)
                                {
                                    _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                                    await Task.Delay(50);
                                }
                                _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                                await characteristic.WriteAsync(chunk);
                                start += App.SIZE;
                            }

                            App.LMDSentSuccessfully = true;
                            App.StoredAnimationId = animation.Id;
                            _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                            await SendDoneCommand();
                            App.LMDSentSuccessfully = false;

                            UserDialogs.Instance.HideLoading();

                            completedPercentages = completedPercentages + singleSectionPercentages;
                            ProgressValue = completedPercentages;
                            progressBar.Progress = ProgressValue / 100;
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Stream GetFileStream(string filaname)
        {
            Stream stream = null;
            try
            {
                var assembly = System.Reflection.IntrospectionExtensions.GetTypeInfo(typeof(ControllerUpdateDetailPage)).Assembly;
                stream = assembly.GetManifestResourceStream(filaname);
            }
            catch (Exception ex)
            {
                CommonUtils.WriteLog($"GetFileStream() Exception: {ex.Message}");
            }

            return stream;
        }

        public byte[] GetFileByteArray(Stream input)
        {
            try
            {
                _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                using (MemoryStream ms = new MemoryStream())
                {
                    input.CopyTo(ms);
                    return ms.ToArray();
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                CommonUtils.WriteLog($"GetFileByteArray() Exception: {ex.Message}");
                throw;
            }
        }

        private async Task SendDoneCommand()
        {
            try
            {
                _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                //await App.Characteristic.StartUpdatesAsync();
                var commandArray = Encoding.ASCII.GetBytes($"DONE\r\n");
                _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                await Task.Delay(500);
                _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                //var response = await App.Characteristic.WriteAsync(commandArray);
                await Task.Delay(500);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                CommonUtils.WriteLog($"SendDoneCommand() Exception: {ex.Message}");
                throw;
            }
        }

        private void StartAutoConnectingInBackground()
        {
            Task.Run(async () =>
            {
                //Get bluetooth status on App Starts
                if (Device.RuntimePlatform == Device.iOS && !CrossBluetoothLE.Current.IsOn)
                {
                    var platformSpecific = DependencyService.Get<IPlatformSpecific>();
                    platformSpecific.GetBluetoothStatus();
                    await Task.Delay(500);
                }

                if ((CrossBluetoothLE.Current.IsOn || App.IsiOSBluetoothOn) && !App.IsScanningAlreadyGoingOn/* && App.Characteristic == null*/)
                {
                    Preferences.Set("defaultControllerName", App.ConnectedController.Name);

                    App.IsScanningAlreadyGoingOn = true;
                    App.IsAutoScanningGoingOn = true;

                    var isConnected = await BlueToothService.ConnectToKnownDeviceInBackground(App.ConnectedController);
                    if (!isConnected && App.ConnectionState != ConnectionButtonState.ShowDisconnect)
                    {
                        StartAutoConnectingInBackground();
                    }

                    App.IsScanningAlreadyGoingOn = false;
                    App.IsAutoScanningGoingOn = false;
                }
            }).ConfigureAwait(false);
        }

        private async Task SendCommand(string command)
        {
            try
            {
                await BlueToothService.SendCommandToUpdateFirmware(command, cancellationToken: _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                CommonUtils.WriteLog($"OTA Update Error for {command} command : {ex.Message}");
                throw;
            }
        }

        private void CancelClicked(object sender, EventArgs e)
        {
            try
            {
                _cancellationTokenSource.Cancel();
                PopupNavigation.Instance.PopAsync();
                _navigation.PopToRootAsync();
            }
            catch (Exception)
            {

            }
        }
    }
}