using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Skyline.DataMiner.Scripting;

/// <summary>
/// DataMiner QAction Class: QActionName.
/// </summary>
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
            string ips = Convert.ToString(protocol.GetParameter(40000));

            // Check if the string contains multiple IPs, otherwise, use the single IP
            string ipAddress = ips.Contains(";") ? ips.Split(';')[0] : ips;

            // Retrieve the row key and row data from the table
            string rowKey = protocol.RowKey(); // e.g., "job_23"
            object selected = protocol.GetRow(230, rowKey); // Fetch the row data
            object[] rowArray = (object[])selected;
            string rowValues = string.Join(", ", rowArray); // Log the row data for reference
            protocol.Log($"Dropdown clicked rowKey: {rowKey}, row object: {rowValues}");

            // Extract values from the row (assumed column indexes based on row structure)
            string jobName = rowArray[1]?.ToString(); // job name (e.g., "job_23")
            string selectedInput = rowArray[3]?.ToString(); // active input (e.g., "file_1
            string inputGroupNo=rowArray[2]?.ToString();
            string groupNo = inputGroupNo.Split(' ')[2];

            // Log extracted job name and active input
            protocol.Log($"Extracted Job Name: {jobName}, Selected Input: {selectedInput} , and groupNo {groupNo}");

            // Get the JSON data from the parameter (assumed parameter ID 19)
            string source = Convert.ToString(protocol.GetParameter(19));

            // Parse the JSON data using JObject
            JObject json = JObject.Parse(source);

            // Correct JSONPath query that takes into account the nested structure of input_details
            var switcherCount = json.SelectToken($"$.jobs[?(@.job_name == '{jobName}')].input_details..[?(@.input_name == '{selectedInput}')].switcher_count" );

            // Check if the pfmt_input_stream_id was found
            if (switcherCount != null)
            {
                protocol.Log($"Found pfmt_input_stream_id for Job '{jobName}' and  is:{switcherCount} and group no is {groupNo}");
                string apiUrl = $"http://{ipAddress}/jobs/external_switcher";
                protocol.Log($"API URL: {apiUrl}");
                SendPostRequest(protocol, apiUrl, jobName, switcherCount.ToString(),groupNo).Wait();
            }
            else
            {
                protocol.Log($"No matching pfmt_input_stream_id found for Job '{jobName}' and Input '{selectedInput}'");
            }
        }
        catch (Exception ex)
        {
            // Log any exception that occurs
            protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|Run|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
        }
    }

    private static async Task SendPostRequest(SLProtocol protocol, string url, string jobName, string switcherCount, string groupNo)
    {
        using (HttpClient client = new HttpClient())
        {
            protocol.Log($"url:{url} jobName: {jobName} inputNumber: {switcherCount} groupNumber: {groupNo}");
            var postData = new Dictionary<string, string>
            {
                { "job_name", jobName },
                { "stream_number", switcherCount },
                {"input_number", groupNo},
            };

            // Create the HTTP content with form URL-encoded data
            HttpContent content = new FormUrlEncodedContent(postData);

            try
            {
                // Send the POST request
                HttpResponseMessage response = await client.PostAsync(url, content);

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    protocol.Log($"QA{protocol.QActionID}|POST request successful. Response: {result}");
                }
                else
                {
                    protocol.Log($"QA{protocol.QActionID}|POST request failed. Status Code: {response.StatusCode}, Reason: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                protocol.Log($"QA{protocol.QActionID}|Exception occurred during POST request: {ex.Message}", LogType.Error, LogLevel.NoLogging);
            }
        }
    }
}
