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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ModernCaveMan {
    public sealed partial class ProbeControl : UserControl {
        DispatcherTimer timer = new DispatcherTimer();
        TempViewModel tempdata = new TempViewModel();

        public ProbeControl() {
            this.InitializeComponent();
            this.DataContext = tempdata;

            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
            }
        
        public int ProbeID {
            get { return int.Parse(lblProbeID.Text); }
            set { lblProbeID.Text = value.ToString(); }
            }

        public Brush ProbeColor {
            get {
                return MainGrid.Background;
                }
            set {
                //MainGrid.Background = value;
                //MainGrid.Opacity = .5;
                }
            }

        private void Timer_Tick(object sender, object e) {
            TempReading LAST = tempdata.Last;
            if (LAST == null) LAST = new TempReading { Temp = 250, ReadingTime = DateTime.Now };
            Random rnd = new Random();
            int newtmp = rnd.Next((int)LAST.Temp - 5, (int)LAST.Temp + 5);
            if (newtmp > 300) newtmp = 300;
            if (newtmp < 200) newtmp = 200;

            tempdata.AddReading(newtmp);

            }
        }
    public class TempReading {
        public DateTime ReadingTime { get; set; }
        public double Temp { get; set; }
        }

    public class TempViewModel {
        public ObservableCollection<TempReading> TempsList { get; set; }
        const int MAX_GRAPH_DISPLAY = 30;

        public void AddReading(double temp) {
            if (TempsList.Count > MAX_GRAPH_DISPLAY) TempsList.RemoveAt(TempsList.Count-1);
            TempsList.Insert(0, new TempReading { Temp = temp, ReadingTime = DateTime.Now });
            }

        public TempReading Last {
            get {
                if (TempsList.Count <= 0) return null;
                return TempsList[0];
                }
            }
        public TempViewModel() {
            this.TempsList = new ObservableCollection<TempReading>();

            //DateTime date = DateTime.Today;

            //TempsList.Add(new TempReading { ReadingTime = date.AddHours(0.5), Temp = 250 });

            //TempsList.Add(new TempReading { ReadingTime = date.AddHours(0.5), Temp = 255 });

            //TempsList.Add(new TempReading { ReadingTime = date.AddHours(0.5), Temp = 265 });

            //TempsList.Add(new TempReading { ReadingTime = date.AddHours(0.5), Temp = 255 });

            //TempsList.Add(new TempReading { ReadingTime = date.AddHours(0.5), Temp = 250 });

            //TempsList.Add(new TempReading { ReadingTime = date.AddHours(0.5), Temp = 245 });

            //TempsList.Add(new TempReading { ReadingTime = date.AddHours(0.5), Temp = 240 });

            //TempsList.Add(new TempReading { ReadingTime = date.AddHours(0.5), Temp = 245 });

            //TempsList.Add(new TempReading { ReadingTime = date.AddHours(0.5), Temp = 250 });

            //TempsList.Add(new TempReading { ReadingTime = date.AddHours(0.5), Temp = 250 });

            }
        }
    } 
