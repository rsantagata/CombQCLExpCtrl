using System.Collections.Generic;
using SharedCode;


namespace EDMPlotter
{

    public static class TSVExportHelper
    {
        public static string GetTSVString(ExperimentParameters parameters, List<DataSet> dataArchive)
        {
            string tsv = "";

            for (int i = 0; i < dataArchive.Count; i++)
            {
                foreach (KeyValuePair<string, double> kv in dataArchive[i].Points[0].kvPairs)
                {
                    tsv += kv.Key + "\t";
                }
                tsv = tsv.Substring(0, tsv.Length - 1);
                tsv += "\n";
                foreach (DataPoint p in dataArchive[i].Points)
                {
                    foreach (KeyValuePair<string, double> kv in p.kvPairs)
                    {
                        tsv += kv.Value + "\t";
                    }
                    tsv = tsv.Substring(0, tsv.Length - 1);
                    tsv += "\n";
                }
                tsv += "\n";
            }

            //string csv = "{ \"params\" : " + JsonConvert.SerializeObject(parameters, Formatting.Indented);
            return tsv;
        }
    }
}