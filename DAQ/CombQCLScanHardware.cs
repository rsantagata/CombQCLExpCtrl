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

            daq = new DAQmxTriggeredMultiAIHardware();
            daq.ConfigureAI(parameters.DAQmx);
            
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
                    DataPoint d = new DataPoint(parameters.ScanParams.ScanParameterName, i);
                    //Add VISA command here.
                    //dds.Write("*IDN?\n");
                    Console.Out.Write(dds.SetFrequency(i * Math.Exp(6)));

                    //Reading AIs for this position in scan
                    d.Add(parameters.DAQmx.AINames, daq.ReadAI());
                    data.Add(d);

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