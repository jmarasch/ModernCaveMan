using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Adc.Provider;
using Windows.Devices.Enumeration;
using Windows.Devices.Spi;

namespace Microsoft.IoT.AdcMcp3208 {
    public sealed class AdcMcp3208ControllerProvider : IAdcControllerProvider {
        // Our bus interface to the chip
        private SpiDevice spiController;

        const byte MCP3208_ChannelCount = 8;
        const int MCP3208_ResolutionInBits = 12;
        const int MCP3208_MinValue = 0;
        const int MCP3208_MaxValue = 4096;

        // ADC chip operation constants
        const byte MCP3208_SingleEnded = 0x08;

        const int DEFAULT_SPI_CHIP_SELECT_LINE = 0;  // SPI0 CS0 pin 24 on the RPi2

        static public int DefaultChipSelectLine {
            get { return DEFAULT_SPI_CHIP_SELECT_LINE; }
            }


        const int MCP3208_Clock = 1350000;

        private readonly Task _initializingTask;
        public AdcMcp3208ControllerProvider(int chipSelectLine) : base() {
            _initializingTask = Init(chipSelectLine);
            }

        private async Task Init(int chipSelectLine) {
            try {
                // Setup the SPI bus configuration
                var settings = new SpiConnectionSettings(chipSelectLine);

                settings.ClockFrequency = MCP3208_Clock;
                settings.Mode = SpiMode.Mode0;

                // Ask Windows for the list of SpiDevices

                // Get a selector string that will return all SPI controllers on the system 
                string aqs = SpiDevice.GetDeviceSelector();

                // Find the SPI bus controller devices with our selector string           
                var dis = await DeviceInformation.FindAllAsync(aqs);

                // Create an SpiDevice with our bus controller and SPI settings           
                spiController = await SpiDevice.FromIdAsync(dis[0].Id, settings);

                if (spiController == null) {
                    Debug.WriteLine(
                        "SPI Controller {0} is currently in use by another application. Please ensure that no other applications are using SPI.",
                        dis[0].Id);
                    throw new Exception();
                    }

                } catch (Exception e) {
                Debug.WriteLine("Exception: " + e.Message + "\n" + e.StackTrace);
                throw;
                }

            }



        public int ChannelCount {
            get { return MCP3208_ChannelCount; }
            }

        ProviderAdcChannelMode channelMode = ProviderAdcChannelMode.SingleEnded;
        public ProviderAdcChannelMode ChannelMode {
            get { return channelMode; }
            set { channelMode = value; }
            }

        public int MaxValue {
            get { return MCP3208_MaxValue; }
            }

        public int MinValue {
            get { return MCP3208_MinValue; }
            }

        public int ResolutionInBits {
            get { return MCP3208_ResolutionInBits; }
            }

        public bool IsChannelModeSupported(ProviderAdcChannelMode channelMode) {
            return (channelMode == ProviderAdcChannelMode.SingleEnded || channelMode == ProviderAdcChannelMode.Differential);
            }

        public int ReadValue(int channelNumber) {
            /* mcp3208 is 12 bits output */
            // To line everything up for ease of reading back (on byte boundary) we 
            // will pad the command start bit with 5 leading "0" bits

            // Write 0000 0SGD DDxx xxxx xxxx xxxx
            // Read  ???? ???? ???N BA98 7654 3210
            // S = start bit
            // G = Single / Differential
            // D = Chanel data 
            // ? = undefined, ignore
            // N = 0 "Null bit"
            // B-0 = 12 data bits

            byte[] readBuffer = new byte[3] { 0x00, 0x00, 0x00 };
            byte[] writeBuffer = new byte[3];

            switch (channelNumber) {
                case 0:
                    //1 0 0 0 single - ended CH0
                    // 0000 0110 = 5 pad bits, start bit, single ended, channel bit 2
                    // 0000 0000 = channel bit 1, channel bit 0, 6 clocking bits
                    // 0000 0000 = 8 clocking bits
                    writeBuffer = new byte[3] { 0x06, 0x00, 0x00 };
                    break;
                case 1:
                    //1 0 0 1 single - ended CH1
                    // 0000 0110 = 5 pad bits, start bit, single ended, channel bit 2
                    // 0100 0000 = channel bit 1, channel bit 0, 6 clocking bits
                    // 0000 0000 = 8 clocking bits
                    writeBuffer = new byte[3] { 0x06, 0x40, 0x00 };
                    break;
                case 2:
                    //1 0 1 0 single - ended CH2 
                    // 0000 0110 = 5 pad bits, start bit, single ended, channel bit 2
                    // 1000 0000 = channel bit 1, channel bit 0, 6 clocking bits
                    // 0000 0000 = 8 clocking bits
                    writeBuffer = new byte[3] { 0x06, 0x80, 0x00 };
                    break;
                case 3:
                    //1 0 1 1 single - ended CH3 
                    // 0000 0110 = 5 pad bits, start bit, single ended, channel bit 2
                    // 1100 0000 = channel bit 1, channel bit 0, 6 clocking bits
                    // 0000 0000 = 8 clocking bits
                    writeBuffer = new byte[3] { 0x06, 0xC0, 0x00 };
                    break;
                case 4:
                    //1 1 0 0 single - ended CH4 
                    // 0000 0111 = 5 pad bits, start bit, single ended, channel bit 2
                    // 0000 0000 = channel bit 1, channel bit 0, 6 clocking bits
                    // 0000 0000 = 8 clocking bits
                    writeBuffer = new byte[3] { 0x07, 0x00, 0x00 };
                    break;
                case 5:
                    //1 1 0 1 single - ended CH5 
                    // 0000 0111 = 5 pad bits, start bit, single ended, channel bit 2
                    // 0100 0000 = channel bit 1, channel bit 0, 6 clocking bits
                    // 0000 0000 = 8 clocking bits
                    writeBuffer = new byte[3] { 0x07, 0x40, 0x00 };
                    break;
                case 6:
                    //1 1 1 0 single - ended CH6 
                    // 0000 0111 = 5 pad bits, start bit, single ended, channel bit 2
                    // 1000 0000 = channel bit 1, channel bit 0, 6 clocking bits
                    // 0000 0000 = 8 clocking bits
                    writeBuffer = new byte[3] { 0x07, 0x80, 0x00 };
                    break;
                case 7:
                    //1 1 1 1 single - ended CH7 
                    // 0000 0111 = 5 pad bits, start bit, single ended, channel bit 2
                    // 1100 0000 = channel bit 1, channel bit 0, 6 clocking bits
                    // 0000 0000 = 8 clocking bits
                    writeBuffer = new byte[3] { 0x07, 0xC0, 0x00 };
                    break;
                }

            spiController.TransferFullDuplex(writeBuffer, readBuffer);

            int sample = readBuffer[2] + ((readBuffer[1] & 0x0F) << 8);

            /* mcp3208 is 12 bits output */
            //sample = readBuf[1] & 0x0F;
            //sample <<= 8;
            //sample += readBuf[2];

            return sample;
            }

        uint channelStatus;

        public void AcquireChannel(int channel) {
            uint oldChannelStatus = channelStatus;
            uint channelToAquireFlag = (uint)(1 << channel);

            // See if the channel is available
            if ((oldChannelStatus & channelToAquireFlag) == 0) {
                // Not currently acquired
                channelStatus |= channelToAquireFlag;
                } else {
                // Already acquired, throw an exception
                throw new UnauthorizedAccessException();
                }
            }

        public void ReleaseChannel(int channel) {
            uint oldChannelStatus = channelStatus;
            uint channelToAquireFlag = (uint)(1 << channel);

            channelStatus &= ~channelToAquireFlag;
            }
        }
    }
