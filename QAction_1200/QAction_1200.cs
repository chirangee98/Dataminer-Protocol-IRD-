using System;
using Skyline.DataMiner.Scripting;

public class QAction
{
	public static void Run(SLProtocol protocol, object taskTable)
	{
		try
		{
			var taskTableRowCount = protocol.RowCount(taskTable);

			if (Convert.ToString(protocol.GetParameter(Parameter.Write.clearnormalizetm_1200)) == "Normalize Alarms")
			{
				for (int i = 1; i <= taskTableRowCount; i++)
				{
					var taskRow = (object[])protocol.GetRow(Parameter.Taskmanager.tablePid, i);
					var rowToAdd = new NormalizetaskmanagerQActionRow
					{
						Normalizetaskmanagerprocessname_111 = Convert.ToString(taskRow[0]),
						Normalizetaskmanagerprocesscpu_112 = taskRow[3],
						Normalizetaskmanagerprocessmemusage_113 = taskRow[5],
					};

					protocol.AddRow(Parameter.Normalizetaskmanager.tablePid, rowToAdd);
				}
			}
			else
			{
				for (int i = taskTableRowCount; i > 0; i--)
				{
					if (Convert.ToInt32(protocol.GetParameterIndex(Parameter.Taskmanager.tablePid, i, 7)) != 4)
					{
						continue;
					}

					string strKey = Convert.ToString(protocol.GetParameterIndex(Parameter.Taskmanager.tablePid, i, 1));
					protocol.DeleteRow(Parameter.Taskmanager.tablePid, strKey);
					protocol.DeleteRow(Parameter.Hrswrunperftable.tablePid, strKey);

				}
			}
		}
		catch (Exception e)
		{
			protocol.Log("QA" + protocol.QActionID + "|Normalize Task Manager|Error/Message: " + e, LogType.Error, LogLevel.NoLogging);
		}
	}
}