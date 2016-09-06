using NationalInstruments.DAQmx;
using SharedCode;
using System.Threading;
using System;

namespace DAQ
{
    public class CombQCLScanHardware : IExperiment
    {
        USBVisaDDS dds;
        DAQmxTriggeredMultiAIHardware daq;
        object updateDataLock = new object();

        ExperimentParameters parameters;

        public CombQCLScanHardware() { }

        #region public region

        public void Initialise(ExperimentParameters p)
        {
            parameters = p;

            if (p.ScanParams.AcquireDataDuringScan)
            {
                daq = new DAQmxTriggeredMultiAIHardware();
                daq.ConfigureAI(parameters.DAQmx);
            }

            dds = new USBVisaDDS(parameters.DDS.DDSAddress);
            dds.Connect();

        }

        public DataPoint SetupAndAcquire(double scanParameterValue)
        {
            DataPoint p = new DataPoint(parameters.ScanParams.ScanParameterName, scanParameterValue, "Unacquired");

            //This is where the parameter is sent to DDS.
            dds.SetFrequency(scanParameterValue);

            //Interval between measurements. Want the sleep to be between 'frequency change' command and acquisition
            // (for the laser to settle at new frequency and lock-in to average inputs)
            Thread.Sleep(parameters.ScanParams.Sleep);

            //Reading AIs for this position in scan. 
            //Note! No matter how many measurements are performed in daq.ReadAI, this only takes one number per channel.
            //Any averaging has to happen before getting added to d.
            p.Add(parameters.DAQmx.AINames, daq.ReadAI(), DateTime.UtcNow.ToString("{dd/MM/yyy HH:mm:ss.fff}"));
            return p;
        }

        public void Dispose()
        {
            daq.Dispose();
            dds.Disconnect();
        }

        #endregion

    }
}