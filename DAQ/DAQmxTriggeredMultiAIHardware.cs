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
            double[] data = new double[parameters.AIAddresses.Length];
            try
            {
                data = analogReader.ReadSingleSample();
                readAIsTask.WaitUntilDone();
            }
            catch (DaqException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message.ToString());
                Dispose();
            }

            return data;
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
                   1/(0.001 * parameters.Sleep),
                   SampleClockActiveEdge.Rising,
                   SampleQuantityMode.FiniteSamples, parameters.NumberOfPoints == 1 ? 2 : parameters.NumberOfPoints);

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