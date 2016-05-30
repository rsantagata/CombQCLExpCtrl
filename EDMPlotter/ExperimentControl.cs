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
    public class ExperimentControl //: IDataDroppable
    {
        #region Declarations, constructors, accessors
        private readonly static Lazy<ExperimentControl> _instance = new Lazy<ExperimentControl>(() => new ExperimentControl(GlobalHost.ConnectionManager.GetHubContext<PlotHub>().Clients));

        List<DataSet> dataArchive;
        DataSet currentDataSet;
        ExperimentParameters parameters;
        enum ExperimentState { IsStopped, IsStarting, IsRunning, IsFinishing }
        ExperimentState es;

        object keepRunningCheckLock = new object();

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
            if (es.Equals(ExperimentState.IsStopped))
            {

                es = ExperimentState.IsStarting;
                Clients.All.toConsole("Starting...");
                initialiseExperimentalParameters(jsonParams);

                dataArchive = new List<DataSet>();

                experimentThread = new Thread(new ThreadStart(run));
                experimentThread.Start();

                es = ExperimentState.IsRunning;
                Clients.All.toConsole("Thread started. Running experiment sequence.");

                //Data should be coming in here; As fake data, generate a point every 0.5 seconds.
            }
        }
        public void StopExperiment()
        {
            if (experimentThread.IsAlive)
            {
                Clients.All.toConsole("Stopping...");
                es = ExperimentState.IsFinishing;
                experimentThread.Join();
            }
            else
            {
                Clients.All.toConsole("Experiment seems to be stopped already.");
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
                Clients.All.toConsole("Data Saved.");
            }
            else
            {
                Clients.All.toConsole("Cannot save data. Experiment is still running.");
            }

        }

        #endregion

        #region RUN
        void run()
        {
            Clients.All.toConsole("Initialising hardware.");
            exp.Initialise(parameters);

            Clients.All.toConsole("Acquiring data...");
            int numberOfScans = 0;
            while (es.Equals(ExperimentState.IsRunning))
            {
                currentDataSet = exp.Run();
                //Push data down to the client like this.
                Clients.All.pushLatestData(currentDataSet.ToJson());
                dataArchive.Add(currentDataSet);
                numberOfScans++;
            }
            Clients.All.toConsole("Acquisition complete.");
            Clients.All.toConsole("Disposing hardware classes...");
            exp.Dispose();
            Clients.All.toConsole("Disposed.");
            Clients.All.toConsole("Setting ExperimentState to stopped and closing thread...");
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
                Clients.All.toConsole("Preparing new CSV file at: " + path);
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
                Clients.All.toConsole("Preparing new CSV file at: " + path);
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
                Clients.All.toConsole("Preparing new Json file at: " + path);
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
                string location = MathematicaExportHelper.CreateNotebook(mmaFormat, path, new MathKernel(), false);
                Clients.All.toConsole("Preparing new Mathematica notebook at: " + location);
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
            Clients.All.toConsole("Reading experimental parameters...");
            try
            {
                parameters = JsonConvert.DeserializeObject<ExperimentParameters>(jsonParams);
            }
            catch (JsonException e)
            {
                Clients.All.toConsole(e.Message);
                Clients.All.toConsole("Loading default values.");
                jsonParams = @"{
            'NumberOfPoints': '1000',
            'AINames': ['x_val', 'y_val', 'y_val1', 'y_val2'],
            'AIAddresses': ['/dev1/ai1', '/dev1/ai2', '/dev1/ai3', '/dev1/ai4'],
            'AutoStart': 'false',
            'TriggerAddress': '/dev1/PFI0',
            'SampleRate': '200',
            'DDSAddress' : 'ASRL3::INSTR'
            }
            ";
                parameters = JsonConvert.DeserializeObject<ExperimentParameters>(jsonParams);
            }

        }
        /*
        public void Save(string path)
        {
            //CsvExport csv = new CsvExport();
            //TsvExport csv = new TsvExport();
            JSONExport csv = new JSONExport();
             for (int i = 0; i < Length; i++)
             {
                 csv.AddRow();
                 foreach(KeyValuePair<string, double> p in Points[i].kvPairs)
                 {
                     csv[p.Key] = p.Value;
                 }
             }
            csv.ExportToFile(@"" + path, );
        }*/
    }
    #endregion
}