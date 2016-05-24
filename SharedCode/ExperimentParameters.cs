using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace SharedCode
{
    public class ExperimentParameters
    {
        public int NumberOfPoints { get; set; }
        public string[] AINames { get; set; }
        public string[] AIAddresses { get; set; }
        public bool AutoStart { get; set; }
        public string TriggerAddress { get; set; }
        public int SampleRate { get; set; }
        public bool EOSStop { get; set; }
        public bool EOSSave { get; set; }
        public string SavePath { get; set; }
    }
}