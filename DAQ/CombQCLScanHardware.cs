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

        DataSet data;
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

        public DataSet Run()
        {
            data = new DataSet();
            lock (updateDataLock)
            {
                for (int i = 0; i < parameters.ScanParams.NumberOfPoints; i++)
                {
                    //This part of the experiment doesn't know if we're actually scanning. It simply writes the sequence of values passed over from the UI.
                    //(Note:index.html is written so that ScanParameterName tells you if you were actually scanning.)
                    double scanParameterValue = parameters.ScanParams.ScanParameterValues[i];
                    DataPoint d = new DataPoint(parameters.ScanParams.ScanParameterName, scanParameterValue);
                    
                    //This is where the parameter is sent to DDS.
                    dds.SetFrequency(scanParameterValue);

                    if (parameters.ScanParams.AcquireDataDuringScan)
                    {
                        //Reading AIs for this position in scan. 
                        //Note! No matter how many measurements are performed in daq.ReadAI, this only takes one number per channel.
                        //Any averaging has to happen before getting added to d.
                        d.Add(parameters.DAQmx.AINames, daq.ReadAI());
                        data.Add(d);
                    }
                    //Interval between measurements
                    Thread.Sleep(parameters.ScanParams.Sleep);
                }
            }
            return data;
        }

        public void Dispose()
        {
            daq.Dispose();
            dds.Disconnect();
        }

        #endregion
        
    }
}