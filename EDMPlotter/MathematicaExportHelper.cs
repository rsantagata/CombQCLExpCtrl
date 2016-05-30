using System;
using System.Collections.Generic;
using Wolfram.NETLink;
using System.IO;
using SharedCode;
using System.Reflection;

namespace EDMPlotter
{

    public static class MathematicaExportHelper
    {
        
        public static string PrepareDataForMMA(List<DataSet> d, ExperimentParameters parameters)
        {
            string result = "";
            result += convertParams(parameters);
            result += "params[\"NumberOfScans\"] = " + d.Count + ";\n";
            for (int i = 0; i < d.Count; i++)
            {
                result += convertData(d[i], parameters, i);
            }
            

            return result;
        }

        static string convertParams(ExperimentParameters p)
        {
            string parameters = "";
            foreach (PropertyInfo i in p.GetType().GetProperties())
            {
                object o = i.GetValue(p, null);
                if(o.GetType().IsArray)
                {
                    parameters += "params[\"" + i.Name + "\"] = {";
                    foreach (object obj in (object[])o)
                    {
                        parameters += toMMAValue(obj) + ",";
                    }
                    parameters = parameters.Remove(parameters.Length - 1) + "};\n"; //Cheezy way of removing the last comma
                }
                else
                {
                    parameters += "params[\"" + i.Name + "\"] = " + toMMAValue(o) + ";\n";
                }                
            }
            return parameters;
        }
        static string convertData(DataSet d, ExperimentParameters parameters, int index)
        {
            string result = "";
            double[] scanParameterValues = d.GetAllValuesOfKey(parameters.ScanParameter);
            result += "data[" + (index + 1).ToString() + ", \"" + parameters.ScanParameter + "\"] = {";
            foreach (double val in scanParameterValues)
            {
                result += val.ToString() + ",";
            }
            result = result.Remove(result.Length - 1); //Cheezy way of removing the last comma
            result += "};\n";

            foreach (string AIName in parameters.AINames)
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
            string commonFuncs = "getScan[index_] := Transpose[{data[index, params[\"ScanParameter\"]], data[index, #]}] & /@ params[\"AINames\"];\n";
            commonFuncs += "PlotAll[] := Show[ListPlot[getScan[#], ImageSize -> 600], ListPlot[getScan[#], ImageSize -> 600, Joined -> True]] & /@ Range[1, params[\"NumberOfScans\"]]";

            mathCommand += commonFuncs;
            mathCommand = string.Format("{0}{1}{2}", "FullForm[ToBoxes[Defer[", mathCommand, "]]]");

            mathCommand = ComputeMathCommand(mathCommand, kernel);
            
            mathCommand = string.Format("{0}{1}{2}", "Notebook[{Cell[\"Import\", \"Section\"], Cell[BoxData[", mathCommand, "], \"Input\"],  Cell[\"Analysis\", \"Section\"]}, WindowSize->{615, 750}, WindowMargins->{{328, Automatic}, {Automatic, 76}}, StyleDefinitions->\"Default.nb\"]");

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