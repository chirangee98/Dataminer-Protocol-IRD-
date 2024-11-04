using System;
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
	public static async void Run(SLProtocol protocol)
	{
		try
		{
            // Define the API URL (you provided)
            string apiUrl = "http://10.0.90.68/jobs/get_image_url/40";

            // Fetch the API response
            string jsonResponse = await FetchApiData(apiUrl);

            if (string.IsNullOrEmpty(jsonResponse))
            {
                protocol.Log($"QA{protocol.QActionID}|Failed to fetch data from API", LogType.Error, LogLevel.NoLogging);
                return;
            }

            // Log the API response
            // protocol.Log($"QA{protocol.QActionID}|API response: {jsonResponse}");

            // Parse the JSON response to extract the "result" field
            string imageUrl = ExtractImageUrl(jsonResponse);

            if (string.IsNullOrEmpty(imageUrl))
            {
                protocol.Log($"QA{protocol.QActionID}|No valid image URL found in the API response", LogType.Error, LogLevel.NoLogging);
                return;
            }

            // Log the extracted image URL
            protocol.Log($"QA{protocol.QActionID}|Extracted Image URL: {imageUrl}");

            // Set the image URL into parameter 16
            protocol.SetParameter(14, imageUrl);
            protocol.Log($"QA{protocol.QActionID}|Image URL set into parameter 16");
        }
		catch (Exception ex)
		{
			protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|Run|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);

			protocol.Log($"QA{protocol.QActionID}|Exception thrown by updated code: {ex.Message}", LogType.Error, LogLevel.NoLogging);
        }
	}

    // Method to fetch data from the API
	public static async Task<string> FetchApiData(string url)
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                // Send the HTTP GET request to the API
                HttpResponseMessage response = await client.GetAsync(url);

                // Ensure the request was successful (status code 200)
                response.EnsureSuccessStatusCode();

                // Read and return the response content as a string
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

    // Method to extract the image URL from the JSON response
	public static string ExtractImageUrl(string jsonResponse)
    {
        try
        {
            // Parse the JSON response
            JObject json = JObject.Parse(jsonResponse);

            // Extract the "result" field
            string imageUrl = json["result"]?.ToString();

            return imageUrl;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing JSON: {ex.Message}");
            return null;
        }
    }
}