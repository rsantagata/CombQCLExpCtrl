﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SharedCode
{
    public class DataPoint
    {
        public List<KeyValuePair<string, double>> kvPairs;

        public DataPoint(string name, double val)
        {
            kvPairs = new List<KeyValuePair<string, double>>();
            kvPairs.Add(new KeyValuePair<string, double>(name, val));
        }

        public DataPoint(string[] names, double[] vals)
        {
            kvPairs = new List<KeyValuePair<string, double>>();
            for (int i = 0; i < names.Length; i++)
            {
                kvPairs.Add(new KeyValuePair<string, double>(names[i], vals[i]));
            }
        }

        public int Dimensions()
        {
            return kvPairs.Count;
        }
        public JObject ToJson()
        {
            JObject o = new JObject();
            for(int i = 0; i < Dimensions(); i++)
            {
                o[kvPairs[i].Key] = kvPairs[i].Value;
            }
            return o;
        }

        public void Add(string[] names, double[] vals)
        {
            for (int i = 0; i < names.Length; i++)
            {
                kvPairs.Add(new KeyValuePair<string, double>(names[i], vals[i]));
            }
        }

        public double GetValueOfKey(string key)
        {
            foreach(KeyValuePair<string, double> p in kvPairs)
            {
                if(p.Key.Equals(key))
                {
                    return p.Value;
                }
            }
            ArgumentException a = new ArgumentException("No value found for this key.");
            throw a;
           
        }

        public string[] GetAllKeys()
        {
            string[] keys = new string[kvPairs.Count];
            for(int i = 0; i < keys.Length; i++)
            {
                keys[i] = kvPairs[i].Key;
            }
            return keys;
        }
        public double[] GetAllValues()
        {
            string[] keys = GetAllKeys();
            double[] values = new double[keys.Length];
            for(int i = 0; i < values.Length; i++)
            {
                values[i] = GetValueOfKey(keys[i]); 
            }
            return values;
        }

        //Don't use this unless you're sure that the two points have the same number of kv pairs.
        public static DataPoint operator+ (DataPoint a, DataPoint b)
        {
            string[] keys = a.GetAllKeys();
            double[] aValues = a.GetAllValues();
            double[] bValues = b.GetAllValues();
            double[] sumValues = new double[aValues.Length];
            for(int i = 0; i < sumValues.Length; i++)
            {
                sumValues[i] = aValues[i] + bValues[i];
            }
            DataPoint newPoint = new DataPoint(keys, sumValues);
            return newPoint;
        }

        public static DataPoint operator* (double val, DataPoint p)
        {
            string[] keys = p.GetAllKeys();
            double[] values = p.GetAllValues();
            double[] newValues = new double[values.Length];
            for (int i = 0; i < newValues.Length; i++)
            {
            }
            DataPoint newPoint = new DataPoint(keys, newValues);
            return newPoint;
        }
        public static DataPoint operator*(DataPoint p, double val)
        {
            return val * p;
        }


    }

}