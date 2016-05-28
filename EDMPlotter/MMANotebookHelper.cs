using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wolfram.NETLink;
using System.IO;
using SharedCode;
using System.Reflection;

namespace EDMPlotter
{

    public static class MMANotebookHelper
    {
        public static string PrepareDataForMMAOld(DataSet d, ExperimentParameters parameters)
        {
            string result = "";
            foreach(PropertyInfo p in parameters.GetType().GetRuntimeProperties())
            {
                object o = p.GetValue(parameters, null);
                if (o.GetType().IsArray)
                {
                    result += ToMMASymbol(p.Name) + " = {";
                    foreach (object obj in (object[])o)
                    {
                        result += ToMMAValue(obj);
                    }
                    result = result.Remove(result.Length - 1) + "};\n"; //Cheezy way of removing the last comma
                }
                else
                {
                    object obj = p.GetValue(parameters, null);
                    if (obj.GetType().Equals(typeof(string)))
                    {
                        result += ToMMASymbol(p.Name) + " = " + "\"" + obj.ToString() + "\"" + ";\n";
                    }
                   else
                    {
                        result += ToMMASymbol(p.Name) + " = " + obj.ToString() + ";\n";
                    }
                }
                
            }
            
            double[] scanParameterValues = d.GetAllValuesOfKey(parameters.ScanParameter);
            result += ToMMASymbol(parameters.ScanParameter) + " = {";
            foreach(double val in scanParameterValues)
            {
                result += val.ToString() + ",";
            }
            result = result.Remove(result.Length - 1); //Cheezy way of removing the last comma
            result += "};\n";

            foreach (string AIName in parameters.AINames)
            {
                double[] values = d.GetAllValuesOfKey(AIName);
                result += ToMMASymbol(AIName) + " = {";
                foreach (double val in values)
                {
                    result += val.ToString() + ",";
                }
                result = result.Remove(result.Length - 1); //Cheezy way of removing the last comma
                result += "};\n";
            }
            
            return result;
        }
        
        private static string ToMMAValue(object o)
        {
            Type t = o.GetType();
            if(t.Equals(typeof(string)))
            {
                return "\"" + o.ToString() + "\"";
            }
            else
            {
                return o.ToString();
            }
        }
        private static string ToMMASymbol(string s)
        {
            var charsToRemove = new string[] { "@", ",", ".", ";", "'" };
            foreach (var c in charsToRemove)
            {
                s = s.Replace(c, string.Empty);
            };
            return s;
        }

        public static string PrepareDataForMMA(DataSet d, ExperimentParameters parameters)
        {
            string result = "";
            result += convertParams(parameters);
            result += convertData(d, parameters);
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
        static string convertData(DataSet d, ExperimentParameters parameters)
        {
            string result = "";
            double[] scanParameterValues = d.GetAllValuesOfKey(parameters.ScanParameter);
            result += "data[\"" + parameters.ScanParameter + "\"] = {";
            foreach (double val in scanParameterValues)
            {
                result += val.ToString() + ",";
            }
            result = result.Remove(result.Length - 1); //Cheezy way of removing the last comma
            result += "};\n";

            foreach (string AIName in parameters.AINames)
            {
                double[] values = d.GetAllValuesOfKey(AIName);
                result += "data[\"" + AIName + "\"] = {";
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

        public static string CreateNotebook(string mathCommand, string fileLocation, MathKernel kernel, bool addNewLines)
        {
            if (addNewLines)
            {
                mathCommand = string.Format("{0}{1}{2}", "Module[{boxoutput,b2},boxoutput=FullForm[ToBoxes[Defer[", mathCommand, "]]];b2=boxoutput[[1,1,3,1]];boxoutput[[1,1,3,1]]=Join[Flatten[Riffle[Partition[b2,2],\"\\[IndentingNewLine]\"],1],{\"\\[IndentingNewLine]\",Last[b2]}];boxoutput]");
            }
            else
            {
                mathCommand = string.Format("{0}{1}{2}", "FullForm[ToBoxes[Defer[", mathCommand, "]]]");
            }
            fileLocation = Path.ChangeExtension(fileLocation, ".nb");

            mathCommand = ComputeMathCommand(mathCommand, kernel);
            mathCommand = string.Format("{0}{1}{2}", "Notebook[{Cell[BoxData[", mathCommand, "], \"Input\"]},WindowSize->{615, 750}, WindowMargins->{{328, Automatic}, {Automatic, 76}},StyleDefinitions->\"Default.nb\"]");

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