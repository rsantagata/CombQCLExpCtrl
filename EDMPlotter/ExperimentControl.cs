using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using DAQ;
using SharedCode;
using Wolfram.NETLink;


namespace EDMPlotter
{
    public class ExperimentControl
    {
        #region Declarations, constructors, accessors
        private readonly static Lazy<ExperimentControl> _instance = new Lazy<ExperimentControl>(() => new ExperimentControl(GlobalHost.ConnectionManager.GetHubContext<ExperimentHub>().Clients));

        //I deliberately keep current and averaged data separate from the archive. 
        //In time, I want to take the archive and saving functions away from this program, and over to a database.
        List<DataSet> dataArchive;
        DataSet currentDataSet;
        DataSet averagedDataSet;
        ExperimentParameters parameters;
        public enum ExperimentState { IsStopped, IsStarting, IsRunning, IsFinishing, IsPaused }
        ExperimentState es;

        Thread experimentThread;

        IExperiment exp;

        public ExperimentControl(IHubConnectionContext<dynamic> clients)
        {
            Clients = clients;
            //exp = new CombQCLScanHardware();
            exp = new FakeHardware();

            es = ExperimentState.IsStopped;
            Clients.All.toConsole("Experiment is ready.");
        }

        public static ExperimentControl Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        private IHubConnectionContext<dynamic> Clients
        {
            get;
            set;
        }
        #endregion

        #region public
        public void StartExperiment(string jsonParams)
        {
            bool IsParamsReadable = false;
            if (es.Equals(ExperimentState.IsStopped))
            {
                es = ExperimentState.IsStarting;
                ToConsole("Starting. Reading experiment parameters...");
                try
                {
                    initialiseExperimentalParameters(jsonParams);
                    IsParamsReadable = true;
                }
                catch(ExperimentalParametersException) //Catch weird parameters from UI that would crash the experiment
                {
                    ToConsole("Could not interpret experiment parameters. Please check and try again.");
                    IsParamsReadable = false;
                }
                if (IsParamsReadable)
                {
                    dataArchive = new List<DataSet>();

                    experimentThread = new Thread(new ThreadStart(run));
                    experimentThread.Start();

                    es = ExperimentState.IsRunning;
                    ToConsole("Thread started. Running experiment sequence.");

                    //Data should be coming in here; As fake data, generate a point every 0.5 seconds.
                }
            }
        }
        public void StopExperiment()
        {
            if (experimentThread.IsAlive)
            {
                ToConsole("Stopping...");
                es = ExperimentState.IsFinishing;
                experimentThread.Join();
            }
            else
            {
                ToConsole("Experiment seems to be stopped already.");
            }
        }

        public void PauseExperiment()
        {
            if (experimentThread.IsAlive)
            {
                if (es.Equals(ExperimentState.IsRunning))
                {
                    ToConsole("Pausing experiment...");
                    es = ExperimentState.IsPaused;
                }
                else if (es.Equals(ExperimentState.IsPaused))
                {
                    ToConsole("Restarting experiment...");
                    es = ExperimentState.IsRunning;
                }
            }
            else
            {
                ToConsole("Cannot Pause. Experiment doesn't seem to be running.");
            }
        }

        public void Save(string format)
        {
            Clients.All.toConsole("Preparing Export.");
            if (es.Equals(ExperimentState.IsStopped))
            {
                string outputData = "";
                if(format.Equals("JSON"))
                {
                    string path = @"C:\Users\Equipe\Documents\GitHub\CombQCLExpCtrl\EDMPlotter\tmp\tempjson.json";
                    outputData = saveDataToJSON(path);
                }
                if(format.Equals("Mathematica"))
                {
                    string path = @"C:\Users\Equipe\Documents\GitHub\CombQCLExpCtrl\EDMPlotter\tmp\tempmma.nb";
                    outputData = saveDataToMMANotebook(path);
                    
                }
                if(format.Equals("CSV"))
                {
                    string path = @"C:\Users\Equipe\Documents\GitHub\CombQCLExpCtrl\EDMPlotter\tmp\tempcsv.csv";
                    outputData = saveDataToCSV(path);
                }
                if (format.Equals("TSV"))
                {
                    string path = @"C:\Users\Equipe\Documents\GitHub\CombQCLExpCtrl\EDMPlotter\tmp\temptsv.tsv";
                    outputData = saveDataToTSV(path);
                }
                Clients.All.pushAllDataToTextArea(outputData);
                ToConsole("Data Saved.");
            }
            else
            {
                ToConsole("Cannot save data. Experiment is still running.");
            }

        }

        public void ToConsole(string s)
        {
            Clients.All.toConsole(s);
        }

        #endregion

        #region RUN
        void run()
        {
            ToConsole("Initialising hardware.");
            exp.Initialise(parameters);

            ToConsole("Acquiring data...");

            //Prepare for keeping track of average
            int numberOfScans = 0;

            while (es.Equals(ExperimentState.IsRunning) || es.Equals(ExperimentState.IsPaused))
            {
                //Tell UI to clear in preparation for new data.
                Clients.All.clearPlot();

                currentDataSet = new DataSet();
                int i = 0, iterationsSinceLastDataUpdate = 0;
                while (i < parameters.ScanParams.NumberOfPoints)
                {
                    //This is to break out mid-scan
                    if (es.Equals(ExperimentState.IsRunning))
                    {
                        //Need to call this whether acquiring or not... (otherwise DDS command doesn't get sent)
                        currentDataSet.Add(exp.SetupAndAcquire(parameters.ScanParams.ScanParameterValues[i]));
                        i++;
                        iterationsSinceLastDataUpdate++;

                        //Only send to plot if acquisition happened.
                        if (parameters.ScanParams.AcquireDataDuringScan)
                        {
                            //Cheezy solution for real time plotting. Update roughly every 0.5 seconds. If I update too often, it crashes on UI side. 
                            if (iterationsSinceLastDataUpdate * parameters.ScanParams.Sleep > 500)
                            {
                                //Push data down to the client like this.
                                Clients.All.pushLatestData(currentDataSet.GetSubset(i - iterationsSinceLastDataUpdate, i).ToJson());
                                iterationsSinceLastDataUpdate = 0;
                            }
                        }
                    }
                    else if(es.Equals(ExperimentState.IsPaused))
                    {
                        //Only send to plot if acquisition happened.
                        if (parameters.ScanParams.AcquireDataDuringScan)
                        {
                            //Plots any residual data points during pause, if there is anything to plot
                            if (iterationsSinceLastDataUpdate != 0)
                            {
                                Clients.All.pushLatestData(currentDataSet.GetSubset(i - iterationsSinceLastDataUpdate, i).ToJson());
                                iterationsSinceLastDataUpdate = 0;
                            }
                        }
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        break; //Somebody pressed the stop button during a scan.
                    }               
                }
                if (parameters.ScanParams.AcquireDataDuringScan)
                {
                    //Finished acquiring a scan
                    numberOfScans++;

                    //One day, this part will be more like: "Send to server"
                    dataArchive.Add(currentDataSet);                   

                    //Push any remaining data down to the current plot.
                    Clients.All.pushLatestData(currentDataSet.GetSubset(i - iterationsSinceLastDataUpdate, i).ToJson());

                    //Deal with averaging
                    if(numberOfScans > 1)
                    {
                        updateAveragedDataSet(currentDataSet, numberOfScans);
                        Clients.All.clearAveragePlot();
                    }
                    else
                    {
                        averagedDataSet = currentDataSet;
                        Clients.All.clearAveragePlot();
                    }
                    

                    //Push averaged data to the aveplot.
                    Clients.All.pushAverageData(averagedDataSet.ToJson());
                } 
                if(parameters.ScanParams.StopOnEOS)
                {
                    es = ExperimentState.IsFinishing;
                }
            }
            ToConsole("Acquisition/Move complete.");
            ToConsole("Disposing hardware classes...");
            exp.Dispose();
            ToConsole("Disposed.");
            ToConsole("Setting ExperimentState to stopped and closing thread...");
            es = ExperimentState.IsStopped;
        }
        #endregion

        #region private 

        string saveDataToTSV(string path)
        {
            string tsv = "";
            try
            {
                tsv = TSVExportHelper.GetTSVString(parameters, dataArchive);
                File.WriteAllText(path, tsv);
                ToConsole("Preparing new CSV file at: " + path);
                Clients.All.displayDownloadLink(path);
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }
            return tsv;
        }

        string saveDataToCSV(string path)
        {
            string csv = "";
            try
            {
                csv = CSVExportHelper.GetCSVString(parameters, dataArchive);
                File.WriteAllText(path, csv);
                ToConsole("Preparing new CSV file at: " + path);
                Clients.All.displayDownloadLink(path);
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }
            return csv;
        }

        string saveDataToJSON(string path)
        {
            string jsontext = "";
            try
            {
                jsontext = JsonExportHelper.GetJSONString(parameters, dataArchive);   
                File.WriteAllText(path, jsontext);
                ToConsole("Preparing new Json file at: " + path);
                Clients.All.displayDownloadLink(path);
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }
            return jsontext;
        }

        string saveDataToMMANotebook(string path)
        {
            string mmaFormat = "";
            try
            {
                mmaFormat = MathematicaExportHelper.PrepareDataForMMA(dataArchive, parameters);
                //Clients.All.toConsole(mmaFormat);
                string location = MathematicaExportHelper.CreateNotebook(mmaFormat, path, new MathKernel());
                ToConsole("Preparing new Mathematica notebook at: " + location);
                Clients.All.displayDownloadLink(location);
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }
            return mmaFormat;
        }

        void initialiseExperimentalParameters(string jsonParams)
        {
            try
            {
                parameters = JsonConvert.DeserializeObject<ExperimentParameters>(jsonParams);
            }
            catch (JsonException e)
            {
                Clients.All.toConsole(e.Message);
                Clients.All.toConsole("Could not initialise hardware. Stopping.");
                throw new ExperimentalParametersException();
            }

        }

        //
        void updateAveragedDataSet(DataSet newData, int numberOfScans)
        {
            for(int i = 0; i < newData.Points.Count; i++)
            {
                averagedDataSet.Points[i] = (1 / (double)numberOfScans) * ((((double)numberOfScans - 1) * averagedDataSet.Points[i]) + newData.Points[i]);
            }
        }

    }
    #endregion

    public class ExperimentalParametersException : Exception
    {
        public ExperimentalParametersException()
        {
        }

        public ExperimentalParametersException(string message) : base(message)
        {
        }

        public ExperimentalParametersException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
}