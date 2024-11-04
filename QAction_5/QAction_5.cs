using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skyline.DataMiner.Scripting;
using static System.Net.WebRequestMethods;
using static Skyline.DataMiner.Scripting.Parameter;

/// <summary>
/// DataMiner QAction Class.
/// </summary>
public static class QAction
{
    /// <summary>
    /// The QAction entry point.
    /// </summary>
    /// <param name="protocol">Link with SLProtocol process.</param>
    public static async void Run(SLProtocol protocol)
    {
        try
        {
            string ips = Convert.ToString(protocol.GetParameter(40000));

            // Check if the string contains multiple IPs, otherwise, use the single IP
            string ipAddress = ips.Contains(";") ? ips.Split(';')[0] : ips;

            // string ipAddress = "10.0.10.9";

            // Get the JSON string from the parameter (assuming parameter 19 holds the JSON data)
            string jsonString = protocol.GetParameter(19).ToString();

            // Parse the JSON string
            JObject jsonObj = JObject.Parse(jsonString);

            // Extract the first node name
            string firstNodeName = jsonObj["jobs"][0]["node_name"].ToString();

            // string firstNodeName = "sr109";

            // Log the first node name or use it as needed
            protocol.Log($"First Node Name: {firstNodeName}  and ip address is: {ipAddress}");

            // API URL to fetch data
            string apiUrl = $"http://{ipAddress}/node/rf_tuning_nms/{firstNodeName}"; // Replace with your actual API URL

            // Fetch data from the API
            string jsonResponse = Convert.ToString(await GetApiResponse(apiUrl));

            if (string.IsNullOrEmpty(jsonResponse))
            {
                protocol.Log($"QA{protocol.QActionID}|Failed to fetch data from API", LogType.Error, LogLevel.NoLogging);
                return;
            }

            // Deserialize the JSON response to the Root object
            Root rootObject = JsonConvert.DeserializeObject<Root>(jsonResponse);

            if (rootObject?.Rf_tunning_list == null || rootObject.Rf_tunning_list.Count == 0)
            {
                protocol.Log($"QA{protocol.QActionID}|No data available in the API response", LogLevel.NoLogging);
                return;
            }

            // Prepare the data for insertion into the DataMiner table
            List<object[]> tableRows = new List<object[]>();
            int serialNumber = 1;

            // Initialize a new list to hold program data for the current RF tuning entry
            List<object[]> programTableRows = new List<object[]>();
            int programIndex = 1;  // Index to differentiate programs within the same RF tuning
            foreach (var rftunning in rootObject.Rf_tunning_list)
            {
                tableRows.Add(new object[]
                {
                    serialNumber++,
                    rftunning.Pf_rf_frequency,
                    rftunning.Pf_rf_symbol_rate,
                    rftunning.Pf_rf_fec,
                    rftunning.Pf_rf_modulation_type,
                    rftunning.Pf_rf_adapter_port,
                    rftunning.Pf_rf_lnb_voltage,
                    rftunning.Pf_rf_lnb_tone,
                    rftunning.Pf_rf_ip_address,
                    rftunning.Pf_rf_Signal,
                    rftunning.Pf_rf_status,
                });
                SetChannalInfo(protocol, rftunning.Pf_rf_channel_info, programTableRows, ref programIndex, rftunning);
            }

            protocol.FillArray(250, tableRows, NotifyProtocol.SaveOption.Full);
        }
        catch (Exception ex)
        {
            protocol.Log($"QA{protocol.QActionID}|Exception thrown: {ex.Message}", LogType.Error, LogLevel.NoLogging);
        }
    }

    // Method to fetch data from the API
    public static async Task<string> GetApiResponse(string url)
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode(); // Throw if not 200-299
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException e)
            {
                // Log HTTP request errors
                Console.WriteLine($"Request error: {e.Message}");
                return null;
            }
        }
    }

    private static void SetChannalInfo(SLProtocol protocol, string channelInfo, List<object[]> programTableRows, ref int programIndex, RfTunningList rftunning)
    {
        // Parse JSON
        JObject json;
        try
        {
            protocol.Log($"channal info for each list {channelInfo}");
            json = JObject.Parse(channelInfo);

            // protocol.Log($"{rftunning.Pf_rf_config_id} json object: {json.ToString()}");
        }
        catch (JsonReaderException e)
        {
            protocol.Log($"Error parsing Pf_rf_channel_info JSON: {e.Message}");
            return; // Skip this entry and proceed
        }

        // Extract values
        string frequency = rftunning.Pf_rf_frequency ?? "Unknown";

        // Check if "program_info" exists and is not null
        if (json["program_info"] != null && json["program_info"].HasValues)
        {
            // Iterate over the "program_info" array
            protocol.Log("Processing program_info...");
            foreach (var program in json["program_info"])
            {
                string programName = program["program_name"]?.ToString() ?? "Unknown";
                string programNumber = program["program_number"]?.ToString() ?? "Unknown";
                string videoPid = string.Empty;
                string videoCodec = string.Empty;
                string audioPid = string.Empty;
                string audioCodec = string.Empty;

                if (program["tracks"] != null)
                {
                    // Extract track details
                    foreach (var track in program["tracks"])
                    {
                        string trackType = track["track_type"]?.ToString() ?? "Unknown";
                        if (trackType == "VIDEO")
                        {
                            videoPid = track["track_pid"]?.ToString() ?? "Unknown";
                            videoCodec = track["track_codec"]?.ToString() ?? "Unknown";
                        }
                        else if (trackType == "AUDIO")
                        {
                            audioPid = track["track_pid"]?.ToString() ?? "Unknown";
                            audioCodec = track["track_codec"]?.ToString() ?? "Unknown";
                        }
                    }
                }

                // Add the extracted program data into the program table (e.g., table with PID 251)
                programTableRows.Add(new object[]
                {
                programIndex++, // Program index within the RF tuning
                frequency,
                programName,    // Program name
                programNumber,  // Program number
                videoPid,       // Video PID
                videoCodec,     // Video codec
                audioPid,       // Audio PID
                audioCodec,    // Audio codec
                });
            }
        }

        // Insert the program data for the current RF tuning entry into the program table (PID 280)
        protocol.FillArray(280, programTableRows, NotifyProtocol.SaveOption.Full);
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Root
    {
        public string Node_name { get; set; }

        public List<RfTunningList> Rf_tunning_list { get; set; }
    }

    public class RfTunningList
    {
        public string Pf_rf_config_id { get; set; }

        public string Pf_rf_ip_address { get; set; }

        public string Pf_rf_frequency { get; set; }

        public string Pf_rf_symbol_rate { get; set; }

        public string Pf_rf_adapter_port { get; set; }

        public string Pf_rf_lnb_voltage { get; set; }

        public string Pf_rf_fec { get; set; }

        public string Pf_rf_lnb_tone { get; set; }

        public string Pf_rf_status { get; set; }

        public string Pf_rf_Signal { get; set; }

        public string Pf_rf_modulation_type { get; set; }

        public string Pf_rf_channel_info { get; set; }
    }
}