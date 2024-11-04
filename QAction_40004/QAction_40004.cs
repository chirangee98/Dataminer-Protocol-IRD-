using System;
using Skyline.DataMiner.Scripting;

public class QAction
{
	public static void Run(SLProtocol protocol)
	{
		int iTriggerParam = protocol.GetTriggerParameter();
		string sRowKey = Convert.ToString(protocol.GetParameter(iTriggerParam));

		if (!string.IsNullOrEmpty(sRowKey))
		{
			// Exception for Result, Delay, Previous Execution Time and Comment
			switch (iTriggerParam)
			{
				case 40004:
					multiThreadMethods.multiThreadMethods.setRowValues(protocol, sRowKey, new object[] { sRowKey, null, null, -1, -1, "-1", "-1", null, null, null });
					break;
				default:
					multiThreadMethods.multiThreadMethods.setRowValues(protocol, sRowKey, new object[] { sRowKey, null, null, -2, -2, "-2", "-2", null, null, null });
					break;
			}
		}
	}

}