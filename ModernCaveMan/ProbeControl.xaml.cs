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
        SolidColorBrush colorRed = new SolidColorBrush(Colors.Red);
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

            SetTargetVisuals();

            return true;
            }

        private void SetTargetVisuals() {

            }

        public bool SetProbeAsRange(Double temp, double min, double max) {
            probedata.ProbeType = ProbeTypeEnu.Range;
            probedata.TempTarget = temp;
            probedata.TempMin = min;
            probedata.TempMax = max;

            SetRangeVisuals();

            return true;
            }


        private void SetRangeVisuals() {
            //        < sfg:CircularScale x:Name = "CircleScale"
            //               StartAngle = "135"
            //               SweepAngle = "270"
            //               StartValue = "175"
            //               TickStroke = "DarkGray"
            //               TickLength = "5"
            //               TickShape = "Triangle"
            //               SmallTickLength = "3"
            //               SmallTickStroke = "Gray"
            //               TickStrokeThickness = "2"
            //               EndValue = "325"
            //               Interval = "25"
            //               LabelStroke = "Red"
            //               LabelAutoSizeChange = "True"
            //               LabelOffset = "5"
            //               EnableSmartLabels = "True"
            //               NumericScaleType = "Auto"
            //               NoOfFractionalDigit = "0"
            //               LabelPostfix = "°"
            //               LabelPosition = "Inside"
            //               >
            //< sfg:CircularScale.Ranges x:Name = "CircleScaleRanges" >

            RangeLowLow.StartValue = 0;

                //     < sfg:CircularRange StartValue = "0"
                    //                         EndValue = "200" Stroke = "red" />

            //      < sfg:CircularRange StartValue = "200"
            //                         EndValue = "225" Stroke = "Orange" />

            //      < sfg:CircularRange StartValue = "225"
            //                         EndValue = "275" Stroke = "Green" />

            //      < sfg:CircularRange StartValue = "275"
            //                         EndValue = "300" Stroke = "Orange" />

            //      < sfg:CircularRange StartValue = "300"
            //                         EndValue = "500" Stroke = "red" />

            //  </ sfg:CircularScale.Ranges >
            }

        public ProbeControl(int ID, string FriendlyName, AdcChannel dataChannel, double targetTemp) {
            this.InitializeComponent();
            this.DataContext = probedata;

            probedata = new Probe(ID, FriendlyName, dataChannel, targetTemp);

            SetTargetVisuals();

            AddEvents();
            }


        public ProbeControl(int ID, string FriendlyName, AdcChannel dataChannel, double targetTemp, double min, double max) {
            this.InitializeComponent();
            this.DataContext = probedata;

            probedata = new Probe(ID, FriendlyName, dataChannel, targetTemp, min, max);

            SetRangeVisuals();

            AddEvents();
            }

        private void AddEvents() {
            probedata.ProbeTargetReached += ProbeTargetReached;
            probedata.ProbeOutOfRange += ProbeOutOfRange;
            probedata.ProbeBackInRange += ProbeBackInRange;
            }
        private void ProbeBackInRange(object sender, EventArgs e) {
            MainGrid.Background = colorClear;
            }

        private void ProbeOutOfRange(object sender, EventArgs e) {
            MainGrid.Background = colorRed;
            }

        private void ProbeTargetReached(object sender, EventArgs e) {
            MainGrid.Background = colorGreen;
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
