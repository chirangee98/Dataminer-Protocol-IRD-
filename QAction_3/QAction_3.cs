using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Skyline.DataMiner.Scripting;

public static class Parameter
{
    public static class Jobs
    {
        public const int TablePid = 230; // Table ID for the Jobs
    }
}

/// <summary>
/// DataMiner QAction Class: SwitcherQaction.
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
            // Get the JSON data as a string from a parameter (e.g., parameter ID 19)
            string source = Convert.ToString(protocol.GetParameter(19));

            // Deserialize the JSON content into C# objects
            Rootobject rootObjects = JsonConvert.DeserializeObject<Rootobject>(source);
            List<object[]> jobsTableData = new List<object[]>();

            int serialNumber = 1;

            // Iterate over each job in the JSON and map it to the table structure
            foreach (Job job in rootObjects.Jobs)
            {
                // Iterate over the input_details groups
                int groupNo = 1;
                foreach (var group in job.Input_details)
                {
                    // Get input names for the dropdown list (from input_details)
                    List<string> inputNames = new List<string>();
                    string activeInput = string.Empty;

                    // Iterate through each input in the group
                    foreach (var input in group.Value)
                    {
                        inputNames.Add(input.Value.Input_name);

                        // Check if the input is active and store the active input name
                        if (input.Value.Active_input == 1)
                        {
                            activeInput = input.Value.Input_name;
                        }
                    }

                    // Join input names into a semicolon-separated string to set in the dropdown
                    string inputDropdownValues = string.Join(";", inputNames);

                    // Prepare table row for each group in input_details
                    jobsTableData.Add(new object[]
                    {
                        serialNumber++,            // Column: S. No.
                        job.Job_Name,              // Column: Job Name
                        $"Input Group {groupNo++}",      // Column: Group No.
                        activeInput,               // Column: Active Input
                        inputDropdownValues,        // Column: Input Name (Dropdown values)
                    });
                }
            }

            // Insert the data into the DataMiner table (with table PID 210)
            protocol.FillArray(Parameter.Jobs.TablePid, jobsTableData, NotifyProtocol.SaveOption.Full);
        }
		catch (Exception ex)
		{
            protocol.Log("QA" + protocol.QActionID + "|Deserializing JSON text failed: " + ex.Message, LogType.Error, LogLevel.NoLogging);
        }
	}
}

public class Rootobject
{
    public Job[] Jobs { get; set; }
}

public class Job
{
    public string Job_Name { get; set; }

    public string Job_Type { get; set; }

    public string Job_Status { get; set; }

    public string Updated_Time { get; set; }

    public string Node_Name { get; set; }

    public string Active_Id { get; set; }

    public Dictionary<string, Dictionary<string, InputDetail>> Input_details { get; set; }
}

public class InputDetail
{
    public int Active_input { get; set; }

    public string Input_name { get; set; }

    public string Pfmt_input_stream_id { get; set; }

    public string Pfmt_input_switcher_id { get; set; }
}