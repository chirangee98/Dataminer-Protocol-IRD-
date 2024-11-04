using System;
using Skyline.DataMiner.Scripting;

public class QAction
{
	public static void Run(SLProtocol protocol)
	{
		//protocol.Log(1,1,"ADD PORT");
		string sRowKey = Convert.ToString(protocol.GetParameter(Parameter.Write.addport_40043));
		string ElementID = Convert.ToString(protocol.GetParameter(2));
		string UniqueID = ElementID + "-" + sRowKey;

		try
		{
			lock (multiThreadMethods.multiThreadMethods.provideKeyLock(UniqueID))
			{
				if (!String.IsNullOrEmpty(sRowKey) && protocol.GetKeyPosition(Parameter.Portlist.tablePid, sRowKey) == 0)
				{
					protocol.AddRow(Parameter.Portlist.tablePid, new object[] { sRowKey, 2000, 1, });
				}
			}
		}
		catch (Exception e)
		{
			protocol.Log("QA" + protocol.QActionID + "|Add Port in Port List|Error/Message: " + e, LogType.Error, LogLevel.NoLogging);
		}
	}
}