using NationalInstruments.DAQmx;
using SharedCode;
using System.Threading;

namespace DAQ
{
    public class DAQmxTriggeredMultiAIHardware
    {

        private AnalogMultiChannelReader analogReader;
        private Task readAIsTask;
        DataSet data;
        ExperimentParameters parameters;

        #region public region
        
        public void Dispose()
        {
            readAIsTask.Dispose();
        }

        public double[] ReadAI()
        {
            double[,] data = new double[parameters.AIAddresses.Length, parameters.NumberOfSamplesPerIntegrationTime];
            try
            {
                data = analogReader.ReadMultiSample(parameters.NumberOfSamplesPerIntegrationTime);
                readAIsTask.WaitUntilDone();
            }
            catch (DaqException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message.ToString());
                Dispose();
            }
            double[] result = new double[parameters.AIAddresses.Length];

            if(parameters.MustDemodulate)
            {
                for (int i = 0; i < result.Length; i++)
                {
                    double[] temp = new double[parameters.NumberOfSamplesPerIntegrationTime];
                    for(int j = 0; j < parameters.NumberOfSamplesPerIntegrationTime; j++)
                    {
                        temp[j] = data[i, j];
                    }
                    result[i] = NumLockIn.GetAmplitude(temp, parameters.ModulationFrequency, parameters.MaxModulationHarmonic, parameters);
                }
            }
            else
            {
                for (int i = 0; i < result.Length; i++)
                {
                    for (int j = 0; j < parameters.NumberOfSamplesPerIntegrationTime; j++)
                    {
                        result[i] += data[i, j];
                    }
                    result[i] = result[i] / parameters.NumberOfSamplesPerIntegrationTime;
                }
            }
            return result;
        }

        public void ConfigureAI(ExperimentParameters p)
        {
            parameters = p;
            data = new DataSet();
            readAIsTask = new Task("readAI");

            for (int i = 0; i < parameters.AINames.Length; i++)
            {
                readAIsTask.AIChannels.CreateVoltageChannel(parameters.AIAddresses[i], parameters.AINames[i], AITerminalConfiguration.Rse, -10, 10, AIVoltageUnits.Volts);
            }

            readAIsTask.Timing.ConfigureSampleClock(
                   "",
                   parameters.SampleRate,
                   SampleClockActiveEdge.Rising,
                   SampleQuantityMode.FiniteSamples);

            if (parameters.AutoStart == false)
            {
                readAIsTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                    parameters.TriggerAddress,
                    DigitalEdgeStartTriggerEdge.Falling);
            }
            readAIsTask.Stream.Timeout = -1;
            readAIsTask.Control(TaskAction.Verify);
            analogReader = new AnalogMultiChannelReader(readAIsTask.Stream);
        }

        #endregion
        
    }
}