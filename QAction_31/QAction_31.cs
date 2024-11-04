using System;
using Skyline.DataMiner.Scripting;

//Add Validate Process
public class QAction
{
	public static void Run(SLProtocol protocol)
	{
		try
		{
			int iTriggerParam = protocol.GetTriggerParameter();
			string newProcessKey = Convert.ToString(protocol.GetParameter(iTriggerParam));
			Object[] row = new Object[3];
			row[0] = newProcessKey;
			row[1] = sharedMethods.sharedMethods.validateProcess(protocol, newProcessKey); //validateProcess(protocol, newProcessKey);
			row[2] = "Monitored";

			//ADD ROW
			if (protocol.GetKeyPosition(Parameter.Processvalidationtable.tablePid, newProcessKey) == 0)
			{
				protocol.AddRow(Parameter.Processvalidationtable.tablePid, newProcessKey);
				protocol.SetRow(Parameter.Processvalidationtable.tablePid, newProcessKey, row);
			}
			// if the row already exists, update it
			else
			{
				protocol.SetRow(Parameter.Processvalidationtable.tablePid, newProcessKey, row);
			}
		}
		catch (Exception e)
		{
			protocol.Log("QA" + protocol.QActionID + "|Add/Set Process Validation Process|Error/Message: " + e, LogType.Error, LogLevel.NoLogging);
		}
	}
}