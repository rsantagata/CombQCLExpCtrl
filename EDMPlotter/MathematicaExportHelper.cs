using System;
using System.Collections.Generic;
using Wolfram.NETLink;
using System.IO;
using SharedCode;
using System.Reflection;
using System.Collections;

namespace EDMPlotter
{

    public static class MathematicaExportHelper
    {
        
        public static string PrepareDataForMMA(List<DataSet> d, ExperimentParameters parameters)
        {
            string result = "";
            result += convertParams(parameters);
            result += "ScanParams[\"NumberOfScans\"] = " + d.Count + ";\n";
            for (int i = 0; i < d.Count; i++)
            {
                result += convertData(d[i], parameters, i);
            }
            

            return result;
        }

        //Gets called recursively to parse the various parameter classes inside ExperimentParametrs.
        //An array gets special treatment so that it parses to a mathematica list correctly.
        static string convertParams(string paramLabel, Parameters p)
        {
            string localParams = "";
            foreach (PropertyInfo i in p.GetType().GetProperties())
            {
   
                object o = i.GetValue(p, null);
                if (o.GetType().IsSubclassOf(typeof(Parameters)))
                {
                    localParams += convertParams(i.Name, (Parameters)o);
                }
                else
                {
                    if (o.GetType().IsArray)
                    {
                        Type t = o.GetType();
                        object[] objs;
                        if (t.Equals(typeof(double[]))) //Fixes some odd bug where o appears as double[] instead of object[], which crashes the loop below. Cheezy.
                        {
                            objs = Array.ConvertAll<double, object> ((double[])o, obj => (object)obj);
                        }
                        else
                        {
                            objs = (object[])o;
                        }
                        localParams += paramLabel + "[\"" + i.Name + "\"] = {";
                        foreach (object oo in (object[])objs)
                        {
                            localParams += toMMAValue(oo) + ",";
                        }
                        localParams = localParams.Remove(localParams.Length - 1) + "};\n"; //Cheezy way of removing the last comma
                    }
                    else
                    {
                        localParams += paramLabel + "[\"" + i.Name + "\"] = " + toMMAValue(o) + ";\n";
                    }
                }            
            }
            return localParams;
        }

        static string convertParams(Parameters p)
        {
            string paramLabel = p.GetType().Name;
            return convertParams(paramLabel, p);
        }
        static string convertData(DataSet d, ExperimentParameters parameters, int index)
        {
            string result = "";
            double[] scanParameterValues = d.GetAllValuesOfKey(parameters.ScanParams.ScanParameterName);
            result += "data[" + (index + 1).ToString() + ", \"" + parameters.ScanParams.ScanParameterName + "\"] = {";
            foreach (double val in scanParameterValues)
            {
                result += val.ToString() + ",";
            }
            result = result.Remove(result.Length - 1); //Cheezy way of removing the last comma
            result += "};\n";

            foreach (string AIName in parameters.DAQmx.AINames)
            {
                double[] values = d.GetAllValuesOfKey(AIName);
                result += "data[" + (index + 1).ToString() + ", \"" + AIName + "\"] = {";
                foreach (double val in values)
                {
                    result += val.ToString() + ",";
                }
                result = result.Remove(result.Length - 1); //Cheezy way of removing the last comma
                result += "};\n";
            }
            return result;
        }

        private static string toMMAValue(object o)
        {
            Type t = o.GetType();
            if (t.Equals(typeof(string)))
            {
                return "\"" + o.ToString() + "\"";
            }
            else
            {
                return o.ToString();
            }
        }

        public static string CreateNotebook(string mathCommand, string fileLocation, MathKernel kernel)
        {
            //Add common functions here;
            string commonFuncs = "GetScan[index_] := Transpose[{data[index, ScanParams[\"ScanParameterName\"]], data[index, #]}] & /@ DAQmx[\"AINames\"];\n";
            commonFuncs += "PlotAll[] := Show[ListPlot[GetScan[#], ImageSize -> 600], ListPlot[GetScan[#], ImageSize -> 600, Joined -> True]] & /@ Range[1, ScanParams[\"NumberOfScans\"]];\n";

            mathCommand += commonFuncs;
            mathCommand = string.Format("{0}{1}{2}", "FullForm[ToBoxes[Defer[", mathCommand, "]]]");
            //
            mathCommand = ComputeMathCommand(mathCommand, kernel);
            mathCommand = string.Format("{0}{1}{2}", "Notebook[{Cell[\"Import\", \"Section\"], Cell[BoxData[", mathCommand, "], \"Input\"],  Cell[\"Analysis\", \"Section\"]}, WindowSize->{615, 750}, WindowMargins->{{328, Automatic}, {Automatic, 76}}]");

            File.WriteAllText(fileLocation, mathCommand);
            kernel.Dispose();
            return fileLocation;
        }

        private static string ComputeMathCommand(string command, MathKernel kernel)
        {
            kernel.Compute(command);
            return kernel.Result.ToString();
        }
    }
}