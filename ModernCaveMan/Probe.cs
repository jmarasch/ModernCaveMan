using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.Devices.Adc;
using System.Collections.Generic;
using System.Linq;

namespace ModernCaveMan {
    public class Probe {
        

        private ObservableCollection<TempReading> _readings;
        //private TempReadings _readings;
        private ObservableCollection<TempReading> _graphedReadings;
        //private TempReadings _graphedReadings;

        public String FriendlyName { get; set; }

        public string Title { get { return String.Format("{0}:{1}", ProbeID, FriendlyName); } }

        private int graphLength = 60;

        public ObservableCollection<TempReading> GraphHistory {
            get { return _graphedReadings; }
            }

        public double ReadingInterval {
            get { return readingTimer.Interval.TotalSeconds; }
            set { readingTimer.Interval = TimeSpan.FromSeconds(value); }
            }

        public void TakeReading() {

            }

       public double GaugeMin { get { return TempMin - 25; } }
       public double GaugeMax { get { return TempMax - 25; } }

        public AdcChannel DataChannel { get; set; }

        public ProbeTypeEnu ProbeType { get; set; }

        public double TempMin { get; set; }

        public double TempTarget { get; set; }

        public double TempMax { get; set; }

        public int ProbeID { get; set; }

        public bool SetGraphLength(int readings) {
            graphLength = readings;
            return true;
            }

        private DispatcherTimer readingTimer = new DispatcherTimer();
        private double readingInterval = 1;
        
        public Probe(int ID, string friendlyName, AdcChannel dataChannel, double targetTemp) {
            this.ProbeID = ID;
            this.FriendlyName = friendlyName;
            readingTimer.Interval = TimeSpan.FromSeconds(1);
            DataChannel = dataChannel;
            this.graphLength = 60;
            ProbeType = ProbeTypeEnu.Target;
            TempTarget = targetTemp;
            ProbeInit();            
            }

        public Probe(int ID, string friendlyName, AdcChannel dataChannel, double targetTemp, double min, double max){
            this.ProbeID = ID;
            this.FriendlyName = friendlyName;
            readingTimer.Interval = TimeSpan.FromSeconds(1);
            DataChannel = dataChannel;
            this.graphLength = 60;
            ProbeType = ProbeTypeEnu.Range;
            TempTarget = targetTemp;
            TempMax = max;
            TempMin = min;
            ProbeInit();
            }

        private void ProbeInit() {
            _readings = new ObservableCollection<TempReading>();
            _graphedReadings = new ObservableCollection<TempReading>();
            //_readings = new TempReadings();
            //_graphedReadings = new TempReadings();
            }

        public TempReading Last() {
            if (_readings.Count <= 0) return null;
            return _readings[0];
            }

        private const double VRef = 5;
        private const double mVRef = VRef*1000;
        private const double FIXED_OHMS = 10000.0;
        private Dictionary<double, double> THERM_TABLE = ThermisterSettings.Senstech100k25c.ThermisterTable;

        public void AddReading(int adcVal) {
            if (_graphedReadings.Count > graphLength) _graphedReadings.RemoveAt(_graphedReadings.Count - 1);
            TempReading newReading = new TempReading { ReadingTime = DateTime.Now };
            int AdcMax = DataChannel == null ? 1023 : DataChannel.Controller.MaxValue;
            
            float reading = 0;
            newReading.ADC = adcVal;

            // convert the adc reading volts
            newReading.Volts = ((mVRef / AdcMax) * adcVal) / 1000;

            //Find the thermister restance using adc volts and fixed resistor 
            newReading.Ohms = newReading.Volts*FIXED_OHMS/(VRef-newReading.Volts);

            double tempA, tempB;

            if (newReading.Ohms < THERM_TABLE.Last().Key) newReading.Ohms = THERM_TABLE.Last().Key + .000001;
            if (newReading.Ohms > THERM_TABLE.First().Key) newReading.Ohms = THERM_TABLE.First().Key - .000001;

            tempA = THERM_TABLE.First(r => newReading.Ohms > r.Key).Value;
            tempB = THERM_TABLE.Last(r => newReading.Ohms < r.Key).Value;

            newReading.TempC = (tempA + tempB) / 2;
            newReading.TempF = newReading.TempC * 9 / 5 + 32;
            newReading.TempK = newReading.TempC + 273.15;
            
            _graphedReadings.Insert(0, newReading);
            _readings.Insert(0, newReading);

            double temp = 0.0;

            switch () {
                default:
                    break;
                }


            if (ProbeOutOfRange != null && ProbeType == ProbeTypeEnu.Range &&
                (newReading.TempC > TempMax | newReading.TempC < TempMin)) {
                if (LastStateAtRead != ProbeState.OutOfRange) {
                    LastStateAtRead = ProbeState.OutOfRange;
                    ProbeOutOfRange(this, null);
                    }
                }

            if (ProbeTargetReached != null && ProbeType == ProbeTypeEnu.Target &&
                (newReading.TempC >= TempTarget)) {
                if (LastStateAtRead != ProbeState.TargetReached) {
                    LastStateAtRead = ProbeState.TargetReached;
                    ProbeTargetReached(this, null);
                    }
                }

            if (ProbeBackInRange != null && ProbeType == ProbeTypeEnu.Range &&
                (newReading.TempC <= TempMax & newReading.TempC >= TempMin)) {
                if (LastStateAtRead == ProbeState.OutOfRange) {
                    LastStateAtRead = ProbeState.Good;
                    ProbeBackInRange(this, null);
                    }
                }
            }

        private ProbeState LastStateAtRead { get; set; }


        public event EventHandler ProbeOutOfRange;
        public event EventHandler ProbeTargetReached;
        public event EventHandler ProbeBackInRange;
        
        }

    enum ProbeState {
        Good,
        OutOfRange,
        TargetReached
        }
    }