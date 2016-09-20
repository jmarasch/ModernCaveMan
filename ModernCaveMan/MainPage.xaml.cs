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


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ModernCaveMan {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page {
        public MainPage() {
            //Microsoft.IoT.AdcMcp3008.AdcMcp3008ControllerProvider chip = new Microsoft.IoT.AdcMcp3008.AdcMcp3008ControllerProvider(0);

            //chip.ReadValue(0);


            this.InitializeComponent();

            Listen(null, null);

            //< local:ProbeControl Grid.Column = "0" Grid.Row = "0" x: Name = "Probe1" Background = "Blue" BorderThickness = "1" ></ local:ProbeControl >
            //< local:ProbeControl Grid.Column = "1" Grid.Row = "0" x: Name = "Probe2" Background = "Green" BorderThickness = "1" ></ local:ProbeControl >
            //< local:ProbeControl Grid.Column = "0" Grid.Row = "1" x: Name = "Probe3" Background = "Red" BorderThickness = "1" ></ local:ProbeControl >
            //< local:ProbeControl Grid.Column = "1" Grid.Row = "1" x: Name = "Probe4" Background = "Yellow" BorderThickness = "1" ></ local:ProbeControl >
            //< local:ProbeControl Grid.Column = "0" Grid.Row = "2" x: Name = "Probe5" Background = "Lavender" BorderThickness = "1" ></ local:ProbeControl >
            //< local:ProbeControl Grid.Column = "1" Grid.Row = "2" x: Name = "Probe6" Background = "LawnGreen" BorderThickness = "1" ></ local:ProbeControl >
            //< local:ProbeControl Grid.Column = "0" Grid.Row = "3" x: Name = "Probe7" Background = "Purple" BorderThickness = "1" ></ local:ProbeControl >
            //< local:ProbeControl Grid.Column = "1" Grid.Row = "3" x: Name = "Probe8" Background = "Aquamarine" BorderThickness = "1" ></ local:ProbeControl >

            }

        #region SocketServer
        private StreamSocket _socket = new StreamSocket();
        private StreamSocketListener _listener = new StreamSocketListener();
        private List<StreamSocket> _connections = new List<StreamSocket>();
        private bool _connecting = false;

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e) {
            //myConnectionTargetText.Text = "192.168.1.3";
            //myConnectionTargetText.Text = "localhost";
            }

        async private void WaitForData(StreamSocket socket) {
            var dr = new DataReader(socket.InputStream);
            //dr.InputStreamOptions = InputStreamOptions.Partial;
            var stringHeader = await dr.LoadAsync(4);

            if (stringHeader == 0) {
                LogMessage(string.Format("Disconnected (from {0})", socket.Information.RemoteHostName.DisplayName));
                return;
                }

            int strLength = dr.ReadInt32();

            uint numStrBytes = await dr.LoadAsync((uint)strLength);
            string msg = dr.ReadString(numStrBytes);

            LogMessage(string.Format("Received (from {0}): {1}", socket.Information.RemoteHostName.DisplayName, msg));

            WaitForData(socket);
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
            ConsoleBox.Items.Insert(0, new TextBlock { Text = message });
            }
        private void ConsoleClear() {
            ConsoleBox.Items.Clear();
            }
        #endregion
        }
    }