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
        DispatcherTimer timer = new DispatcherTimer();

        public TempScale Scale { get; set; }

        Probe probedata;
        SolidColorBrush colorRed = new SolidColorBrush(Colors.PaleVioletRed);
        SolidColorBrush colorGreen = new SolidColorBrush(Colors.LightGreen);
        SolidColorBrush colorYellow = new SolidColorBrush(Colors.LightYellow);
        SolidColorBrush colorOrange = new SolidColorBrush(Colors.LightGoldenrodYellow);
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
            double target = probedata.TempTarget;

            TargetLow.StartValue = 0.0; TargetLow.EndValue = target;
            TargetGood.StartValue = target; TargetGood.EndValue = 1000000.0;

            RangeLowLow.Visibility = Visibility.Collapsed;
            RangeLowMin.Visibility = Visibility.Collapsed;
            RangeLow.Visibility = Visibility.Collapsed;
            RangeGreen.Visibility = Visibility.Collapsed;
            RangeHi.Visibility = Visibility.Collapsed;
            RangeHiMax.Visibility = Visibility.Collapsed;
            RangeHiHi.Visibility = Visibility.Collapsed;
            TargetLow.Visibility = Visibility.Visible;
            TargetGood.Visibility = Visibility.Visible;

            switch (Scale) {
                case TempScale.F:
                    CircleGauge.Scales[0].StartValue = target - 100;
                    CircleGauge.Scales[0].EndValue = target + 50;
                    lblTarget.Text = string.Format("Target Temp: {0}°F", target);
                    break;
                case TempScale.C:
                    CircleGauge.Scales[0].StartValue = target - 60;
                    CircleGauge.Scales[0].EndValue = target + 30;
                    lblTarget.Text = string.Format("Target Temp: {0}°C", target);
                    break;
                case TempScale.K:
                    CircleGauge.Scales[0].StartValue = target - 60;
                    CircleGauge.Scales[0].EndValue = target + 30;
                    lblTarget.Text = string.Format("Target Temp: {0}°K", target);
                    break;
                case TempScale.R:
                    CircleGauge.Scales[0].StartValue = target - 10000;
                    CircleGauge.Scales[0].EndValue = target + 10000;
                    lblTarget.Text = string.Format("Target Temp: {0} OHMS", target);
                    break;
                case TempScale.A:
                    CircleGauge.Scales[0].StartValue = target - 100;
                    CircleGauge.Scales[0].EndValue = target + 100;
                    lblTarget.Text = string.Format("Target Temp: {0} ADC", target);
                    break;
                default:
                    break;
                }
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
            double min = probedata.TempMin;
            double cnt = probedata.TempTarget;
            double max = probedata.TempMax;

            double lowStep = (cnt - min) / 3;
            double hiStep = (max - cnt) / 3;

            double lowlow = min;
            double low = min + lowStep;
            double lowmin = low + lowStep;
            double himax = lowmin + lowStep + hiStep;
            double hi = himax + hiStep;
            double hihi = hi + hiStep;

            lblTarget.Text = string.Format("Target Temp: {0}°", cnt);

            RangeLowLow.StartValue = 0.0; RangeLowLow.EndValue = lowlow;
            RangeLowMin.StartValue = lowlow; RangeLowMin.EndValue = low;
            RangeLow.StartValue = low; RangeLow.EndValue = lowmin;
            RangeGreen.StartValue = lowmin; RangeGreen.EndValue = himax;
            RangeHi.StartValue = himax; RangeHi.EndValue = hi;
            RangeHiMax.StartValue = hi; RangeHiMax.EndValue = hihi;
            RangeHiHi.StartValue = hihi; RangeHiHi.EndValue = 500.0;

            TempHistoryGraph.BandRangeStart = lowlow;
            TempHistoryGraph.BandRangeEnd = hihi;

            RangeLowLow.Visibility = Visibility.Visible;
            RangeLowMin.Visibility = Visibility.Visible;
            RangeLow.Visibility = Visibility.Visible;
            RangeGreen.Visibility = Visibility.Visible;
            RangeHi.Visibility = Visibility.Visible;
            RangeHiMax.Visibility = Visibility.Visible;
            RangeHiHi.Visibility = Visibility.Visible;
            TargetLow.Visibility = Visibility.Collapsed;
            TargetGood.Visibility = Visibility.Collapsed;

            switch (Scale) {
                case TempScale.F:
                    CircleGauge.Scales[0].StartValue = min - 25;
                    CircleGauge.Scales[0].EndValue = max + 25;
                    lblTarget.Text = string.Format("Target Temp: {0}°F", cnt);
                    break;
                case TempScale.C:
                    CircleGauge.Scales[0].StartValue = min - 10;
                    CircleGauge.Scales[0].EndValue = max + 10;
                    lblTarget.Text = string.Format("Target Temp: {0}°C", cnt);
                    break;
                case TempScale.K:
                    CircleGauge.Scales[0].StartValue = min - 10;
                    CircleGauge.Scales[0].EndValue = max + 10;
                    lblTarget.Text = string.Format("Target Temp: {0}°K", cnt);
                    break;
                case TempScale.R:
                    CircleGauge.Scales[0].StartValue = min - 10000;
                    CircleGauge.Scales[0].EndValue = max + 10000;
                    lblTarget.Text = string.Format("Target Temp: {0} OHMS", cnt);
                    break;
                case TempScale.A:
                    CircleGauge.Scales[0].StartValue = min - 100;
                    CircleGauge.Scales[0].EndValue = max + 100;
                    lblTarget.Text = string.Format("Target Temp: {0} ADC", cnt);
                    break;
                default:
                    break;
                }
            }

        public ProbeControl(int ID, string FriendlyName, AdcChannel dataChannel, double targetTemp, TempScale scale) {
            this.InitializeComponent();
            this.DataContext = probedata;

            this.Scale = scale;

            probedata = new Probe(ID, FriendlyName, dataChannel, targetTemp);

            SetTargetVisuals();

            AddEvents();

            lblProbeID.Text = probedata.Title;
            lblTarget.Text = String.Format("Target Temp: {0}°", probedata.TempTarget);

            BindGraph();

            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += TakeReading;
            timer.Start();
            }

        private void BindGraph() {
            TempHistoryGraph.ItemsSource = probedata.GraphHistory;
            TempHistoryGraph.YBindingPath = "ReadingValue";
            }

        Random rnd = new Random();

        private void TakeReading(object sender, object e) {
#if X86
            int newtmp = 0;

            TempReading LAST = probedata.Last();

            //if (probedata.ProbeType == ProbeTypeEnu.Target) {
            //    if (LAST == null) LAST = new TempReading { ReadingValue = 60, ReadingTime = DateTime.Now };
            //    newtmp = rnd.Next((int)LAST.ReadingValue, (int)LAST.ReadingValue + 2);
            //    if (newtmp > 200) newtmp = 200;
            //    }

            //if (probedata.ProbeType == ProbeTypeEnu.Range) {
            //    if (LAST == null) LAST = new TempReading { ReadingValue = 250, ReadingTime = DateTime.Now };
            //    newtmp = rnd.Next((int)LAST.ReadingValue - 1, (int)LAST.ReadingValue + 5);
            //    if (newtmp > 300) newtmp = 300;
            //    if (newtmp < 200) newtmp = 200;
            //    }

            if (probedata.ProbeType == ProbeTypeEnu.Target) {
                if (LAST == null) LAST = new TempReading { ADC = 962, ReadingTime = DateTime.Now };
                newtmp = rnd.Next((int)LAST.ADC - 2, (int)LAST.ADC - 1);
                if (newtmp > 1000) newtmp = 1000;
                }
            
            if (probedata.ProbeType == ProbeTypeEnu.Range) {
                if (LAST == null) LAST = new TempReading { ADC = 346, ReadingTime = DateTime.Now };
                newtmp = rnd.Next((int)LAST.ADC - 2, (int)LAST.ADC + 2);
                if (newtmp > 1000) newtmp = 1000;
                if (newtmp < 5) newtmp = 5;
                }

            probedata.AddReading(newtmp);
#endif
#if ARM
            //DataChannel.ReadValue();
#endif
            UpDateUI();
            }

        public ProbeControl(int ID, string FriendlyName, AdcChannel dataChannel, double targetTemp, double min, double max, TempScale scale) {
            this.InitializeComponent();
            this.DataContext = probedata;

            this.Scale = scale;

            probedata = new Probe(ID, FriendlyName, dataChannel, targetTemp, min, max);

            SetRangeVisuals();

            AddEvents();

            lblProbeID.Text = probedata.Title;
            lblTarget.Text = String.Format("Target Temp: {0}°", probedata.TempTarget);

            BindGraph();

            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += TakeReading;
            timer.Start();
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

        private void UpDateUI() {
            switch (Scale) {
                case TempScale.F:
                    lblTemp.Text = String.Format("{0}°F", probedata.GraphHistory[0].TempF);
                    CircleGauge.Scales[0].Pointers[0].Value = probedata.GraphHistory[0].TempF;
                    break;
                case TempScale.C:
                    lblTemp.Text = String.Format("{0}°C", probedata.GraphHistory[0].TempC);
                    CircleGauge.Scales[0].Pointers[0].Value = probedata.GraphHistory[0].TempC;
                    break;
                case TempScale.K:
                    lblTemp.Text = String.Format("{0}°K", probedata.GraphHistory[0].TempK);
                    CircleGauge.Scales[0].Pointers[0].Value = probedata.GraphHistory[0].TempK;
                    break;
                case TempScale.R:
                    lblTemp.Text = String.Format("{0} OHMS", probedata.GraphHistory[0].Ohms);
                    CircleGauge.Scales[0].Pointers[0].Value = probedata.GraphHistory[0].Ohms;
                    break;
                case TempScale.A:
                    lblTemp.Text = String.Format("{0} ADC", probedata.GraphHistory[0].ADC);
                    CircleGauge.Scales[0].Pointers[0].Value = probedata.GraphHistory[0].ADC;
                    break;
                default:
                    break;
                }

            }
        }
    } 
