using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Windows.Forms;


namespace SharedCode
{
    
    public class MySQLHelper
    {
        MySqlConnection connection;
        MySqlCommand command;

        public void SendData(DataSet data, ExperimentParameters parameters)
        {
            string connectionString = "server=127.0.0.1;uid=mmtf;pwd=lplsql2016.;database=spectra;";
            try
            {
                connection = new MySqlConnection(connectionString);
                connection.Open();
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.Message);
            }

            int nextScanID = getNextScanID();

            string commandString = getAddScanParametersString(parameters.ScanParams, nextScanID);
            commandString += getAddDDSParametersString(parameters.DDS, nextScanID);
            commandString += getAddExternalParametersString(parameters.ExternalParams, nextScanID);
            commandString += getAddDAQmxParametersString(parameters.DAQmx, nextScanID);


            commandString += "INSERT INTO input_config (scanid, input_name, input_address) VALUES";
            for (int i = 0; i < parameters.DAQmx.AINames.Length; i++)
            {
                commandString += "(" + nextScanID + ",\"" + parameters.DAQmx.AINames[i] + "\",\"" + parameters.DAQmx.AIAddresses[i] + "\"),";
            }
            commandString = commandString.Remove(commandString.Length - 1) + ";" ;

            //If Data was acquired, send that too.
            
            if (parameters.ScanParams.AcquireDataDuringScan)
            {
                int nextPointID = getNextPointID();

                commandString += "INSERT INTO amplitudes (point_id, input_name, amplitude) VALUES ";
                for (int i = 0; i < data.Points.Count; i++)
                {
                    for (int j = 0; j < parameters.DAQmx.AINames.Length; j++)
                    {
                        commandString += "("+ (nextPointID + i) +",\"" + parameters.DAQmx.AINames[j] + "\"," + data.Points[i].GetValueOfKey(parameters.DAQmx.AINames[j]) + "),";
                    }
                }
                commandString = commandString.Remove(commandString.Length - 1) + ";";

                commandString += "INSERT INTO frequencies (point_id, frequency) VALUES ";
                for (int i = 0; i < data.Points.Count; i++)
                {
                    commandString += "("+ (nextPointID + i) + "," + data.Points[i].GetValueOfKey(parameters.ScanParams.ScanParameterName) + "),";
                }
                commandString = commandString.Remove(commandString.Length - 1) + ";";

                //Finally, attribute a point_id to each point.
                commandString += "INSERT INTO datetimes (point_id, datetime) VALUES ";
                for (int i = 0; i < data.Points.Count; i++)
                {
                    commandString += "("+ (nextPointID + i) + ",\"" + data.Points[i].GetDateTime() + "\"),";
                }
                commandString = commandString.Remove(commandString.Length - 1) + ";";

                //Finally, attribute a point_id to each point.
                commandString += "INSERT INTO scanids (point_id, scanid) VALUES ";
                for (int i = 0; i < data.Points.Count; i++)
                {
                    commandString += "(" + (nextPointID + i) + "," + nextScanID + "),";
                }
                commandString = commandString.Remove(commandString.Length - 1) + ";";
            }

            command = new MySqlCommand(commandString, connection);
            command.ExecuteNonQuery();

        }

        int getNextScanID()
        {
            command = new MySqlCommand();
            command.CommandText =
                "SELECT `AUTO_INCREMENT` FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'spectra' AND TABLE_NAME = 'scan_parameters';";
            command.Connection = connection;
            command.CommandType = System.Data.CommandType.Text;
            MySqlDataReader reader = command.ExecuteReader();
            int nextScanID = 0, i = 0;
            string resultOfRead = "";
            while (reader.Read())
            {
                resultOfRead += reader[i];
                i++;
            }
            reader.Close();
            nextScanID = int.Parse(resultOfRead);
            return nextScanID;
        }

        int getNextPointID()
        {
            command = new MySqlCommand();
            command.CommandText =
                "SELECT `AUTO_INCREMENT` FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'spectra' AND TABLE_NAME = 'datetimes';";
            command.Connection = connection;
            command.CommandType = System.Data.CommandType.Text;
            MySqlDataReader reader = command.ExecuteReader();
            int nextScanID = 0, i = 0;
            string resultOfRead = "";
            while (reader.Read())
            {
                resultOfRead += reader[i];
                i++;
            }
            reader.Close();
            nextScanID = int.Parse(resultOfRead);
            return nextScanID;
        }

        string getAddScanParametersString(ScanParameters p, int nextScanID)
        {
            return "INSERT INTO scan_parameters (scanid, scan_start, scan_stop, number_of_points, scan_hold_value," 
                + " sleep, scan_parameter_name, acquire_data_during_scan, stop_on_end_of_scan)"
                + "VALUES (" + nextScanID + "," + p.ScanStart.ToString() + "," + p.ScanStop.ToString() + "," + p.NumberOfPoints.ToString() + "," 
                + p.ScanHoldValue.ToString() + ","+ p.Sleep.ToString() + ",\""+ p.ScanParameterName.ToString() + "\","
                + p.AcquireDataDuringScan.ToString() + ","+ p.StopOnEOS.ToString() + ");";
        }
        
        string getAddDDSParametersString(DDSParameters p, int nextScanID)
        {
            return "INSERT INTO dds_parameters (scanid, dds_address)"
                + "VALUES (" + nextScanID + ",\"" + p.DDSAddress.ToString() + "\");";
        }

        string getAddExternalParametersString(ExternalParameters p, int nextScanID)
        {
            return "INSERT INTO external_parameters (scanid, comments, external_multiplication_factor)"
                + "VALUES (" + nextScanID + ",\"" + p.Comments + "\"," + p.ExternalFactor.ToString() + ");";
        }
        string getAddDAQmxParametersString(DAQmxParameters p, int nextScanID)
        {
            return "INSERT INTO daqmx_parameters (scanid, auto_start, trigger_address, sample_rate, points_per_integration_period)"
                + "VALUES (" + nextScanID + "," + p.AutoStart + ",\"" + p.TriggerAddress + "\"," + p.SampleRate.ToString() 
                + "," + p.NumberOfSamplesPerIntegrationTime.ToString() + ");";
        }
    }
}
