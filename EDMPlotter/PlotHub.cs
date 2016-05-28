using System;
using System.Web;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;

namespace EDMPlotter
{
    public class PlotHub : Hub
    {
        private readonly ExperimentControl _experiment;

        public PlotHub() : this(ExperimentControl.Instance) { }

        public PlotHub(ExperimentControl e)
        {
            _experiment = e;
        }

        public void StartExperiment(string parameters)
        {
            ExperimentControl.Instance.StartExperiment(parameters);
        }

        public void StopExperiment()
        {
            ExperimentControl.Instance.StopExperiment();
        }

        public void SaveExperiment(string path)
        {
            ExperimentControl.Instance.Save(path);
        }

        public void OpenMathematicaNotebook()
        {
            ExperimentControl.Instance.GenerateMMANotebook();
        }

    }



}

