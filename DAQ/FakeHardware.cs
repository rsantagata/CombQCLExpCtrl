using NationalInstruments.DAQmx;
using SharedCode;
using System.Threading;
using System;

namespace DAQ
{
    public class FakeHardware : IExperiment
    {
        object updateDataLock = new object();

        ExperimentParameters parameters;

        public DataPoint Acquire(double scanParameterValue)
        {
            DataPoint p = new DataPoint(parameters.ScanParams.ScanParameterName, scanParameterValue);

            //Interval between measurements. Want the sleep to be between 'frequency change' command and acquisition
            // (for the laser to settle at new frequency and lock-in to average inputs)
            Thread.Sleep(parameters.ScanParams.Sleep);

            //Generates an array of 0 as data.
            Random r = new Random();
            double[] emptyData = new double[parameters.DAQmx.AINames.Length];
            for(int i = 0; i < emptyData.Length; i++)
            {
                emptyData[i] = r.Next();
            }
            p.Add(parameters.DAQmx.AINames, emptyData);

            return p;
        }

        public void Dispose()
        {
            
        }

        public void Initialise(ExperimentParameters p)
        {
            parameters = p;
        }
    }
}
