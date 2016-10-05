using System.Collections.ObjectModel;
using Windows.Storage;
using System;
using System.IO;
using System.Text;

namespace ProjectCaveMan {
    public class ProbeData {

        #region Fields

        private byte[] ch0 = new byte[3] { 0x06, 0x00, 0x00 };
        private byte[] ch1 = new byte[3] { 0x06, 0x40, 0x00 };
        private byte[] ch2 = new byte[3] { 0x06, 0x80, 0x00 };
        private byte[] ch3 = new byte[3] { 0x06, 0xC0, 0x00 };
        private byte[] ch4 = new byte[3] { 0x07, 0x00, 0x00 };
        private byte[] ch5 = new byte[3] { 0x07, 0x40, 0x00 };
        private byte[] ch6 = new byte[3] { 0x07, 0x80, 0x00 };
        private byte[] ch7 = new byte[3] { 0x07, 0xC0, 0x00 };

        private int channelID;
        private StorageFile dataFile;
        private ObservableCollection<Data> logData = new ObservableCollection<Data>();

        #endregion Fields

        #region Constructors

        //private string name;
        public ProbeData(string name, int channel) {
            switch (channel) {
                case 0:
                    writeBuffer = ch0;
                    break;
                case 1:
                    writeBuffer = ch1;
                    break;
                case 2:
                    writeBuffer = ch2;
                    break;
                case 3:
                    writeBuffer = ch3;
                    break;
                case 4:
                    writeBuffer = ch4;
                    break;
                case 5:
                    writeBuffer = ch5;
                    break;
                case 6:
                    writeBuffer = ch6;
                    break;
                case 7:
                    writeBuffer = ch7;
                    break;

                }
            channelID = channel;

            Name = name;

            logData.CollectionChanged += LogData_CollectionChanged;
            }

        public ProbeData(byte[] channel, string ProbeName) {
            writeBuffer = channel;
            }

        #endregion Constructors

        #region Properties

        public int ChannelID { get { return channelID; } }

        public StorageFile DataFile {
            get {
                return dataFile;
                }
            set {
                dataFile = value;
                SetupFile();
                }
            }

        public ObservableCollection<Data> LogData {
            get { return logData; }
            }

        public string Name { get; set; }

        public double reading { get; set; }

        public byte[] writeBuffer { get; }

        #endregion Properties

        #region Methods

        private async void LogData_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    try {
                        if (dataFile == null) break;

                        //String s = "hello";
                        //string writeLine = logData[0].ToString() + Environment.NewLine;

                        Byte[] bytes = Encoding.UTF8.GetBytes(logData[0].ToString() + Environment.NewLine);
                        // StorageStreamTransaction sts = await ((StorageStreamTransaction)dataFile.OpenTransactedWriteAsync()).Stream.AsStream();

                        using (Stream f = await dataFile.OpenStreamForWriteAsync()) {
                            f.Seek(0, SeekOrigin.End);
                            await f.WriteAsync(bytes, 0, bytes.Length);
                            
                            }
                        } catch (Exception ex) {

                        throw;
                        }

                    //await FileIO.WriteLinesAsync(DataFile, new[] { writeLine });
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    break;
                }

            }

        private async void SetupFile() {
            string[] writeLine = new string[] { string.Format("Date,ProbeChanel,ProbeName"),
                                                string.Format("{0},{1},{2}",DateTime.Now,ChannelID,Name),
                                                string.Format("Date,ADC,VOLTS,THERM,CALC-C,CALC-F,CALC-K")                                                
                };//LogData[0].cTF.ToString()
            await FileIO.WriteLinesAsync(DataFile, writeLine);
            }

        public override string ToString() {
            return string.Format("{0}-CH{1}", Name, ChannelID);
            }

        #endregion Methods
        }
    }
