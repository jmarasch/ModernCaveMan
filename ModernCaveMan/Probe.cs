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

namespace ModernCaveMan {
    public class Probe {
        private ObservableCollection<TempReading> _readings;
        private ObservableCollection<TempReading> _graphedReadings;

        private const int MAX_DISPLAY_READINGS = 60;

        public ObservableCollection<TempReading> GraphHistory {
            get { return _graphedReadings; }
            }

        public Probe() {
            _readings = new ObservableCollection<TempReading>();
            _graphedReadings = new ObservableCollection<TempReading>();
            }

        public ProbeType ProbeType { get; set; }

        public int TempMin {get; set; }

        public int TempTarget { get; set; }

        public int TempMax { get; set; }

        public int ProbeID { get; set; }

        public Brush ProbeColor { get; set; }

        public TempReading Last() {
            if (_readings.Count <= 0) return null;
            return _readings[0];
            }

        public void AddReading(double temp) {
            if (_graphedReadings.Count > MAX_DISPLAY_READINGS) _graphedReadings.RemoveAt(_graphedReadings.Count - 1);
            TempReading newReading = new TempReading { ReadingValue = temp, ReadingTime = DateTime.Now };
            _graphedReadings.Insert(0, newReading);
            _readings.Insert(0, newReading);
            }
        }
    }