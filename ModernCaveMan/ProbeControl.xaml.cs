using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
using Windows.Devices.Adc;
using Windows.UI;


// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ModernCaveMan {
    public sealed partial class ProbeControl : UserControl {
        //DispatcherTimer timer = new DispatcherTimer();
        Probe probedata;
        SolidColorBrush colorRed\ = new SolidColorBrush(Colors.Red);
        SolidColorBrush colorGreen = new SolidColorBrush(Colors.Green);
        SolidColorBrush colorClear = new SolidColorBrush();

        public string ProbeName {
            get { return probedata.FriendlyName; }
            set { probedata.FriendlyName = value; }
            }

        public AdcChannel ProbeChannel {
            get { return probedata.DataChannel; }
            set { probedata.DataChannel = value; }
            }

        public bool SetProbeAsTarget(Double temp) {
            probedata.ProbeType = ProbeTypeEnu.Target;
            probedata.TempTarget = temp;
            return true;
            }
        public bool SetProbeAsRange(Double temp, double min, double max) {
            probedata.ProbeType = ProbeTypeEnu.Range;
            probedata.TempTarget = temp;
            probedata.TempMin = min;
            probedata.TempMax = max;
            return true;
            }

        public ProbeControl(int ID, string FriendlyName, AdcChannel dataChannel, double targetTemp) {
            this.InitializeComponent();
            this.DataContext = probedata;

            probedata = new Probe(ID, FriendlyName, dataChannel, targetTemp);

            probedata.ProbeTargetReached += ProbeTargetReached;
            probedata.ProbeOutOfRange += ProbeOutOfRange;
            }

        private void ProbeOutOfRange(object sender, EventArgs e) {
            MainGrid.Background = colorRed;
            }

        private void ProbeTargetReached(object sender, EventArgs e) {
            MainGrid.Background = colorGreen;
            }

        public ProbeControl(int ID, string FriendlyName, AdcChannel dataChannel, double targetTemp, double tempMin, double tempMax) {
            this.InitializeComponent();
            this.DataContext = probedata;

            probedata = new Probe(ID, FriendlyName, dataChannel, targetTemp, tempMin, tempMax);
            }

        public int ProbeID {
            get { return int.Parse(lblProbeID.Text); }
            set { lblProbeID.Text = value.ToString(); }
            }

        public string FriendlyName {
            get { return probedata.FriendlyName; }
            set { probedata.FriendlyName = value; }
            }
        }
    } 
