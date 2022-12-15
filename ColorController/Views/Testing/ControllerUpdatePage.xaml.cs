using Acr.UserDialogs;
using ColorController;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BLESample1.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ControllerUpdatePage : ContentPage
    {
        //var command = Encoding.ASCII.GetBytes($"{CommandTxt.Text}\r\n");

        private ObservableCollection<string> _output;
        public ObservableCollection<string> OutputSource
        {
            get { return _output; }
            set { _output = value; OnPropertyChanged(nameof(OutputSource)); }
        }

       
        private ICharacteristic selectedCharacteristic;
        //private IReadOnlyList<IDescriptor> descriptors;

        private string commandText;
        public string CommandText
        {
            get { return commandText; }
            set { commandText = value; OnPropertyChanged(nameof(CommandText)); }
        }

        public bool IsAwait { get; private set; }

        public ControllerUpdatePage()
        {
            InitializeComponent();
            BindingContext = this;
            selectedCharacteristic = App.Characteristic;
            CommandText = "PING";
            OutputSource = new ObservableCollection<string>();
            IsAwait = true;
        }

        //protected async override void OnAppearing()
        //{
        //    //Get descriptor
        //    //descriptors = await selectedCharacteristic.GetDescriptorsAsync();
        //    if (App.Characteristic!=null)
        //    {
        //        //selectedCharacteristic.ValueUpdated += OnValueUpdated;
        //    }
        //    else
        //    {
        //        UserDialogs.Instance.Alert("Please Connect Controller!");
        //    }
        //    //await selectedCharacteristic.StartUpdatesAsync();
        //}

        //private async void OnValueUpdated(object sender, CharacteristicUpdatedEventArgs e)
        //{
        //    try
        //    {
        //        //Call below method just after receiving response from controller
        //        await App.Characteristic.StopUpdatesAsync();

        //        var bytes = e.Characteristic.Value;
        //        Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() =>
        //        {
        //            var response = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        //            Debug.WriteLine($"OTA Update Response: {response}");
        //            //UserDialogs.Instance.Alert(response);
        //            Debug.WriteLine($"Output: {response}");
        //            OutputSource.Insert(0, response);
        //        });
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}

        //protected async override void OnDisappearing()
        //{
        //    await App.Characteristic.StopUpdatesAsync();
        //    App.Characteristic.ValueUpdated -= OnValueUpdated;
        //}

        #region Methods

        private async void RobocopClicked(object sender, EventArgs e)
        {
            await CommonCodeToPlayAnimation("ColorController.Robocop.lmd", "1001");
        }
        
        private async void PortalClicked(object sender, EventArgs e)
        {
            await CommonCodeToPlayAnimation("ColorController.portal.lmd", "1001");
        }

        private async void CypeneticClicked(object sender, EventArgs e)
        {
            await CommonCodeToPlayAnimation("ColorController.cybernetic.lmd", "1006");
        }

        private async void Patt_1093Clicked(object sender, EventArgs e)
        {
            await CommonCodeToPlayAnimation("ColorController.patt_1093.lmd", "1093");
        }

        //STOR is now updated to take two parameters, in the form of STOR XXXX YYYYYYY,
        //where xxxx is a number between 1001 and 1999. YYY is the size of the file, and it can be 0 to 200704.
        private async Task CommonCodeToPlayAnimation(string animationFile, string command = null)
        {
            try
            {
                int delay = 2000;

                App.ContinueFetchingBatteryDetail = false;

                await ExecuteCommand("OFF-");
                await Task.Delay(delay);
                var stream = GetFileStream(animationFile);
                var fileSize = stream.Length;

                WriteLog($"Sending Command: STOR {command} {fileSize}");

                await selectedCharacteristic.StartUpdatesAsync();

                var commandArray = Encoding.ASCII.GetBytes($"STOR {command} {fileSize}");
                var response = await selectedCharacteristic.WriteAsync(commandArray);

                if (Device.RuntimePlatform == Device.Android)
                {
                    await Task.Delay(500);
                }

                var byteArray = GetFileByteArray(stream);

                //SendChunks
                UserDialogs.Instance.ShowLoading("Storing....");

                //await Task.Delay(500);
                var startingTime = DateTime.Now;
                //WriteLog($"Sending Chunks : {startingTime}");

                int start = 0;
                while (start < byteArray.Length)
                {
                    int chunkLength = Math.Min(App.SIZE, byteArray.Length - start);
                    byte[] chunk = new byte[chunkLength];
                    Array.Copy(byteArray, start, chunk, 0, chunkLength);
                    if (Device.RuntimePlatform == Device.Android)
                    {
                        await Task.Delay(50); 
                    }
                    await selectedCharacteristic.WriteAsync(chunk);
                    start += App.SIZE;
                }

                await SendDoneClicked(null, null);

                var endTime = DateTime.Now;
                var timeTaken = endTime.Subtract(startingTime);

                WriteLog($"End Chunks : Time Taken: {timeTaken}");

                UserDialogs.Instance.HideLoading();
                await ExecuteCommand("ON--");
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.HideLoading();
                WriteLog(ex.Message); 

                await Task.Delay(1000);
                await ExecuteCommand("ON--");
                UserDialogs.Instance.Alert(ex.Message);
            }
        }

        private void WriteLog(string message)
        {
            Debug.WriteLine($"---------------------------------------------------------------------------------------");
            Debug.WriteLine($"{message}");
            Debug.WriteLine($"---------------------------------------------------------------------------------------");
        }

        public Stream GetFileStream(string filaname)
        {
            Stream stream = null;
            try
            {
                var assembly = System.Reflection.IntrospectionExtensions.GetTypeInfo(typeof(ControllerUpdatePage)).Assembly;
                stream = assembly.GetManifestResourceStream(filaname);
            }
            catch (Exception)
            {

            }

            return stream;
        }

        public byte[] GetFileByteArray(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private void SendChunks(byte[] senddata)
        {
            var repeat = true;
            int time = 0;
            try
            {
                Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        try
                        {
                            UserDialogs.Instance.ShowLoading($"Time Elapsed {time}");
                            if (!repeat)
                            {
                                UserDialogs.Instance.HideLoading();
                            }

                            //UserDialogs.Instance.HideLoading();
                        }
                        catch (Exception)
                        {
                        }
                    });

                    time++;

                    return repeat; // True = Repeat again, False = Stop the timer
                });


                //byte[] senddata = Encoding.ASCII.GetBytes("Hi1290004847846767627723676");
                int start = 0;
                Device.BeginInvokeOnMainThread(() =>
                {
                    while (start < senddata.Length)
                    {
                        int chunksize = 20;
                        int chunkLength = Math.Min(chunksize, senddata.Length - start);
                        byte[] chunk = new byte[chunkLength];
                        Array.Copy(senddata, start, chunk, 0, chunkLength);

                        //await selectedCharacteristic.StartUpdatesAsync();
                        //selectedCharacteristic.WriteType = Plugin.BLE.Abstractions.CharacteristicWriteType.WithoutResponse;
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            await selectedCharacteristic.WriteAsync(chunk);
                            start += chunksize;
                            await Task.Delay(100);

                            await SendDoneClicked(null, null);
                            repeat = false;
                            UserDialogs.Instance.HideLoading();
                        });
                    }
                });
            }
            catch (Exception)
            {

            }
        }

        private async Task SendDoneClicked(object sender, EventArgs e)
        {
            await selectedCharacteristic.StartUpdatesAsync();
            var commandArray = Encoding.ASCII.GetBytes($"DONE\r\n");
            await Task.Delay(500);
            var response = await selectedCharacteristic.WriteAsync(commandArray);
            await Task.Delay(500);
        }


        private async void Button1Clicked(object sender, EventArgs e)
        {
            await ExecuteCommand(Entry1.Text);
        }

        private async void Button2Clicked(object sender, EventArgs e)
        {
            await ExecuteCommand(Entry2.Text);
        }

        private async void Button3Clicked(object sender, EventArgs e)
        {
            await ExecuteCommand(Entry3.Text);
        }

        private async void Button4Clicked(object sender, EventArgs e)
        {
            await ExecuteCommand(Entry4.Text);
        }

        private async void Button5Clicked(object sender, EventArgs e)
        {
            await ExecuteCommand(Entry5.Text);
        }

        private async void Button6Clicked(object sender, EventArgs e)
        {
            await ExecuteCommand(Entry6.Text);
        }

        private async void Button7Clicked(object sender, EventArgs e)
        {
            await ExecuteCommand(Entry7.Text);
        }

        private async void Button8Clicked(object sender, EventArgs e)
        {
            await ExecuteCommand(Entry8.Text);
        }

        private async void DONEClicked(object sender, EventArgs e)
        {
            await ExecuteCommand("DONE\r\n");
        }

        private async Task ExecuteCommand(string command)
        {
            WriteLog($"OTA Update Sending: {command}");

            try
            {
                if (MainThread.IsMainThread)
                {
                    await App.Characteristic.StartUpdatesAsync();
                    var commandArray = Encoding.ASCII.GetBytes(command);
                    var cancellationTokenSource = new CancellationTokenSource();
                    cancellationTokenSource.CancelAfter(1000);
                    var response = await selectedCharacteristic.WriteAsync(commandArray, cancellationTokenSource.Token);
                    //UserDialogs.Instance.HideLoading();
                    Debug.WriteLine($"OTA Update Success: {command}");
                }
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.HideLoading();
                WriteLog($"OTA Update Error for {command} command : {ex.Message}");
                throw;
            }
        }

        #endregion

        private void ClearData(object sender, EventArgs e)
        {
            OutputSource.Clear();
        }

        private void DisableAsyncClicked(object sender, EventArgs e)
        {
            if (IsAwait)
            {
                IsAwait = false;
                DisableAsyncButton.Text = "Enable Async";
            }
            else
            {
                IsAwait = true;
                DisableAsyncButton.Text = "Disable Async";
            }
        }

        private async void ButtonTestCommandClicked(object sender, EventArgs e)
        {
            await ExecuteCommand(EntryTest.Text);
        }
    }
}