using System;
using Skyline.DataMiner.Scripting;

/// <summary>
/// DataMiner QAction Class.
/// </summary>
public class QAction
{
	/// <summary>
	/// The QAction entry point.
	/// </summary>
	/// <param name="protocol">The link between SLScripting and SLProtocol.</param>
	public static void Run(SLProtocol protocol)
	{
		try
		{
			var column = (object[])protocol.NewRow();
			var rowKey = protocol.RowKey();
			if (column.Length < 7 || column[6] == null)
			{
				return;
			}

			var stateArray = Convert.ToInt32(((object[])column[6])[0]);
			if (stateArray.Equals(4))
			{
				var rowToSet = new object[11];
				for (int j = 1; j < 6; j++)
				{
					rowToSet[j] = -1;
				}

				rowToSet[7] = string.Empty;
				rowToSet[8] = string.Empty;
				rowToSet[9] = -1;
				rowToSet[10] = -1;

				protocol.SetRow(Parameter.Taskmanager.tablePid, rowKey, rowToSet);
			}
			else if (stateArray.Equals(3))
			{
				var processPID = Convert.ToString(protocol.GetParameterIndexByKey(Parameter.Taskmanager.tablePid, rowKey, 2));
				if (!string.IsNullOrEmpty(processPID) && !protocol.GetKeyPosition(Parameter.Hrswrunperftable.tablePid, processPID).Equals(0))
				{
					double newCPUTime = Convert.ToDouble(protocol.GetParameterIndexByKey(Parameter.Hrswrunperftable.tablePid, processPID, 2));
					var rowToUpdate = new TaskmanagerQActionRow((object[])protocol.GetRow(Parameter.Taskmanager.tablePid, rowKey))
					{
						Taskmanagerpreviousperf_89 = newCPUTime,
					};
					protocol.Log("QA"+protocol.QActionID+"|rowToUpdate: " + string.Join(";", rowToUpdate.ToObjectArray()), LogType.Error, LogLevel.NoLogging);
					protocol.SetRow(Parameter.Taskmanager.tablePid, rowKey, rowToUpdate.ToObjectArray());
				}
			}
			else
			{
				// another state.
			}
		}
		catch (Exception e)
		{
			protocol.Log("QA" + protocol.QActionID + "|Manage Task Removal|Error/Message: " + e, LogType.Error, LogLevel.NoLogging);
		}
	}
}