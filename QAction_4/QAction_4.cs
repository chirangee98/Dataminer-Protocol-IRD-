using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skyline.DataMiner.Net.Jobs;
using Skyline.DataMiner.Net.NetworkDiscovery;
using Skyline.DataMiner.Net.SLDataGateway.Types;
using Skyline.DataMiner.Scripting;

public static class Parameter
{
    public static class Jobs
    {
        public const int TablePid = 210; // Table ID for the Jobs
    }
}

public static class QAction
{
    /// <summary>
    /// The QAction entry point.
    /// </summary>
    /// <param name="protocol">Link with SLProtocol process.</param>
    public static void Run(SLProtocol protocol)
    {
        try
        {
            // Get the JSON data as a string from a parameter (e.g., parameter ID 19)
            string source = Convert.ToString(protocol.GetParameter(19));

            // Call the SetCpuTemperature function with the JSON source and protocol instance for set cpu temperature
            SetCpuTemperature(source, protocol);
            SetJobStatistic(source, protocol);
            SetPixfixVersion(source, protocol);

            // Deserialize the JSON content into C# objects
            Rootobject rootObjects = JsonConvert.DeserializeObject<Rootobject>(source);
            List<object[]> jobs = new List<object[]>();

            // protocol.Log($"cpu details {rootObjects.Cpu_Details.ToString()}");

            // Iterate over each job in the JSON and map it to the table structure
            foreach (Job job in rootObjects.Jobs)
            {
                // Generate button action based on job status
                string buttonAction = GetButtonForJobStatus(job.Job_Status);

                // Create the table row directly as an object[] with required fields (Job Name, Job Type, etc.)
                jobs.Add(new object[]
                {
                    job.Job_Name,          // Job Name
                    job.Job_Type,          // Job Type
                    job.Job_Status,        // Job Status
                    job.Updated_Time,      // Updated Time
                    job.Node_Name,         // Node Name
                    buttonAction,           // Button Action (Start/Stop based on job status)
                });
            }

            // Insert the data into the DataMiner table (with table PID 210)
            protocol.FillArray(Parameter.Jobs.TablePid, jobs, NotifyProtocol.SaveOption.Full);
        }
        catch (Exception ex)
        {
            protocol.Log("QA" + protocol.QActionID + "|Deserializing JSON text failed: " + ex.Message, LogType.Error, LogLevel.NoLogging);
        }
    }

    private static void SetCpuTemperature(string source, SLProtocol protocol)
    {
        try
        {
            // Log the source data for debugging
            // protocol.Log($"QA{protocol.QActionID}|Source JSON data: {source}");

            // Verify if the source data is in a valid JSON format
            if (string.IsNullOrWhiteSpace(source) || !source.TrimStart().StartsWith("{"))
            {
                throw new Exception("Source data is not in valid JSON format.");
            }

            // Parse the JSON
            JObject json = JObject.Parse(source);

            // Find the dynamic device name
            var device = json["cpu_details"]?.First as JProperty;

            if (device != null)
            {
                string dynamicDeviceName = device.Name;

                // Retrieve the sensor details as a string and clean it up
                var cpuDetailsString = device.Value["system_sensors_details_json"]?.ToString();
                if (cpuDetailsString != null && cpuDetailsString.StartsWith("{"))
                {
                    JObject sensorDetails = JObject.Parse(cpuDetailsString);

                    string cpuTemp0 = sensorDetails["cpu_package_temp_0"]?["current_value"]?.ToString();
                    string cpuTemp1 = sensorDetails["cpu_package_temp_1"]?["current_value"]?.ToString();

                    protocol.SetParameter(22, cpuTemp0 ?? "N/A");
                    protocol.SetParameter(23, cpuTemp1 ?? "N/A");

                    protocol.Log($"QA{protocol.QActionID}|Successfully set CPU temperatures: Temp0 = {cpuTemp0}, Temp1 = {cpuTemp1}");
                }
                else
                {
                    protocol.Log($"QA{protocol.QActionID}|Error: 'system_sensors_details_json' not found or improperly formatted.", LogType.Error, LogLevel.NoLogging);
                }
            }
            else
            {
                protocol.Log($"QA{protocol.QActionID}|Error: 'cpu_details' not found or empty in the JSON data.", LogType.Error, LogLevel.NoLogging);
            }
        }
        catch (JsonReaderException jsonEx)
        {
            protocol.Log($"QA{protocol.QActionID}|JSON parsing error in SetCpuTemperature method: {jsonEx.Message}", LogType.Error, LogLevel.NoLogging);
        }
        catch (Exception ex)
        {
            protocol.Log($"QA{protocol.QActionID}|Error in SetCpuTemperature method: {ex.Message}", LogType.Error, LogLevel.NoLogging);
        }
    }

    private static void SetPixfixVersion(string source, SLProtocol protocol)
    {
        try
        {
            // Log the source data for debugging
            // protocol.Log($"QA{protocol.QActionID}|Source JSON data for Pixfix version: {source}");

            // Verify if the source data is in a valid JSON format
            if (string.IsNullOrWhiteSpace(source) || !source.TrimStart().StartsWith("{"))
            {
                throw new Exception("Source data is not in valid JSON format.");
            }

            // Parse the JSON
            JObject json = JObject.Parse(source);

            // Extract the pfs_pf_version value if it exists
            string pixfixVersion = json["pfs_pf_version"]?.ToString();

            // If the version exists, set it to parameter 24
            if (!string.IsNullOrEmpty(pixfixVersion))
            {
                protocol.SetParameter(24, pixfixVersion);
                protocol.Log($"QA{protocol.QActionID}|Successfully set Pixfix version: {pixfixVersion}");
            }
            else
            {
                protocol.Log($"QA{protocol.QActionID}|Error: 'pfs_pf_version' not found in JSON data.", LogType.Error, LogLevel.NoLogging);
            }
        }
        catch (JsonReaderException jsonEx)
        {
            protocol.Log($"QA{protocol.QActionID}|JSON parsing error in SetPixfixVersion method: {jsonEx.Message}", LogType.Error, LogLevel.NoLogging);
        }
        catch (Exception ex)
        {
            protocol.Log($"QA{protocol.QActionID}|Error in SetPixfixVersion method: {ex.Message}", LogType.Error, LogLevel.NoLogging);
        }
    }

    private static void SetJobStatistic(string source, SLProtocol protocol)
    {
        try
        {
            // Verify if the source data is in a valid JSON format
            if (string.IsNullOrWhiteSpace(source) || !source.TrimStart().StartsWith("{"))
            {
                throw new Exception("Source data is not in valid JSON format.");
            }

            // Parse the JSON
            JObject json = JObject.Parse(source);

            // Check if the 'pfmt_job_stat_data' exists in JSON
            var jobStatData = json["pfmt_job_stat_data"] as JObject;
            var jobsTableData = new List<object[]>();

            if (jobStatData != null)
            {
                // Iterate over each job in 'pfmt_job_stat_data'
                foreach (var jobEntry in jobStatData)
                {
                    string jobName = jobEntry.Key;
                    JObject statData = jobEntry.Value as JObject;

                    // Extract individual fields
                    string jobStatus = statData["job_status"]?.ToString() ?? "N/A";
                    string latency = statData["latency"]?.ToString() ?? "N/A";
                    string bufferedData = statData["buffered_data"]?.ToString() ?? "N/A";
                    string cpuLoad = statData["cpu_load"]?.ToString() ?? "N/A";
                    string cpuLoadAvg = statData["cpu_load_avg"]?.ToString() ?? "N/A";
                    string memoryLoad = statData["memory_load"]?.ToString() ?? "N/A";
                    string memoryLoadAvg = statData["memory_load_avg"]?.ToString() ?? "N/A";

                    // Add job data to the table data list
                    jobsTableData.Add(new object[]
                    {
                    jobName,            // Job Name
                    jobStatus,          // Job Status
                    latency,            // Latency
                    bufferedData,       // Buffered Data
                    cpuLoad,            // CPU Load
                    cpuLoadAvg,         // CPU Load Average
                    memoryLoad,         // Memory Load
                    memoryLoadAvg,      // Memory Load Average
                    });
                }

                // Insert the data into the DataMiner table (table PID 290)
                protocol.FillArray(290, jobsTableData, NotifyProtocol.SaveOption.Full);

                protocol.Log($"QA{protocol.QActionID}|Successfully set job statistics for {jobsTableData.Count} jobs.");
            }
            else
            {
                protocol.Log($"QA{protocol.QActionID}|Error: 'pfmt_job_stat_data' not found in JSON data.", LogType.Error, LogLevel.NoLogging);
            }
        }
        catch (JsonReaderException jsonEx)
        {
            protocol.Log($"QA{protocol.QActionID}|JSON parsing error in SetJobStatistic method: {jsonEx.Message}", LogType.Error, LogLevel.NoLogging);
        }
        catch (Exception ex)
        {
            protocol.Log($"QA{protocol.QActionID}|Error in SetJobStatistic method: {ex.Message}", LogType.Error, LogLevel.NoLogging);
        }
    }

    private static string GetButtonForJobStatus(string jobStatus)
    {
        return jobStatus.ToLower() == "running" ? "Stop" : "Start";
    }
}

// Rootobject representing the JSON structure
public class Rootobject
{
    public Job[] Jobs { get; set; }
}

// Job class representing each job's structure in the JSON
public class Job
{
    public string Job_Name { get; set; }// Job Name

    public string Job_Type { get; set; }// Job Type

    public string Job_Status { get; set; }// Job Status

    public string Updated_Time { get; set; }// Updated Time

    public string Node_Name { get; set; }// Node Name

    public string Active_Id { get; set; }// Active ID
}