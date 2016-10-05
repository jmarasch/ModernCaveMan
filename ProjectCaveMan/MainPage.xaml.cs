using System;
using System.Collections.Generic;
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
using System.Collections.ObjectModel;
using Windows.Devices.Enumeration;
using Windows.Devices.Spi;
using Windows.Storage;

namespace ProjectCaveMan
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page {
        #region Fields

        private const int ADC_MAX = 4095;

        private const int CURVE_SMOOTHING = 1;

        private const int MAX_READINGS = 600;

        private const Int32 SPI_CHIP_SELECT_LINE = 0;

        private const string SPI_CONTROLLER_NAME = "SPI0";

        private const double VREF = 4.80, VREF_MV = VREF * 1000;

        // The beta coefficient of the thermistor (usually 3000-4000)
        private double BCOEFFICIENT = 4070;

        //private byte[] ch0 = new byte[3] { 0x06, 0x00, 0x00 };
        //private byte[] ch1 = new byte[3] { 0x06, 0x40, 0x00 };
        //private byte[] ch2 = new byte[3] { 0x06, 0x80, 0x00 };
        //private byte[] ch3 = new byte[3] { 0x06, 0xC0, 0x00 };
        //private byte[] ch4 = new byte[3] { 0x07, 0x00, 0x00 };
        //private byte[] ch5 = new byte[3] { 0x07, 0x40, 0x00 };
        //private byte[] ch6 = new byte[3] { 0x07, 0x80, 0x00 };
        //private byte[] ch7 = new byte[3] { 0x07, 0xC0, 0x00 };

        private List<ProbeData> probes = new List<ProbeData>();

        private byte[] readBuffer = new byte[3];

        //Set Sampling
        private int SAMPLES = 100;

        private bool sampling = false;

        // the value of the 'other' resistor
        private double SERIESRESISTOR = 4600;

        private SpiDevice SpiDisplay;

        // temp. for nominal resistance (almost always 25 C)
        private double TEMPERATURENOMINAL = 25;

        // resistance at 25 degrees C
        private double THERMISTORNOMINAL = 100000;

        // create a timer
        private DispatcherTimer timer;

        private DateTime appStartTime = DateTime.Now;

        #endregion Fields

        #region Constructors

        public MainPage() {
            this.InitializeComponent();
            this.timer = new DispatcherTimer();
            this.timer.Interval = TimeSpan.FromMilliseconds(1000);
            this.timer.Tick += Timer_Tick;
            this.timer.Start();

            commands.Text =
            "Timer Space:Start/Stop | Home:Reset | Left:-10ms | Right:+10ms" + Environment.NewLine +
            "Samples PGU: +100 | Up:+1 | PGD: -100 | Down:-1 | End:=5 | Insert:=50 | Delete:=500";

            InitSPI();

            LoadProbes();

            SetupFileManager();

            }

        #endregion Constructors

        #region Methods

        //private StorageFolder userDir;
        private StorageFolder probDataDirRoot;
        private StorageFolder probDataDir;
        private List<StorageFile> probeDataFiles;

        private async void SetupFileManager() {
            string date = DateTime.Now.ToString("yyyy.MM.dd");
            probDataDirRoot = await ApplicationData.Current.LocalFolder.CreateFolderAsync("ProbeReadings", CreationCollisionOption.OpenIfExists);
            probDataDir = await probDataDirRoot.CreateFolderAsync(date, CreationCollisionOption.OpenIfExists);
            foreach (ProbeData probe in probes) {
                probe.DataFile = await probDataDir.CreateFileAsync(string.Format("{0}-{1}.csv", probe.ToString(), date), CreationCollisionOption.OpenIfExists);

                }
            }

        public double convertThermOhomsR2(double volts) {
            return volts * SERIESRESISTOR / (VREF - volts);
            }

        public int convertToInt(byte[] data) {
            int result = data[1] & 0x0F;
            result <<= 8;
            result += data[2];
            return result;
            }

        public double convertToResAdafruit(double average) {
            average = ADC_MAX / average - 1;
            return average = SERIESRESISTOR / average; ;
            }

        public double convertToVolts(double adcReading) {
            return (((VREF_MV / ADC_MAX) * adcReading) / 1000) - .1;
            }

        private Data DataAvarage(List<Data> logData) {
            Data retval = new Data();

            int currentCurve = CURVE_SMOOTHING > logData.Count ? logData.Count : CURVE_SMOOTHING;

            for (int i = 0; i < currentCurve; i++) {
                retval.adc += logData[i].adc;
                retval.volts += logData[i].volts;
                retval.therm += logData[i].therm;
                retval.tC += logData[i].tC;
                retval.tF += logData[i].tF;
                retval.tK += logData[i].tK;
                retval.cTC += logData[i].cTC;
                retval.cTF += logData[i].cTF;
                retval.cTK += logData[i].cTK;
                retval.addaRes += logData[i].addaRes;
                }

            retval.adc /= CURVE_SMOOTHING;
            retval.volts /= CURVE_SMOOTHING;
            retval.therm /= CURVE_SMOOTHING;
            retval.tC /= CURVE_SMOOTHING;
            retval.tF /= CURVE_SMOOTHING;
            retval.tK /= CURVE_SMOOTHING;
            retval.cTC /= CURVE_SMOOTHING;
            retval.cTF /= CURVE_SMOOTHING;
            retval.cTK /= CURVE_SMOOTHING;
            retval.addaRes /= CURVE_SMOOTHING;
            
            return retval;
            }

        private async void InitSPI() {
            try {
                var settings = new SpiConnectionSettings(SPI_CHIP_SELECT_LINE);
                //slower frequencies seem to make the readings more accurate
                settings.ClockFrequency = 300000;
                settings.Mode = SpiMode.Mode0;

                string spiAqs = SpiDevice.GetDeviceSelector(SPI_CONTROLLER_NAME);
                var deviceInfo = await DeviceInformation.FindAllAsync(spiAqs);
                SpiDisplay = await SpiDevice.FromIdAsync(deviceInfo[0].Id, settings);
                }

            /* If initialization fails, display the exception and stop running */
            catch (Exception ex) {
                throw new Exception("SPI Initialization Failed", ex);
                }
            }

        private void LoadProbes() {
            probes.Add(new ProbeData("Top Ambient",0));
            probes.Add(new ProbeData("Bottom Ambient", 1));
            probes.Add(new ProbeData("Outside Ambient", 6));
            probes.Add(new ProbeData("Top Rack", 2));
            probes.Add(new ProbeData("Mid Top Rack", 3));
            probes.Add(new ProbeData("Mid Bottom Rack",4));
            probes.Add(new ProbeData("Bottom Rack",5));
            //probes.Add(new ProbeData(7));
            }



        private double Stineheart(double average) {
            double steinhart;
            steinhart = average / THERMISTORNOMINAL;          // (R/Ro)
            steinhart = Math.Log(steinhart);                  // ln(R/Ro)
            steinhart /= BCOEFFICIENT;                        // 1/B * ln(R/Ro)
            steinhart += 1.0 / (TEMPERATURENOMINAL + 273.15); // + (1/To)
            steinhart = 1.0 / steinhart;                      // Invert
            steinhart -= 273.15;                              // convert to C

            return steinhart;
            }

        private void Timer_Tick(object sender, object e) {
            if (SpiDisplay == null) {
                lblch0.Text = "Spi Not Ready";
                return;
                }

            sampling = true;
            DateTime start = DateTime.Now;

            for (int i = 0; i < SAMPLES; i++) {
                foreach (ProbeData probe in probes) {
                    SpiDisplay.TransferFullDuplex(probe.writeBuffer, readBuffer);
                    probe.reading += convertToInt(readBuffer);
                    }
                }

            sampling = false;

            foreach (ProbeData probe in probes) {
                probe.reading /= SAMPLES;

                double volts = convertToVolts(probe.reading);
                double therm = convertToResAdafruit(probe.reading);

                double tempCcalc = Stineheart(therm);
                double tempKcalc = tempCcalc + 273.15;
                double tempFcalc = tempCcalc * 9 / 5 + 32;

                Data data = new Data {
                    ReadingTime = DateTime.Now,
                    adc = probe.reading,
                    volts = volts,
                    therm = therm,
                    cTC = tempCcalc,
                    cTF = tempFcalc,
                    cTK = tempKcalc,
                    };

                //log data and average
                probe.LogData.Insert(0, data);
                if (probe.LogData.Count > MAX_READINGS) probe.LogData.RemoveAt(MAX_READINGS);
                }

            DateTime end = DateTime.Now;

            TimeSpan readMS = ((DateTime)end - start).Duration();

            UpdateUI(readMS);

            //recalculate samples
            double timerMS = timer.Interval.Duration().TotalMilliseconds;

            //SAMPLES
            //decrease timer ms by .1 to allow time for other processes to run
            double msDiff = (timerMS - 100) - readMS.TotalMilliseconds;

            //calc total num of samples taken
            double totalSamples = (probes.Count * SAMPLES) + 0.0;

            //calc number of samples taken per ms
            double samplesPerMS = totalSamples / readMS.TotalMilliseconds;

            //round to whole number
            samplesPerMS = Math.Round(samplesPerMS, 0);

            double sampleAdj = Math.Round((samplesPerMS * msDiff / probes.Count), 0);

            SAMPLES += (int)sampleAdj;
            }

        private async void UpdateUI(TimeSpan data) {
            await Dispatcher.RunAsync(
            Windows.UI.Core.CoreDispatcherPriority.Normal,
                () => {
                    int probeCount = probes.Count;

                    if (probeCount > 0) {
                        lblch0.Text = string.Format("{3}{0}{1}°F{0}{2}°C", Environment.NewLine, Math.Round(probes[0].LogData[0].cTF, 1), Math.Round(probes[0].LogData[0].cTC, 1), probes[0].ToString());
                        }
                    if (probeCount > 1) {
                        lblch1.Text = string.Format("{3}{0}{1}°F{0}{2}°C", Environment.NewLine, Math.Round(probes[1].LogData[0].cTF, 1), Math.Round(probes[1].LogData[0].cTC, 1), probes[1].ToString());
                        }
                    if (probeCount > 2) {
                        lblch2.Text = string.Format("{3}{0}{1}°F{0}{2}°C", Environment.NewLine, Math.Round(probes[2].LogData[0].cTF, 1), Math.Round(probes[2].LogData[0].cTC, 1), probes[2].ToString());
                        }
                    if (probeCount > 3) {
                        lblch3.Text = string.Format("{3}{0}{1}°F{0}{2}°C", Environment.NewLine, Math.Round(probes[3].LogData[0].cTF, 1), Math.Round(probes[3].LogData[0].cTC, 1), probes[3].ToString());
                        }
                    if (probeCount > 4) {
                        lblch4.Text = string.Format("{3}{0}{1}°F{0}{2}°C", Environment.NewLine, Math.Round(probes[4].LogData[0].cTF, 1), Math.Round(probes[4].LogData[0].cTC, 1), probes[4].ToString());
                        }
                    if (probeCount > 5) {
                        lblch5.Text = string.Format("{3}{0}{1}°F{0}{2}°C", Environment.NewLine, Math.Round(probes[5].LogData[0].cTF, 1), Math.Round(probes[5].LogData[0].cTC, 1), probes[5].ToString());
                        }
                    if (probeCount > 6) {
                        lblch6.Text = string.Format("{3}{0}{1}°F{0}{2}°C", Environment.NewLine, Math.Round(probes[6].LogData[0].cTF, 1), Math.Round(probes[6].LogData[0].cTC, 1), probes[6].ToString());
                        }
                    if (probeCount > 7) {
                        lblch7.Text = string.Format("{3}{0}{1}°F{0}{2}°C", Environment.NewLine, Math.Round(probes[7].LogData[0].cTF, 1), Math.Round(probes[7].LogData[0].cTC, 1), probes[7].ToString());
                        }
                    IntervalReadout.Text = "";// String.Format("Interval:{0}", timer.Interval.TotalSeconds);
                    SamplesReadout.Text = String.Format("Sps:{0}/pp", SAMPLES);
                    commands.Text = "";// String.Format("ReadTime:{0}", data.TotalSeconds.ToString());
                    TimeSpan elapsed = (DateTime.Now - appStartTime);
                    lblTime.Text = string.Format("Time Elapsed{3}{0}:{1}:{2}", elapsed.Hours,elapsed.Minutes,elapsed.Seconds,Environment.NewLine);
                });
            }

        #endregion Methods
            
        }
    }


//private void Page_KeyDown(object sender, KeyRoutedEventArgs e) {
//    switch (e.Key) {
//        case Windows.System.VirtualKey.None:
//            break;

//        case Windows.System.VirtualKey.LeftButton:
//            break;

//        case Windows.System.VirtualKey.RightButton:
//            break;

//        case Windows.System.VirtualKey.Cancel:
//            break;

//        case Windows.System.VirtualKey.MiddleButton:
//            break;

//        case Windows.System.VirtualKey.XButton1:
//            break;

//        case Windows.System.VirtualKey.XButton2:
//            break;

//        case Windows.System.VirtualKey.Back:
//            break;

//        case Windows.System.VirtualKey.Tab:
//            break;

//        case Windows.System.VirtualKey.Clear:
//            break;

//        case Windows.System.VirtualKey.Enter:
//            break;

//        case Windows.System.VirtualKey.Shift:
//            break;

//        case Windows.System.VirtualKey.Control:
//            break;

//        case Windows.System.VirtualKey.Menu:
//            break;

//        case Windows.System.VirtualKey.Pause:
//            break;

//        case Windows.System.VirtualKey.CapitalLock:
//            break;

//        case Windows.System.VirtualKey.Kana:
//            break;

//        case Windows.System.VirtualKey.Junja:
//            break;

//        case Windows.System.VirtualKey.Final:
//            break;

//        case Windows.System.VirtualKey.Hanja:
//            break;

//        case Windows.System.VirtualKey.Escape:
//            break;

//        case Windows.System.VirtualKey.Convert:
//            break;

//        case Windows.System.VirtualKey.NonConvert:
//            break;

//        case Windows.System.VirtualKey.Accept:
//            break;

//        case Windows.System.VirtualKey.ModeChange:
//            break;

//        case Windows.System.VirtualKey.Space:
//            if (timer.IsEnabled)
//                timer.Stop();
//            else
//                timer.Start();
//            e.Handled = true;
//            break;

//        case Windows.System.VirtualKey.PageUp:
//            if (!sampling && SAMPLES < 12500) {
//                SAMPLES += 100;
//                e.Handled = true;
//                }
//            break;

//        case Windows.System.VirtualKey.PageDown:
//            if (!sampling && SAMPLES > 101) {
//                SAMPLES -= 100;
//                e.Handled = true;
//                }
//            break;

//        case Windows.System.VirtualKey.End:
//            SAMPLES = 5;
//            e.Handled = true;
//            break;

//        case Windows.System.VirtualKey.Home:
//            timer.Interval = TimeSpan.FromSeconds(1);
//            e.Handled = true;
//            break;

//        case Windows.System.VirtualKey.Left:
//            if (timer.Interval.TotalMilliseconds <= 20) break;
//            timer.Interval.Subtract(TimeSpan.FromMilliseconds(10));
//            e.Handled = true;
//            break;

//        case Windows.System.VirtualKey.Up:
//            if (!sampling && SAMPLES < 12500) {
//                SAMPLES++;
//                e.Handled = true;
//                }
//            break;

//        case Windows.System.VirtualKey.Right:
//            timer.Interval.Add(TimeSpan.FromMilliseconds(10));
//            break;

//        case Windows.System.VirtualKey.Down:
//            if (!sampling && SAMPLES > 1) {
//                SAMPLES--;
//                e.Handled = true;
//                }
//            break;

//        case Windows.System.VirtualKey.Select:
//            break;

//        case Windows.System.VirtualKey.Print:
//            break;

//        case Windows.System.VirtualKey.Execute:
//            break;

//        case Windows.System.VirtualKey.Snapshot:
//            break;

//        case Windows.System.VirtualKey.Insert:
//            SAMPLES = 50;
//            e.Handled = true;
//            break;

//        case Windows.System.VirtualKey.Delete:
//            SAMPLES = 500;
//            e.Handled = true;
//            break;

//        case Windows.System.VirtualKey.Help:
//            break;

//        case Windows.System.VirtualKey.Number0:
//            break;

//        case Windows.System.VirtualKey.Number1:
//            break;

//        case Windows.System.VirtualKey.Number2:
//            break;

//        case Windows.System.VirtualKey.Number3:
//            break;

//        case Windows.System.VirtualKey.Number4:
//            break;

//        case Windows.System.VirtualKey.Number5:
//            break;

//        case Windows.System.VirtualKey.Number6:
//            break;

//        case Windows.System.VirtualKey.Number7:
//            break;

//        case Windows.System.VirtualKey.Number8:
//            break;

//        case Windows.System.VirtualKey.Number9:
//            break;

//        case Windows.System.VirtualKey.A:
//            break;

//        case Windows.System.VirtualKey.B:
//            break;

//        case Windows.System.VirtualKey.C:
//            break;

//        case Windows.System.VirtualKey.D:
//            break;

//        case Windows.System.VirtualKey.E:
//            break;

//        case Windows.System.VirtualKey.F:
//            break;

//        case Windows.System.VirtualKey.G:
//            break;

//        case Windows.System.VirtualKey.H:
//            break;

//        case Windows.System.VirtualKey.I:
//            break;

//        case Windows.System.VirtualKey.J:
//            break;

//        case Windows.System.VirtualKey.K:
//            break;

//        case Windows.System.VirtualKey.L:
//            break;

//        case Windows.System.VirtualKey.M:
//            break;

//        case Windows.System.VirtualKey.N:
//            break;

//        case Windows.System.VirtualKey.O:
//            break;

//        case Windows.System.VirtualKey.P:
//            break;

//        case Windows.System.VirtualKey.Q:
//            break;

//        case Windows.System.VirtualKey.R:
//            break;

//        case Windows.System.VirtualKey.S:
//            break;

//        case Windows.System.VirtualKey.T:
//            break;

//        case Windows.System.VirtualKey.U:
//            break;

//        case Windows.System.VirtualKey.V:
//            break;

//        case Windows.System.VirtualKey.W:
//            break;

//        case Windows.System.VirtualKey.X:
//            break;

//        case Windows.System.VirtualKey.Y:
//            break;

//        case Windows.System.VirtualKey.Z:
//            break;

//        case Windows.System.VirtualKey.LeftWindows:
//            break;

//        case Windows.System.VirtualKey.RightWindows:
//            break;

//        case Windows.System.VirtualKey.Application:
//            break;

//        case Windows.System.VirtualKey.Sleep:
//            break;

//        case Windows.System.VirtualKey.NumberPad0:
//            break;

//        case Windows.System.VirtualKey.NumberPad1:
//            break;

//        case Windows.System.VirtualKey.NumberPad2:
//            break;

//        case Windows.System.VirtualKey.NumberPad3:
//            break;

//        case Windows.System.VirtualKey.NumberPad4:
//            break;

//        case Windows.System.VirtualKey.NumberPad5:
//            break;

//        case Windows.System.VirtualKey.NumberPad6:
//            break;

//        case Windows.System.VirtualKey.NumberPad7:
//            break;

//        case Windows.System.VirtualKey.NumberPad8:
//            break;

//        case Windows.System.VirtualKey.NumberPad9:
//            break;

//        case Windows.System.VirtualKey.Multiply:
//            break;

//        case Windows.System.VirtualKey.Add:
//            break;

//        case Windows.System.VirtualKey.Separator:
//            break;

//        case Windows.System.VirtualKey.Subtract:
//            break;

//        case Windows.System.VirtualKey.Decimal:
//            break;

//        case Windows.System.VirtualKey.Divide:
//            break;

//        case Windows.System.VirtualKey.F1:
//            break;

//        case Windows.System.VirtualKey.F2:
//            break;

//        case Windows.System.VirtualKey.F3:
//            break;

//        case Windows.System.VirtualKey.F4:
//            break;

//        case Windows.System.VirtualKey.F5:
//            break;

//        case Windows.System.VirtualKey.F6:
//            break;

//        case Windows.System.VirtualKey.F7:
//            break;

//        case Windows.System.VirtualKey.F8:
//            break;

//        case Windows.System.VirtualKey.F9:
//            break;

//        case Windows.System.VirtualKey.F10:
//            break;

//        case Windows.System.VirtualKey.F11:
//            break;

//        case Windows.System.VirtualKey.F12:
//            break;

//        case Windows.System.VirtualKey.F13:
//            break;

//        case Windows.System.VirtualKey.F14:
//            break;

//        case Windows.System.VirtualKey.F15:
//            break;

//        case Windows.System.VirtualKey.F16:
//            break;

//        case Windows.System.VirtualKey.F17:
//            break;

//        case Windows.System.VirtualKey.F18:
//            break;

//        case Windows.System.VirtualKey.F19:
//            break;

//        case Windows.System.VirtualKey.F20:
//            break;

//        case Windows.System.VirtualKey.F21:
//            break;

//        case Windows.System.VirtualKey.F22:
//            break;

//        case Windows.System.VirtualKey.F23:
//            break;

//        case Windows.System.VirtualKey.F24:
//            break;

//        case Windows.System.VirtualKey.NavigationView:
//            break;

//        case Windows.System.VirtualKey.NavigationMenu:
//            break;

//        case Windows.System.VirtualKey.NavigationUp:
//            break;

//        case Windows.System.VirtualKey.NavigationDown:
//            break;

//        case Windows.System.VirtualKey.NavigationLeft:
//            break;

//        case Windows.System.VirtualKey.NavigationRight:
//            break;

//        case Windows.System.VirtualKey.NavigationAccept:
//            break;

//        case Windows.System.VirtualKey.NavigationCancel:
//            break;

//        case Windows.System.VirtualKey.NumberKeyLock:
//            break;

//        case Windows.System.VirtualKey.Scroll:
//            break;

//        case Windows.System.VirtualKey.LeftShift:
//            break;

//        case Windows.System.VirtualKey.RightShift:
//            break;

//        case Windows.System.VirtualKey.LeftControl:
//            break;

//        case Windows.System.VirtualKey.RightControl:
//            break;

//        case Windows.System.VirtualKey.LeftMenu:
//            break;

//        case Windows.System.VirtualKey.RightMenu:
//            break;

//        case Windows.System.VirtualKey.GoBack:
//            break;

//        case Windows.System.VirtualKey.GoForward:
//            break;

//        case Windows.System.VirtualKey.Refresh:
//            break;

//        case Windows.System.VirtualKey.Stop:
//            break;

//        case Windows.System.VirtualKey.Search:
//            break;

//        case Windows.System.VirtualKey.Favorites:
//            break;

//        case Windows.System.VirtualKey.GoHome:
//            break;

//        case Windows.System.VirtualKey.GamepadA:
//            break;

//        case Windows.System.VirtualKey.GamepadB:
//            break;

//        case Windows.System.VirtualKey.GamepadX:
//            break;

//        case Windows.System.VirtualKey.GamepadY:
//            break;

//        case Windows.System.VirtualKey.GamepadRightShoulder:
//            break;

//        case Windows.System.VirtualKey.GamepadLeftShoulder:
//            break;

//        case Windows.System.VirtualKey.GamepadLeftTrigger:
//            break;

//        case Windows.System.VirtualKey.GamepadRightTrigger:
//            break;

//        case Windows.System.VirtualKey.GamepadDPadUp:
//            break;

//        case Windows.System.VirtualKey.GamepadDPadDown:
//            break;

//        case Windows.System.VirtualKey.GamepadDPadLeft:
//            break;

//        case Windows.System.VirtualKey.GamepadDPadRight:
//            break;

//        case Windows.System.VirtualKey.GamepadMenu:
//            break;

//        case Windows.System.VirtualKey.GamepadView:
//            break;

//        case Windows.System.VirtualKey.GamepadLeftThumbstickButton:
//            break;

//        case Windows.System.VirtualKey.GamepadRightThumbstickButton:
//            break;

//        case Windows.System.VirtualKey.GamepadLeftThumbstickUp:
//            break;

//        case Windows.System.VirtualKey.GamepadLeftThumbstickDown:
//            break;

//        case Windows.System.VirtualKey.GamepadLeftThumbstickRight:
//            break;

//        case Windows.System.VirtualKey.GamepadLeftThumbstickLeft:
//            break;

//        case Windows.System.VirtualKey.GamepadRightThumbstickUp:
//            break;

//        case Windows.System.VirtualKey.GamepadRightThumbstickDown:
//            break;

//        case Windows.System.VirtualKey.GamepadRightThumbstickRight:
//            break;

//        case Windows.System.VirtualKey.GamepadRightThumbstickLeft:
//            break;

//        default:
//            break;
//        }
//    }