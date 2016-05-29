using System.Collections.Generic;
using SharedCode;
using Newtonsoft.Json;


namespace EDMPlotter
{

    public static class JsonExportHelper
    {
        public static string GetJSONString(ExperimentParameters parameters, List<DataSet> dataArchive)
        {
            string jsontext = "{ \"params\" : " + JsonConvert.SerializeObject(parameters, Formatting.Indented);
            for (int i = 0; i < dataArchive.Count; i++)
            {
                jsontext += ", \"data_" + i.ToString() + "\": " + dataArchive[i].ToJson(Formatting.Indented);
            }
            jsontext += "}";
            return jsontext;
        }
    }
}