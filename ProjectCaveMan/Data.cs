using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCaveMan {
    public class Data {

        #region Properties

        public double adc { get; set; }
        public double addaRes { get; set; }
        public double cTC { get; set; }
        public double cTF { get; set; }
        public double cTK { get; set; }
        public TimeSpan sampleTime { get; set; }
        public double tC { get; set; }
        public double tF { get; set; }
        public double therm { get; set; }
        public double tK { get; set; }
        public double volts { get; set; }
        public DateTime ReadingTime { get; set; }
        #endregion Properties

        public override string ToString() {
            return string.Format("{0},{1},{2},{3},{4},{5},{6}", new string[] {
                        ReadingTime.ToString(),
                        adc.ToString(),
                        volts.ToString(),
                        therm.ToString(),
                        cTC.ToString(),
                        cTF.ToString(),
                        cTK.ToString()
                        });
            }
        }
    }