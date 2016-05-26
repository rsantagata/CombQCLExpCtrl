using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.IO;
using Jitbit.Utils;
using Newtonsoft.Json.Linq;

namespace SharedCode
{
    public class DataSet
    {

        public List<DataPoint> Points;

        public DataSet()
        {
            Points = new List<DataPoint>();
        }

        public int Length
        {
            get { return Points.Count; }
        }

        public void Add(DataPoint p)
        {
            Points.Add(p);
        }

        public string ToJson()
        {
            return ToJson(Formatting.None);
        }
        public string ToJson(Formatting f)
        {
            JObject o = new JObject();
            JArray array = new JArray();
            for (int i = 0; i < Length; i++)
            {
                array.Add(Points[i].ToJson());
            }
            return array.ToString(f);
        }

    }
}