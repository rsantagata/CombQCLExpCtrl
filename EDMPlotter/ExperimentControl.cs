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

        List<DataSet> dataArchive;
        DataSet currentDataSet;
        ExperimentParameters parameters;
        public enum ExperimentState { IsStopped, IsStarting, IsRunning, IsFinishing }
        ExperimentState es;

        Thread experimentThread;

        IExperiment exp;

        public ExperimentControl(IHubConnectionContext<dynamic> clients)
        {
            Clients = clients;
            exp = new CombQCLScanHardware();
            //hardware = new FakeHardware();

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
            DataPoint currentPoint;

            ToConsole("Acquiring data...");
            int numberOfScans = 0;
            while (es.Equals(ExperimentState.IsRunning))
            {
                currentDataSet = new DataSet();
                for (int i = 0; i < parameters.ScanParams.NumberOfPoints; i++)
                {

                    currentPoint = exp.Acquire(parameters.ScanParams.ScanParameterValues[i]);
                    currentDataSet.Add(currentPoint);
                    if(es.Equals(ExperimentState.IsFinishing))
                    {
                        break;
                    }
                    //Interval between measurements
                    Thread.Sleep(parameters.ScanParams.Sleep);
                }
                //Push data down to the client like this.
                Clients.All.pushLatestData(currentDataSet.ToJson());
                dataArchive.Add(currentDataSet);
                numberOfScans++;
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