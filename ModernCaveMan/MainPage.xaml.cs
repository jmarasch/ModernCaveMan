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
            ProbeControl probe1 = new ProbeControl(1, "Top Rack", null, 160, TempScale.F);
            ProbeControl probe2 = new ProbeControl(1, "Middle Rack", null, 160, TempScale.F);
            ProbeControl probe4 = new ProbeControl(1, "Bottom Rack", null, 160, TempScale.F);
            ProbeControl probe3 = new ProbeControl(2, "Ambient Top", null, 225 , 200, 250, TempScale.F);
            ProbeControl probe5 = new ProbeControl(2, "Ambient Bottom", null, 225 , 200, 250, TempScale.F);
#endif
            addProbeControl(probe1);
            addProbeControl(probe2);
            addProbeControl(probe4);
            addProbeControl(probe3);
            addProbeControl(probe5);
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

        #region view-layout
        private void addProbeControl(UIElement control) {
            ProbeLayout.Children.Add(control);
            updateProbeLayout();
            }

        private void removeProbeControl(UIElement control) {
            ProbeLayout.Children.Remove(control);
            UpdateLayout();
            }

        private void updateProbeLayout() {
            int probeCount = ProbeLayout.Children.Count();
            int contrl = 0;
            FrameworkElement control;

            //if less than 2 do top and bottom, do 1x2 grid
            if (probeCount <= 2) {
                for (int r = 0; r < 3; r++) {
                        control = ProbeLayout.Children[contrl] as FrameworkElement;
                        Grid.SetRow(control, r);
                        Grid.SetRowSpan(control, 2);
                        Grid.SetColumn(control, 0);
                        Grid.SetColumnSpan(control, 2);
                    contrl++;
                    r++;
                    if (contrl >= probeCount) break;
                    }
                }
            //if more than 2 but less than 4, do 2x2 grid
            if (probeCount > 2 && probeCount <= 4) {
                for (int r = 0; r < 3; r++) {
                    for (int c = 0; c < 2; c++) {
                        control = ProbeLayout.Children[contrl] as FrameworkElement;
                        Grid.SetRow(control, r);
                        Grid.SetRowSpan(control, 2);
                        Grid.SetColumn(control, c);
                        Grid.SetColumnSpan(control, 1);
                        contrl++;
                        if (contrl >= probeCount) break;
                        }
                    r++;
                    if (contrl >= probeCount) break;
                    }
                }
            //if more than 4 do 2x8 grid
            if (probeCount > 4) {
                for (int r = 0; r < 4; r++) {
                    for (int c = 0; c < 2; c++) {
                        control = ProbeLayout.Children[contrl] as FrameworkElement;
                        Grid.SetRow(control, r);
                        Grid.SetRowSpan(control, 1);
                        Grid.SetColumn(control, c);
                        Grid.SetColumnSpan(control, 1);
                        contrl++;
                        if (contrl >= probeCount) break;
                        }
                    if (contrl >= probeCount) break;
                    }
                }
            }
        #endregion

        #region SocketServer
        private StreamSocket _socket = new StreamSocket();
        private StreamSocketListener _listener = new StreamSocketListener();
        private List<StreamSocket> _connections = new List<StreamSocket>();

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

        async private void Listen(object sender, RoutedEventArgs e) {
            _listener.ConnectionReceived += listenerConnectionReceived;
            await _listener.BindServiceNameAsync("1283");

            LogMessage(string.Format("listening on {0}...", _listener.Information.LocalPort));
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

        private void btnSend_Click(object sender, RoutedEventArgs e) {
            string command = txtInput.Text;
            string commandResponce = "";
            DoRemoteCommand(command, out commandResponce);
            txtInput.Text = "";
            ConsoleWriteLine(commandResponce);
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
                    CommandResponce += Environment.NewLine + "AddTargetProbe|int ProbeID, double TargetTemp";
                    CommandResponce += Environment.NewLine + "AddRangeProbe|int ProbeID, double TargetTemp";
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
                    CommandResponce += Environment.NewLine + "clr|";
                    return CommandResult.OK;
                case "clr":
                    ConsoleBox.Text = "";
                    break;
                }

            CommandResponce = "Command not found";
            return CommandResult.CommandNotFound;
            }
        #endregion

        private void Page_KeyDown(object sender, KeyRoutedEventArgs e) {
            switch (e.Key) {
                case Windows.System.VirtualKey.None:
                    break;
                case Windows.System.VirtualKey.LeftButton:
                    break;
                case Windows.System.VirtualKey.RightButton:
                    break;
                case Windows.System.VirtualKey.Cancel:
                    break;
                case Windows.System.VirtualKey.MiddleButton:
                    break;
                case Windows.System.VirtualKey.XButton1:
                    break;
                case Windows.System.VirtualKey.XButton2:
                    break;
                case Windows.System.VirtualKey.Back:
                    break;
                case Windows.System.VirtualKey.Tab:
                    break;
                case Windows.System.VirtualKey.Clear:
                    break;
                case Windows.System.VirtualKey.Enter:
                    if (SplitView.IsPaneOpen) {
                        btnSend_Click(sender, e);
                        e.Handled = true;
                        }
                    break;
                case Windows.System.VirtualKey.Shift:
                    break;
                case Windows.System.VirtualKey.Control:
                    break;
                case Windows.System.VirtualKey.Menu:
                    break;
                case Windows.System.VirtualKey.Pause:
                    break;
                case Windows.System.VirtualKey.CapitalLock:
                    break;
                case Windows.System.VirtualKey.Kana:
                    break;
                case Windows.System.VirtualKey.Junja:
                    break;
                case Windows.System.VirtualKey.Final:
                    break;
                case Windows.System.VirtualKey.Hanja:
                    break;
                case Windows.System.VirtualKey.Escape:
                    break;
                case Windows.System.VirtualKey.Convert:
                    break;
                case Windows.System.VirtualKey.NonConvert:
                    break;
                case Windows.System.VirtualKey.Accept:
                    break;
                case Windows.System.VirtualKey.ModeChange:
                    break;
                case Windows.System.VirtualKey.Space:
                    break;
                case Windows.System.VirtualKey.PageUp:
                    break;
                case Windows.System.VirtualKey.PageDown:
                    break;
                case Windows.System.VirtualKey.End:
                    break;
                case Windows.System.VirtualKey.Home:
                    break;
                case Windows.System.VirtualKey.Left:
                    break;
                case Windows.System.VirtualKey.Up:
                    break;
                case Windows.System.VirtualKey.Right:
                    break;
                case Windows.System.VirtualKey.Down:
                    break;
                case Windows.System.VirtualKey.Select:
                    break;
                case Windows.System.VirtualKey.Print:
                    break;
                case Windows.System.VirtualKey.Execute:
                    break;
                case Windows.System.VirtualKey.Snapshot:
                    break;
                case Windows.System.VirtualKey.Insert:
                    break;
                case Windows.System.VirtualKey.Delete:
                    break;
                case Windows.System.VirtualKey.Help:
                    break;
                case Windows.System.VirtualKey.Number0:
                    break;
                case Windows.System.VirtualKey.Number1:
                    break;
                case Windows.System.VirtualKey.Number2:
                    break;
                case Windows.System.VirtualKey.Number3:
                    break;
                case Windows.System.VirtualKey.Number4:
                    break;
                case Windows.System.VirtualKey.Number5:
                    break;
                case Windows.System.VirtualKey.Number6:
                    break;
                case Windows.System.VirtualKey.Number7:
                    break;
                case Windows.System.VirtualKey.Number8:
                    break;
                case Windows.System.VirtualKey.Number9:
                    break;
                case Windows.System.VirtualKey.A:
                    break;
                case Windows.System.VirtualKey.B:
                    break;
                case Windows.System.VirtualKey.C:
                    break;
                case Windows.System.VirtualKey.D:
                    break;
                case Windows.System.VirtualKey.E:
                    break;
                case Windows.System.VirtualKey.F:
                    break;
                case Windows.System.VirtualKey.G:
                    break;
                case Windows.System.VirtualKey.H:
                    break;
                case Windows.System.VirtualKey.I:
                    break;
                case Windows.System.VirtualKey.J:
                    break;
                case Windows.System.VirtualKey.K:
                    break;
                case Windows.System.VirtualKey.L:
                    break;
                case Windows.System.VirtualKey.M:
                    break;
                case Windows.System.VirtualKey.N:
                    break;
                case Windows.System.VirtualKey.O:
                    break;
                case Windows.System.VirtualKey.P:
                    break;
                case Windows.System.VirtualKey.Q:
                    break;
                case Windows.System.VirtualKey.R:
                    break;
                case Windows.System.VirtualKey.S:
                    break;
                case Windows.System.VirtualKey.T:
                    break;
                case Windows.System.VirtualKey.U:
                    break;
                case Windows.System.VirtualKey.V:
                    break;
                case Windows.System.VirtualKey.W:
                    break;
                case Windows.System.VirtualKey.X:
                    break;
                case Windows.System.VirtualKey.Y:
                    break;
                case Windows.System.VirtualKey.Z:
                    break;
                case Windows.System.VirtualKey.LeftWindows:
                    break;
                case Windows.System.VirtualKey.RightWindows:
                    break;
                case Windows.System.VirtualKey.Application:
                    break;
                case Windows.System.VirtualKey.Sleep:
                    break;
                case Windows.System.VirtualKey.NumberPad0:
                    break;
                case Windows.System.VirtualKey.NumberPad1:
                    break;
                case Windows.System.VirtualKey.NumberPad2:
                    break;
                case Windows.System.VirtualKey.NumberPad3:
                    break;
                case Windows.System.VirtualKey.NumberPad4:
                    break;
                case Windows.System.VirtualKey.NumberPad5:
                    break;
                case Windows.System.VirtualKey.NumberPad6:
                    break;
                case Windows.System.VirtualKey.NumberPad7:
                    break;
                case Windows.System.VirtualKey.NumberPad8:
                    break;
                case Windows.System.VirtualKey.NumberPad9:
                    break;
                case Windows.System.VirtualKey.Multiply:
                    break;
                case Windows.System.VirtualKey.Add:
                    break;
                case Windows.System.VirtualKey.Separator:
                    break;
                case Windows.System.VirtualKey.Subtract:
                    break;
                case Windows.System.VirtualKey.Decimal:
                    break;
                case Windows.System.VirtualKey.Divide:
                    break;
                case Windows.System.VirtualKey.F1:
                    break;
                case Windows.System.VirtualKey.F2:
                    SplitView.IsPaneOpen = !SplitView.IsPaneOpen;
                    e.Handled = true;
                    break;
                case Windows.System.VirtualKey.F3:
                    break;
                case Windows.System.VirtualKey.F4:
                    break;
                case Windows.System.VirtualKey.F5:
                    break;
                case Windows.System.VirtualKey.F6:
                    break;
                case Windows.System.VirtualKey.F7:
                    break;
                case Windows.System.VirtualKey.F8:
                    break;
                case Windows.System.VirtualKey.F9:
                    break;
                case Windows.System.VirtualKey.F10:
                    break;
                case Windows.System.VirtualKey.F11:
                    break;
                case Windows.System.VirtualKey.F12:
                    break;
                case Windows.System.VirtualKey.F13:
                    break;
                case Windows.System.VirtualKey.F14:
                    break;
                case Windows.System.VirtualKey.F15:
                    break;
                case Windows.System.VirtualKey.F16:
                    break;
                case Windows.System.VirtualKey.F17:
                    break;
                case Windows.System.VirtualKey.F18:
                    break;
                case Windows.System.VirtualKey.F19:
                    break;
                case Windows.System.VirtualKey.F20:
                    break;
                case Windows.System.VirtualKey.F21:
                    break;
                case Windows.System.VirtualKey.F22:
                    break;
                case Windows.System.VirtualKey.F23:
                    break;
                case Windows.System.VirtualKey.F24:
                    break;
                case Windows.System.VirtualKey.NavigationView:
                    break;
                case Windows.System.VirtualKey.NavigationMenu:
                    break;
                case Windows.System.VirtualKey.NavigationUp:
                    break;
                case Windows.System.VirtualKey.NavigationDown:
                    break;
                case Windows.System.VirtualKey.NavigationLeft:
                    break;
                case Windows.System.VirtualKey.NavigationRight:
                    break;
                case Windows.System.VirtualKey.NavigationAccept:
                    break;
                case Windows.System.VirtualKey.NavigationCancel:
                    break;
                case Windows.System.VirtualKey.NumberKeyLock:
                    break;
                case Windows.System.VirtualKey.Scroll:
                    break;
                case Windows.System.VirtualKey.LeftShift:
                    break;
                case Windows.System.VirtualKey.RightShift:
                    break;
                case Windows.System.VirtualKey.LeftControl:
                    break;
                case Windows.System.VirtualKey.RightControl:
                    break;
                case Windows.System.VirtualKey.LeftMenu:
                    break;
                case Windows.System.VirtualKey.RightMenu:
                    break;
                case Windows.System.VirtualKey.GoBack:
                    break;
                case Windows.System.VirtualKey.GoForward:
                    break;
                case Windows.System.VirtualKey.Refresh:
                    break;
                case Windows.System.VirtualKey.Stop:
                    break;
                case Windows.System.VirtualKey.Search:
                    break;
                case Windows.System.VirtualKey.Favorites:
                    break;
                case Windows.System.VirtualKey.GoHome:
                    break;
                case Windows.System.VirtualKey.GamepadA:
                    break;
                case Windows.System.VirtualKey.GamepadB:
                    break;
                case Windows.System.VirtualKey.GamepadX:
                    break;
                case Windows.System.VirtualKey.GamepadY:
                    break;
                case Windows.System.VirtualKey.GamepadRightShoulder:
                    break;
                case Windows.System.VirtualKey.GamepadLeftShoulder:
                    break;
                case Windows.System.VirtualKey.GamepadLeftTrigger:
                    break;
                case Windows.System.VirtualKey.GamepadRightTrigger:
                    break;
                case Windows.System.VirtualKey.GamepadDPadUp:
                    break;
                case Windows.System.VirtualKey.GamepadDPadDown:
                    break;
                case Windows.System.VirtualKey.GamepadDPadLeft:
                    break;
                case Windows.System.VirtualKey.GamepadDPadRight:
                    break;
                case Windows.System.VirtualKey.GamepadMenu:
                    break;
                case Windows.System.VirtualKey.GamepadView:
                    break;
                case Windows.System.VirtualKey.GamepadLeftThumbstickButton:
                    break;
                case Windows.System.VirtualKey.GamepadRightThumbstickButton:
                    break;
                case Windows.System.VirtualKey.GamepadLeftThumbstickUp:
                    break;
                case Windows.System.VirtualKey.GamepadLeftThumbstickDown:
                    break;
                case Windows.System.VirtualKey.GamepadLeftThumbstickRight:
                    break;
                case Windows.System.VirtualKey.GamepadLeftThumbstickLeft:
                    break;
                case Windows.System.VirtualKey.GamepadRightThumbstickUp:
                    break;
                case Windows.System.VirtualKey.GamepadRightThumbstickDown:
                    break;
                case Windows.System.VirtualKey.GamepadRightThumbstickRight:
                    break;
                case Windows.System.VirtualKey.GamepadRightThumbstickLeft:
                    break;
                default:
                    break;
                }

            if(e.Key == Windows.System.VirtualKey.F2) {
                }
            }

        }
    }