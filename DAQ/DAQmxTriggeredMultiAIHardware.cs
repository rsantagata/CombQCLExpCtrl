using NationalInstruments.DAQmx;
using SharedCode;

namespace DAQ
{
    public class DAQmxTriggeredMultiAIHardware : IExperimentHardware
    {

        object updateDataLock = new object();

        private AnalogMultiChannelReader analogReader;

        private Task readAIsTask;

        DataSet data;
        ExperimentParameters parameters;

        #region public region

        public void Initialise(ExperimentParameters p)
        {
            parameters = p;
            configureReadAI(parameters.NumberOfPoints, parameters.AutoStart);
        }

        public DataSet Run()
        {
            data = new DataSet();
            double[,] d = readAI(parameters.NumberOfPoints);
            double[] instantD = new double[parameters.AIAddresses.Length];
            lock (updateDataLock)
            {
                for (int i = 0; i < parameters.NumberOfPoints; i++)
                {
                    for (int j = 0; j < parameters.AIAddresses.Length; j++)
                    {
                        instantD[j] = d[j, i];
                    }
                    data.Add(new DataPoint(parameters.AINames, instantD));
                }
            }
            return data;
        }

        public void Dispose()
        {
            readAIsTask.Dispose();
        }

        #endregion


        #region private region

        void configureReadAI(int numberOfMeasurements, bool autostart)
        {
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
                   SampleQuantityMode.FiniteSamples, numberOfMeasurements);

            if (autostart == false)
            {


                readAIsTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                    parameters.TriggerAddress,
                    DigitalEdgeStartTriggerEdge.Falling);
            }
            readAIsTask.Stream.Timeout = -1;
            readAIsTask.Control(TaskAction.Verify);
            analogReader = new AnalogMultiChannelReader(readAIsTask.Stream);
        }


        double[,] readAI(int numberOfMeasurements)
        {
            double[,] data = new double[parameters.AIAddresses.Length, numberOfMeasurements];
            try
            {
                data = analogReader.ReadMultiSample(numberOfMeasurements);
                readAIsTask.WaitUntilDone();
            }
            catch (DaqException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message.ToString());
                Dispose();
            }

            return data;
        }

        #endregion

    }
}