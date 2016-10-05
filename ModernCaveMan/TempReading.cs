using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModernCaveMan.ThermisterSettings;
using Microsoft.IoT;

namespace ModernCaveMan {

    public enum TempScale {
        F, //Ferinheight 
        C, //Celsius
        K, //Kelvin
        R, //Raw resistance
        A //Raw ADC reading
        }
    //public class TempReadings : ObservableCollection<TempReading> {
    //    private Dictionary<double, double> lookuptable = Senstech100k25c.ThermisterTable;

    //    public TempScale Scale { get; set; }

    //    public double GetReading(int index, TempScale scale, int ADCMax, double fixedOhms) {
    //        double reading = 0;
    //        double thermisterOhms;
    //        double deg;
    //        double adcReading = this[index].ReadingValue;
    //        if (scale == TempScale.A) return adcReading;

    //        // convert the value to resistance
    //        reading = (ADCMax / adcReading) - 1;
    //        thermisterOhms = fixedOhms / reading;
    //        if (scale == TempScale.R) return thermisterOhms;

    //        double tempA, tempB;

    //        if (thermisterOhms < lookuptable.Last().Key) thermisterOhms = lookuptable.Last().Key + .000001;
    //        if (thermisterOhms > lookuptable.First().Key) thermisterOhms = lookuptable.First().Key - .000001;

    //        tempA = lookuptable.First(r => thermisterOhms > r.Key).Value;
    //        tempB = lookuptable.Last(r => thermisterOhms < r.Key).Value;

    //        deg = (tempA + tempB) / 2;

    //        if(scale == TempScale.F) return deg * 9 / 5 + 32;
    //        if (scale == TempScale.K) return deg + 273.15;
    //        return deg;
    //        }
    //    }

    public class TempReading {
        public DateTime ReadingTime { get; set; }

        public double TempF { get; set; }
        public double TempC { get; set; }
        public double TempK { get; set; }
        public double Ohms  { get; set; }
        public double Volts { get; set; }
        public int    ADC   { get; set; }
        //need to find formula for converting thermister reading to temperature

        //reading stored as adc value
        //public double ReadingValue { get; set; }

        }
    }
