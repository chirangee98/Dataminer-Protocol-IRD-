using System;
using System.Collections;
using Skyline.DataMiner.Scripting;
using System.Text;

//Validate Process
public class QAction
{
	public static void Run(SLProtocol protocol)
	{
		try
		{
			//string discreets = "";
			ArrayList sortedList = new ArrayList();
			StringBuilder discreets = new StringBuilder();

			Object[] result = (Object[])protocol.NotifyProtocol(168, 80, null);
			int size = result.Length;

			if (size > 0 && null != result.GetValue(0))
			{
				sortedList = new ArrayList((Object[])result.GetValue(0));
			}

			sortedList.Sort();
			foreach (string s in sortedList)
			{
				discreets.Append(s + ";");
				//discreets += s + ";";
			}

			protocol.SetParameter(Parameter.processvalidationlistbuffer_8402, discreets.ToString());
		}
		catch (Exception e)
		{
			protocol.Log("QA" + protocol.QActionID + "|Fill in Process to Validate|Error/Message: " + e, LogType.Error, LogLevel.NoLogging);
		}
	}
}