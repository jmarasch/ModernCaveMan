//#define ADC3208
#define ADC3008

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.Networking;
using Windows.Media.SpeechSynthesis;
using Microsoft.IoT.AdcMcp3008;

#if ARM
using Windows.Devices.Adc;
#endif


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ModernCaveMan {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page {
        const float ReferenceVoltage = 5.0F;
        const float PullupResistorOhms = 100000F;

        //private List<ProbeControl> probes = new List<ProbeControl>();

#if ARM
        const byte ProbeADCChannel1 = 0;
        const byte ProbeADCChannel2 = 1;
        const byte ProbeADCChannel3 = 2;
        const byte ProbeADCChannel4 = 3;
        const byte ProbeADCChannel5 = 4;
        const byte ProbeADCChannel6 = 5;
        const byte ProbeADCChannel7 = 6;
        const byte ProbeADCChannel8 = 7;

        // ADC bus provider classes.
        AdcController adcController;
        AdcChannel ProbeADC1,
                   ProbeADC2,
                   ProbeADC3,
                   ProbeADC4,
                   ProbeADC5,
                   ProbeADC6,
                   ProbeADC7,
                   ProbeADC8;
#endif

        private SpeechSynthesizer synthesizer;

        public MainPage() {
            this.InitializeComponent();
#if ARM
            ProbeControl probe1 = new ProbeControl(1, "Top", ProbeADC1, 225);
#elif X86
            ProbeControl probe1 = new ProbeControl(1, "Top", null, 225);
#endif

            //probes.Add(probe1);

            ProbeContainer.Children.Add(probe1);
#if ARM
            //Microsoft.IoT.AdcMcp3008.AdcMcp3008ControllerProvider chip = new Microsoft.IoT.AdcMcp3008.AdcMcp3008ControllerProvider(0);
            // Create a new SpeechSynthesizer instance for later use.
            synthesizer = new SpeechSynthesizer();
#endif
            Listen(null, null);

            }

#if ARM
        async public void InitADC() {
            // Initialize the ADC chip for use
    #if ADC3008
            adcController = (await AdcController.GetControllersAsync(AdcMcp3008Provider.GetAdcProvider()))[0];
    #elif ADC3208
            adcController = (await AdcController.GetControllersAsync(AdcMcp3208Provider.GetAdcProvider()))[0];
    #endif
            ProbeADC1 = adcController.OpenChannel(ProbeADCChannel1);
            ProbeADC2 = adcController.OpenChannel(ProbeADCChannel2);
            ProbeADC3 = adcController.OpenChannel(ProbeADCChannel3);
            ProbeADC4 = adcController.OpenChannel(ProbeADCChannel4);
            ProbeADC5 = adcController.OpenChannel(ProbeADCChannel5);
            ProbeADC6 = adcController.OpenChannel(ProbeADCChannel6);
            ProbeADC7 = adcController.OpenChannel(ProbeADCChannel7);
            ProbeADC8 = adcController.OpenChannel(ProbeADCChannel8);
            }         
#endif

#region SocketServer
        private StreamSocket _socket = new StreamSocket();
        private StreamSocketListener _listener = new StreamSocketListener();
        private List<StreamSocket> _connections = new List<StreamSocket>();
        private bool _connecting = false;

        async private void WaitForData(StreamSocket socket) {
            var dr = new DataReader(socket.InputStream);
            //dr.InputStreamOptions = InputStreamOptions.Partial;

            try {
                while (true) {
                    var stringHeader = await dr.LoadAsync(4);

                    if (stringHeader == 0) {
                        LogMessage(string.Format("Disconnected (from {0})", socket.Information.RemoteHostName.DisplayName));
                        return;
                        }

                    int strLength = dr.ReadInt32();

                    uint numStrBytes = await dr.LoadAsync((uint)strLength);
                    string msg = dr.ReadString(numStrBytes);

                    LogMessage(string.Format("Received (from {0}): {1}", socket.Information.RemoteHostName.DisplayName, msg));

                    string commandResponse = "command response";

                    await Dispatcher.RunAsync(
                        Windows.UI.Core.CoreDispatcherPriority.Normal,
                        () => DoRemoteCommand(msg, out commandResponse));

                    SendMessage(socket, commandResponse);

                    //WaitForData(socket);
                    }
                }catch(Exception exc) {
                if (Windows.Networking.Sockets.SocketError.GetStatus(exc.HResult) == SocketErrorStatus.Unknown) {
                    throw;
                    }
                }
            
            }

        //async private void Connect(object sender, RoutedEventArgs e) {
        //    try {
        //        _connecting = true;
        //        updateControls(_connecting);
        //        await _socket.ConnectAsync(new HostName(myConnectionTargetText.Text), "121983");
        //        _connecting = false;
        //        updateControls(_connecting);

        //        LogMessage(string.Format("Connected to {0}", _socket.Information.RemoteHostName.DisplayName));

        //        WaitForData(_socket);
        //        } catch (Exception ex) {
        //        _connecting = false;
        //        updateControls(_connecting);
        //        MessageBox.Show(ex.Message);
        //        }
        //    }

        private void updateControls(bool connecting) {
            //myConnectionTargetText.IsEnabled = !connecting;
            //myConnect.IsEnabled = !connecting;
            //myInputText.IsEnabled = !connecting;
            //mySend.IsEnabled = !connecting;
            }

        async private void Listen(object sender, RoutedEventArgs e) {
            _listener.ConnectionReceived += listenerConnectionReceived;
            await _listener.BindServiceNameAsync("1283");

            LogMessage(string.Format("listening on {0}...", _listener.Information.LocalPort));
            //listen.IsEnabled = false;
            }

        void listenerConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args) {
            _connections.Add(args.Socket);

            LogMessage(string.Format("Incoming connection from {0}", args.Socket.Information.RemoteHostName.DisplayName));

            WaitForData(args.Socket);
            }

        async private void LogMessage(string message) {
            await Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                () => { ConsoleWriteLine(message); });
            }

        async private void SendMessage(StreamSocket socket, string message) {
            var writer = new DataWriter(socket.OutputStream);
            var len = writer.MeasureString(message); // Gets the UTF-8 string length.
            writer.WriteInt32((int)len);
            writer.WriteString(message);
            var ret = await writer.StoreAsync();
            writer.DetachStream();

            LogMessage(string.Format("Sent (to {0}) {1}", socket.Information.RemoteHostName.DisplayName, message));
            }

        private void Reply(object sender, RoutedEventArgs e) {
            foreach (var sock in _connections) {
                SendMessage(sock, "OK");
                }
            }
#endregion

#region ConsoleBoxFunctions
        private void ConsoleWriteLine(string message) {
            ConsoleBox.Text = message + Environment.NewLine + ConsoleBox.Text;
            }
        private void ConsoleClear() {
            ConsoleBox.Text = "";
            }
#endregion

#region CommandInterperater
        enum CommandResult {
            OK = 0,
            NFG = 1,
            NoCommand = 2,
            CommandNotFound = 3
            }

        private CommandResult DoRemoteCommand(string command, out string CommandResponce) {
            //command string = "command|args0,args1,args..."
            CommandResponce = "OK";
            string[] cmd = command.Split('|');
            string[] args = { };
            if(cmd.Length>1)
                args = cmd[0].Split(',');
            switch (cmd[0]) {
                case "":
                    //send info back to client
                    CommandResponce = "No command sent";
                    return CommandResult.NoCommand;
                case "ChangeProbeTarget":
                    ConsoleWriteLine("ChangeProbeCommand received");
                    return CommandResult.OK;
                case "?":
                    CommandResponce = "Possible Commands,";
                    CommandResponce += Environment.NewLine + "ChangeProbeTarget|int ProbeID, double TargetTemp";
                    CommandResponce += Environment.NewLine + "ChangeProbeType|int ProbeID, str Target/Range";
                    CommandResponce += Environment.NewLine + "ChangeProbeRange|int ProbeID, double Hi, double Low";
                    CommandResponce += Environment.NewLine + "ProbeEnabled|int ProbeID,bool true/false";
                    CommandResponce += Environment.NewLine + "GetProbeReadings|int ProbeID,(opt)int #ofRecords";
                    CommandResponce += Environment.NewLine + "ClearProbeCache|int ProbeID";
                    CommandResponce += Environment.NewLine + "UpdateSMSTo|str smsEmail";
                    CommandResponce += Environment.NewLine + "UpdateEmailTo|str Email";
                    CommandResponce += Environment.NewLine + "AudioAlarms|bool true/false";
                    CommandResponce += Environment.NewLine + "Shutdown|";
                    CommandResponce += Environment.NewLine + "Restart|";
                    return CommandResult.OK;
                }

            CommandResponce = "Command not found";
            return CommandResult.CommandNotFound;
            }
#endregion
        }
    }