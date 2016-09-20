using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ModernCaveMan {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page {
        public MainPage() {
            //Microsoft.IoT.AdcMcp3008.AdcMcp3008ControllerProvider chip = new Microsoft.IoT.AdcMcp3008.AdcMcp3008ControllerProvider(0);

            //chip.ReadValue(0);


            this.InitializeComponent();

            //< local:ProbeControl Grid.Column = "0" Grid.Row = "0" x: Name = "Probe1" Background = "Blue" BorderThickness = "1" ></ local:ProbeControl >
            //< local:ProbeControl Grid.Column = "1" Grid.Row = "0" x: Name = "Probe2" Background = "Green" BorderThickness = "1" ></ local:ProbeControl >
            //< local:ProbeControl Grid.Column = "0" Grid.Row = "1" x: Name = "Probe3" Background = "Red" BorderThickness = "1" ></ local:ProbeControl >
            //< local:ProbeControl Grid.Column = "1" Grid.Row = "1" x: Name = "Probe4" Background = "Yellow" BorderThickness = "1" ></ local:ProbeControl >
            //< local:ProbeControl Grid.Column = "0" Grid.Row = "2" x: Name = "Probe5" Background = "Lavender" BorderThickness = "1" ></ local:ProbeControl >
            //< local:ProbeControl Grid.Column = "1" Grid.Row = "2" x: Name = "Probe6" Background = "LawnGreen" BorderThickness = "1" ></ local:ProbeControl >
            //< local:ProbeControl Grid.Column = "0" Grid.Row = "3" x: Name = "Probe7" Background = "Purple" BorderThickness = "1" ></ local:ProbeControl >
            //< local:ProbeControl Grid.Column = "1" Grid.Row = "3" x: Name = "Probe8" Background = "Aquamarine" BorderThickness = "1" ></ local:ProbeControl >

            }
        
        }
    }