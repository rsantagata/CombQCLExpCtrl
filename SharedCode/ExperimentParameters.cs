using System.Reflection;

namespace SharedCode
{
    public class ExperimentParameters : Parameters
    {
        public ScanParameters ScanParams { get; set; }
        public DDSParameters DDS { get; set; }
        public DAQmxParameters DAQmx { get; set; }

    }
}