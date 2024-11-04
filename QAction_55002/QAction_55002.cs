using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using Newtonsoft.Json;
using Skyline.DataMiner.Scripting;

/// <summary>
/// DataMiner QAction Class: Update job status on button click.
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
            string rowKey = protocol.RowKey();
            object rowobj = protocol.GetRow(210, rowKey);

            object[] rowArray = (object[])rowobj;
            protocol.Log($"Job Name: {rowArray[0]}");
            protocol.Log($"Node Name: {rowArray[4]}");
            protocol.Log($"need to update:{rowArray[5]}");
            protocol.Log($"status:{rowArray[2]}");

            // Extract Job Name and Node Name
            string jobName = Convert.ToString(rowArray[0]);
            string nodeName = Convert.ToString(rowArray[4]);
            string status=Convert.ToString(rowArray[2]);
            string ips = Convert.ToString(protocol.GetParameter(40000));

            // Check if the string contains multiple IPs, otherwise, use the single IP
            string ipAddress = ips.Contains(";") ? ips.Split(';')[0] : ips;
            protocol.Log($" Using iP address: {ipAddress}");

            // Assuming `ipAddress` is stored in a DataMiner parameter or can be retrieved somehow.
            // string ipAddress = (string)protocol.GetParameter(40000);  // Replace with actual IP or parameter retrieval if necessary.
           // String ipAddress="10.0.90.68";
            string url = string.Empty;

            // Construct the URL for the API call
            if (status == "running")
            {
                url = $"http://{ipAddress}/jobs/job_stop/{jobName}";
            }
            else
            {
                url = $"http://{ipAddress}/jobs/job_start/{jobName}?node_name={nodeName}";
            }

            // Log the constructed URL (optional for debugging)
            protocol.Log($"Constructed URL: {url}");

            // Send the POST request to the constructed URL
            SendPostRequest( url).Wait();
        }
        catch (Exception ex)
		{
			protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|Run|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
		}

		async System.Threading.Tasks.Task SendPostRequest(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                // Since the URL contains query parameters, no need for a JSON body.
                HttpResponseMessage response = await client.PostAsync(url, null);

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    protocol.Log($"QA{protocol.QActionID}| POST request successful. Status code: {response.StatusCode}");
                }
                else
                {
                    protocol.Log($"QA{protocol.QActionID}| POST request failed. Status code: {response.StatusCode}");
                }

                // Optionally log response content if any
                string responseContent = await response.Content.ReadAsStringAsync();
                protocol.Log($"QA{protocol.QActionID}| Response content: {responseContent}");
            }
        }
    }
}