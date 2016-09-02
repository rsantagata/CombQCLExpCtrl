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
        public DataSet(DataPoint[] points)
        {
            Points = new List<DataPoint>();
            foreach(DataPoint p in points)
            {
                Points.Add(p);
            }
        }

        public int Length
        {
            get { return Points.Count; }
        }

        public void Add(DataPoint p)
        {
            Points.Add(p);
        }

        public double[] GetAllValuesOfKey(string key)
        {
            double[] values = new double[Points.Count];
            for(int i = 0; i < Points.Count; i++)
            {
                values[i] = Points[i].GetValueOfKey(key);
            }
            return values;
        }
        public DataSet GetSubset(int startIndex, int endIndex)
        {
            DataSet newSet = new DataSet();
            for(int i = startIndex; i < endIndex; i++)
            {
                newSet.Add(Points[i]);
            }
            return newSet;
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