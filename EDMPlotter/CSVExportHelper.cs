using System.Collections.Generic;
using SharedCode;


namespace EDMPlotter
{

    public static class CSVExportHelper
    {
        public static string GetCSVString(ExperimentParameters parameters, List<DataSet> dataArchive)
        {
            string csv = "";

            for (int i = 0; i < dataArchive.Count; i++)
            {
                foreach (KeyValuePair<string, double> kv in dataArchive[i].Points[0].kvPairs)
                {
                    csv += kv.Key + ",";
                }
                csv = csv.Substring(0, csv.Length - 1);
                csv += "\n";
                foreach (DataPoint p in dataArchive[i].Points)
                {
                    foreach(KeyValuePair<string, double> kv in p.kvPairs)
                    {
                        csv += kv.Value + ",";
                    }
                    csv = csv.Substring(0, csv.Length - 1);
                    csv += "\n";
                }
                csv += "\n";
            }
            
            //string csv = "{ \"params\" : " + JsonConvert.SerializeObject(parameters, Formatting.Indented);
            return csv;
        }
    }
}