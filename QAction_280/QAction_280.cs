using System;
using System.Collections.Generic;
using System.Linq;
using Skyline.DataMiner.Scripting;

public class QAction
{
	/// <summary>
	/// Run
	/// </summary>
	/// <param name="protocol">Link with Skyline Dataminer</param>
	public void Run(SLProtocolExt protocol)
	{
		string Value = Convert.ToString(protocol.Write.Csvbutton);
		switch (Value)
		{
			case "Import CSV":
				ImportCSV(protocol);
				break;
			case "Export CSV":
				ExportCSV(protocol);
				break;
			default:
				protocol.Log("QA" + protocol.QActionID + "|Invalid Operation.", LogType.Error, LogLevel.NoLogging);
				break;
		}
	}

	/// <summary>
	/// ExportCSV
	/// </summary>
	/// <param name="protocol">Link with Skyline Dataminer</param>
	private void ExportCSV(SLProtocolExt protocol)
	{
		try
		{
			UInt32[] paramIds = new UInt32[] { Parameter.pathofthefiles_2011, Parameter.csvfilename_2013 };
			object[] ParamValues = (object[])protocol.GetParameters(paramIds);
			string Path = Convert.ToString(ParamValues[0]);
			string FileName = Convert.ToString(ParamValues[1]);

			if (!string.IsNullOrWhiteSpace(Path) || !string.IsNullOrWhiteSpace(FileName))    //check file path and name are filled in
			{
				List<string> lHeaderIns = new List<string> { "Task Manager Process Name Display Key", "Process Run Path", "Process Run Argument" };

				object[] oaProcessCounterColumns = (object[])protocol.NotifyProtocol(321, Parameter.Processcounter.tablePid, new UInt32[] { 3, 2, 7 });
				object[] oaProcessNames = (object[])oaProcessCounterColumns[0];
				object[] oaProcessRunPaths = (object[])oaProcessCounterColumns[1];
				object[] oaProcessRunArguments = (object[])oaProcessCounterColumns[2];

				string[] rows = new string[oaProcessNames.Length];

				for (int i = 0; i < oaProcessNames.Length; i++)
				{
					rows[i] = Convert.ToString(oaProcessNames[i]) + ";" + Convert.ToString(oaProcessRunPaths[i]) + ";" +
							  Convert.ToString(oaProcessRunArguments[i]);
				}
				sharedMethods.sharedMethods.ExportCSV(Path, FileName, lHeaderIns, rows);
			}
			else
			{
				protocol.ShowInformationMessage("Please make sure to fill in the Path of the Files and the File Name of the CSV before exporting it.");
			}
		}
		catch (Exception ex)
		{
			protocol.Log("QA" + protocol.QActionID + "|ExportMethod|Exception: " + ex, LogType.Error, LogLevel.NoLogging);
		}
	}

	/// <summary>
	/// ImportCSV
	/// </summary>
	/// <param name="protocol">Link with Skyline Dataminer</param>
	private void ImportCSV(SLProtocolExt protocol)
	{
		try
		{
			UInt32[] paramIds = new UInt32[] { Parameter.pathofthefiles_2011, Parameter.csvfilename_2013 };
			object[] ParamValues = (object[])protocol.GetParameters(paramIds);
			string Path = Convert.ToString(ParamValues[0]);
			string FileName = Convert.ToString(ParamValues[1]);

			if (!string.IsNullOrWhiteSpace(Path) || !string.IsNullOrWhiteSpace(FileName))
			{
				int AmountOfCol = 3;

				Dictionary<int, int> HeaderPos_TableColPos = new Dictionary<int, int>();
				List<object[]> TableRows = new List<object[]>();

				int iRow = 0;
				foreach (var line in CSVImport.GetFileData(Path, FileName))
				{
					object[] Row = null;
					Row = new object[AmountOfCol];

					string[] Cells = CSVImport.SplitCSVRow(line);

					for (int iCell = 0; iCell < Cells.Length; iCell++)
					{
						string CellValue = Cells[iCell];
						if (iRow == 0)
						{
							HeaderPos_TableColPos[iCell] = iCell;
						}
						else
						{
							if (HeaderPos_TableColPos.ContainsKey(iCell))
							{
								int colPos = HeaderPos_TableColPos[iCell];
								if (colPos < AmountOfCol)//limit the amount of columns
								{
									Row[colPos] = CellValue;
								}
							}
						}
					}

					if (Row != null && Row[0] != null)
					{
						TableRows.Add(Row);
					}
					iRow++;
				}

				object[] processName = new object[TableRows.Count()];
				object[] processRunPath = new object[TableRows.Count()];
				object[] processRunArgument = new object[TableRows.Count()];

				for (int i = 0; i < TableRows.Count; i++)
				{
					object[] row = TableRows[i].ToArray();
					processName[i] = row[0];
					processRunPath[i] = row[1];
					processRunArgument[i] = row[2];
				}

				object[] oaProcessCounterColumns = (object[])protocol.NotifyProtocol(321, Parameter.Processcounter.tablePid, new UInt32[] { 3, 2, 7 });

				object[] oaProcessNames = (object[])oaProcessCounterColumns[0];
				object[] oaProcessRunPaths = (object[])oaProcessCounterColumns[1];
				object[] oaProcessRunArguments = (object[])oaProcessCounterColumns[2];

				for (int i = 0; i < processRunPath.Length; i++)
				{
					bool doubleDetected = false;

					for (int j = 0; !doubleDetected && j < oaProcessNames.Length; j++)
					{
						if (Convert.ToString(oaProcessNames[j]).Equals(Convert.ToString(processName[i])) &&
							Convert.ToString(oaProcessRunPaths[j]).Equals(Convert.ToString(processRunPath[i])) &&
							Convert.ToString(oaProcessRunArguments[j]).Equals(Convert.ToString(processRunArgument[i])))
						{
							doubleDetected = true;
						}
					}

					AddCouterRow(protocol, processName, processRunPath, processRunArgument, i, doubleDetected);
				}

				protocol.CheckTrigger(80);
			}
			else
			{
				protocol.ShowInformationMessage("Please make sure to fill in the Path of the Files and the File Name of the CSV before importing it.");
			}
		}
		catch (Exception ex)
		{
			protocol.Log("QA" + protocol.QActionID + "|ImportCSV|Exception: " + ex, LogType.Error, LogLevel.NoLogging);
		}
	}

	private static void AddCouterRow(SLProtocolExt protocol, object[] processName, object[] processRunPath, object[] processRunArgument, int i, bool doubleDetected)
	{
		if (!doubleDetected)
		{
			var tableRow = new ProcesscounterQActionRow
			{
				Processcounterdisplaykey_69 = processName[i],
				Processcounterrunpath_73 = processRunPath[i],
				Processcounterrunarguments_63 = processRunArgument[i],
			};

			protocol.processcounter.AddRow(tableRow);
		}
	}
}