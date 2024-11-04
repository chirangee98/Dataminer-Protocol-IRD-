using System;
using System.Collections.Generic;
using Skyline.DataMiner.Scripting;

public class QAction
{
	public static void Run(SLProtocol protocol, Object taskTable)
	{
		try
		{
			List<string> list = new List<string>();
			List<string> listPIDs = new List<string>();

			if (taskTable == null)
			{
				throw new ArgumentException("The argument provided is null.");
			}

			object[] columns = (object[])taskTable;
			if (columns.Length < 7 || columns[0] == null)
			{
				protocol.Log("QA" + protocol.QActionID + "|Task Manager table is empty.", LogType.Error, LogLevel.NoLogging);
				return;
			}

			object[] keys = (object[])columns[0];
			int lAction = Convert.ToInt32(protocol.GetParameter(Parameter.taskmanagerautoclear_30));

			if (lAction == 0)
			{
				protocol.Log("QA" + protocol.QActionID + "|The Task manager auto clear option is disabled.", LogType.Error, LogLevel.NoLogging);
				return;
			}

			object[] processId = (object[])columns[1];
			object[] state = (object[])columns[6];

			for (int i = 0; i < keys.Length; i++)
			{
				if (state[i] == null || keys[i] == null)
				{
					continue;
				}

				int lState = Convert.ToInt32(state[i]);
				if (lState == 4)
				{
					list.Add(Convert.ToString(keys[i]));
					listPIDs.Add(Convert.ToString(processId[i]));
				}
			}

			if (list.Count != 0)
			{
				NotifyProtocol.DeleteRow(protocol, 80, list.ToArray());
			}

			if (listPIDs.Count != 0)
			{
				NotifyProtocol.DeleteRow(protocol, 90, listPIDs.ToArray());
			}
		}
		catch (Exception e)
		{
			protocol.Log("QA" + protocol.QActionID + "|Clean Task Manager|Error/Message: " + e, LogType.Error, LogLevel.NoLogging);
		}
	}
}