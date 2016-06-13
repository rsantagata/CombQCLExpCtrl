using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedCode
{
    public class DAQmxParameters : Parameters
    {
        public string[] AINames { get; set; }
        public string[] AIAddresses { get; set; }
        public bool AutoStart { get; set; }
        public string TriggerAddress { get; set; }
        public int SampleRate { get; set; }
        public int NumberOfSamplesPerIntegrationTime { get; set; }
        public LockinAmplifierParameters LockinParams { get; set; }
    }
}
