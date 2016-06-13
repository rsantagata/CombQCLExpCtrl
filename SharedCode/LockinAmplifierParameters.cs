using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedCode
{
    public class LockinAmplifierParameters : Parameters
    {
        public double ModulationFrequency { get; set; }
        public int MaxModulationHarmonic { get; set; }
        public bool MustDemodulate { get; set; }

    }
}
