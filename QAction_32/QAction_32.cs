using System;
using System.Collections;
using System.Collections.Generic;
using Skyline.DataMiner.Scripting;

// Validate Process
public class QAction
{
	public static void Run(SLProtocol protocol)
	{
		try
		{
			int[] indexes = new[]
				{
					Parameter.Processvalidationtable.Idx.processvalidationprocesstovalidate_8601,
					Parameter.Processvalidationtable.Idx.processvalidationstatus_8602,
					Parameter.Processvalidationtable.Idx.processvalidationprocessmonitoring_8603,
				};

			object[] columns = (object[])protocol.NotifyProtocol(
				321 /*NT_GET_TABLE_COLUMNS*/,
				Parameter.Processvalidationtable.tablePid,
				indexes);

			object[] keys = (object[])columns[0];
			object[] procStatus = (object[])columns[1];
			object[] monStatus = (object[])columns[2];

			List<string> rowsToDelete = new List<string>();
			List<object>[] listProcValCols = new List<object>[2];
			for (int i = 0; i < listProcValCols.Length; i++)
			{
				listProcValCols[i] = new List<object>();
			}

			for (int i = 0; i < keys.Length; i++)
			{
				string processKey = Convert.ToString(keys[i]);
				string processStatus = Convert.ToString(procStatus[i]);
				string monitoringStatus = Convert.ToString(monStatus[i]);
				if (monitoringStatus == "Monitored")
				{
					string svalidateProcess = sharedMethods.sharedMethods.validateProcess(protocol, processKey);

					SetProcessValidationRow(listProcValCols, processKey, processStatus, svalidateProcess);
				}
				else if (monitoringStatus == "Delete")
				{
					rowsToDelete.Add(processKey);
				}
				else
				{
					listProcValCols[0].Add(processKey);
					listProcValCols[1].Add("Not Monitored");
				}
			}

			UpdateProcessValidationTable(protocol, rowsToDelete, listProcValCols);
		}
		catch (Exception e)
		{
			protocol.Log("QA" + protocol.QActionID + "|Update Process Validation Table|Error/Message: " + e, LogType.Error, LogLevel.NoLogging);
		}
	}

	private static void UpdateProcessValidationTable(SLProtocol protocol, List<string> rowsToDelete, List<object>[] listProcValCols)
	{
		if (listProcValCols[0].Count > 0)
		{
			object[] columnPids = new object[]
			{
					Parameter.Processvalidationtable.tablePid,
					Parameter.Processvalidationtable.Pid.processvalidationstatus_8602,
			};

			object[] columnData = new object[]
			{
					listProcValCols[0].ToArray(),
					listProcValCols[1].ToArray(),
			};

			protocol.NotifyProtocol(220, columnPids, columnData);
		}

		protocol.DeleteRow(Parameter.Processvalidationtable.tablePid, rowsToDelete.ToArray());
	}

	private static void SetProcessValidationRow(List<object>[] listProcValCols, string processKey, string processStatus, string svalidateProcess)
	{
		if (processStatus != svalidateProcess)
		{
			listProcValCols[0].Add(processKey);
			listProcValCols[1].Add(svalidateProcess);
		}
	}
}