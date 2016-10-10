using Microsoft.IoT.Lightning.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices;
using Windows.Devices.Gpio;
using Windows.Devices.Pwm;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace LCDDisplayAndButtnTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        int pinRn = 13,
            pinGn = 15,
            pinBn = 16,
            pinL  = 29,
            pinR  = 31,
            pinU  = 32,
            pinD  = 33,
            pinS  = 37;

        PwmPin ledR, ledG, ledB;
        GpioPin btnL, btnR, btnU, btnD, btnS;

        private async void InitPWM() {
            if (LightningProvider.IsLightningEnabled) {
                LowLevelDevicesController.DefaultProvider = LightningProvider.GetAggregateProvider();

                var pwmControllers = await PwmController.GetControllersAsync(LightningPwmProvider.GetPwmProvider());
                var pwmController = pwmControllers[1]; // use the on-device controller
                pwmController.SetDesiredFrequency(50); // try to match 50Hz

                ledR = pwmController.OpenPin(pinRn);
                ledR.SetActiveDutyCyclePercentage(1);
                ledG = pwmController.OpenPin(pinGn);
                ledG.SetActiveDutyCyclePercentage(1);
                ledB = pwmController.OpenPin(pinBn);
                ledB.SetActiveDutyCyclePercentage(1);

                ledR.Start();
                ledG.Start();
                ledB.Start();
                }
            }

        private async void InitGPOI() {
            var gpioController = await GpioController.GetDefaultAsync();
            if (gpioController == null) {
                //Console. "There is no GPIO controller on this device.";
                return;
                }
            btnL = gpioController.OpenPin(pinL);
            btnL.SetDriveMode(GpioPinDriveMode.Input);
            btnR = gpioController.OpenPin(pinR);
            btnR.SetDriveMode(GpioPinDriveMode.Input);
            btnU = gpioController.OpenPin(pinU);
            btnU.SetDriveMode(GpioPinDriveMode.Input);
            btnD = gpioController.OpenPin(pinD);
            btnD.SetDriveMode(GpioPinDriveMode.Input);
            btnS = gpioController.OpenPin(pinS);
            btnS.SetDriveMode(GpioPinDriveMode.Input);

            btnL.ValueChanged += BtnS_ValueChanged;
            btnR.ValueChanged += BtnS_ValueChanged;
            btnU.ValueChanged += BtnS_ValueChanged;
            btnD.ValueChanged += BtnS_ValueChanged;
            btnS.ValueChanged += BtnS_ValueChanged;
            }

        private void BtnS_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args) {
            //throw new NotImplementedException();
            }
        }
}
