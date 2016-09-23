using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.Devices.Adc;

namespace ModernCaveMan {
    public class Probe {
        private ObservableCollection<TempReading> _readings;
        private ObservableCollection<TempReading> _graphedReadings;

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
            TempMax = min;
            TempMin = max;
            ProbeInit();
            }

        private void ProbeInit() {
            _readings = new ObservableCollection<TempReading>();
            _graphedReadings = new ObservableCollection<TempReading>();

            readingTimer.Tick += TakeReading;
            readingTimer.Start();
            }
        
        private void TakeReading(object sender, object e) {
#if X86
            TempReading LAST = Last();
            if (LAST == null) LAST = new TempReading { ReadingValue = 250, ReadingTime = DateTime.Now };
            Random rnd = new Random();
            int newtmp = rnd.Next((int)LAST.ReadingValue - 5, (int)LAST.ReadingValue + 5);
            if (newtmp > 300) newtmp = 300;
            if (newtmp < 200) newtmp = 200;
            
            AddReading(newtmp); 
#endif
#if ARM
            //DataChannel.ReadValue();
#endif
            }


        public TempReading Last() {
            if (_readings.Count <= 0) return null;
            return _readings[0];
            }

        public void AddReading(double temp) {
            if (_graphedReadings.Count > graphLength) _graphedReadings.RemoveAt(_graphedReadings.Count - 1);
            TempReading newReading = new TempReading { ReadingValue = temp, ReadingTime = DateTime.Now };
            _graphedReadings.Insert(0, newReading);
            _readings.Insert(0, newReading);

            if (ProbeOutOfRange != null && ProbeType == ProbeTypeEnu.Range &&
                (newReading.ReadingValue > TempMax | newReading.ReadingValue < TempMin)) {
                if (LastStateAtRead != ProbeState.OutOfRange) {
                    LastStateAtRead = ProbeState.OutOfRange;
                    ProbeOutOfRange(this, null);
                    }
                }
            if (ProbeTargetReached != null && ProbeType == ProbeTypeEnu.Target &&
                (newReading.ReadingValue >= TempTarget)) {
                if (LastStateAtRead != ProbeState.TargetReached) {
                    LastStateAtRead = ProbeState.TargetReached;
                    ProbeTargetReached(this, null);
                    }
                }

            if (ProbeBackInRange != null && ProbeType == ProbeTypeEnu.Range &&
                (newReading.ReadingValue <= TempMax & newReading.ReadingValue >= TempMin)) {
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