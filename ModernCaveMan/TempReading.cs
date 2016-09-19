using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernCaveMan {
    public class TempReading {
        public DateTime ReadingTime { get; set; }

        public double TempF { get; set; }
        public double TempC { get; set; }
        public double TempK { get; set; }

        //need to find formula for converting thermister reading to temperature

        public int ReadingValue { get; set; }

        }
    }
