using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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

        PlotHub hub;

        DataSet dataSet;
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

        public void Save(string path)
        {
            Clients.All.toConsole("Saving to: " + path);
            if (es.Equals(ExperimentState.IsStopped))
            {
                saveData(path);
                Clients.All.toConsole("Data Saved.");
            }
            else
            {
                Clients.All.toConsole("Cannot save data. Experiment is still running.");
            }

        }

        public void GenerateMMANotebook()
        {
            string mmaFormat = MMANotebookHelper.PrepareDataForMMA(dataSet, parameters);
            Clients.All.toConsole(mmaFormat);
            string location = MMANotebookHelper.CreateNotebook(mmaFormat, @"C:/data/mmatest.nb", new MathKernel(), true);
            Clients.All.toConsole("Preparing new Mathematica notebook at: " + location);
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
                dataSet = exp.Run();
                //Push data down to the client like this.
                Clients.All.pushData(dataSet.ToJson());
                if (parameters.EOSSave)
                {
                    string path = Path.GetDirectoryName(parameters.SavePath);
                    string extension = Path.GetExtension(parameters.SavePath);
                    string fileName = Path.GetFileNameWithoutExtension(parameters.SavePath);

                    saveData(path + "\\" + fileName + "_" + numberOfScans.ToString() + extension);
                }
                if (parameters.EOSStop)
                {
                    es = ExperimentState.IsFinishing;
                }
                else
                {
                    numberOfScans++;
                }

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

        void saveData(string path)
        {
            try
            {
                File.WriteAllText(path, "{ \"params\" : " + JsonConvert.SerializeObject(parameters, Formatting.Indented) + ", \"data\": " + dataSet.ToJson(Formatting.Indented) + "}");

                //Use this if saving (only!) the data to a CSV/TSV file. Turned off for the moment.
                //dataSet.Save(path);
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }
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
            'EOSStop' : true,
            'EOSSave' : false,
            'SavePath' : '',
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