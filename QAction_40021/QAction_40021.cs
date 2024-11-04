using System;
using Skyline.DataMiner.Scripting;

public class QAction
{
	public static void Run(SLProtocol protocol)
	{

		int iTriggerParam = protocol.GetTriggerParameter();
		string sRowKey = protocol.RowKey();

		if (!string.IsNullOrEmpty(sRowKey))
		{
			switch (iTriggerParam)
			{
				case 40021:
					{
						multiThreadMethods.multiThreadMethods.setRowValue(protocol, sRowKey, 2, Convert.ToInt32(protocol.GetParameter(Parameter.Write.portlisttimeoutport_40021)));
						break;
					}
				default:
					{
						multiThreadMethods.multiThreadMethods.setRowValue(protocol, sRowKey, 9, Convert.ToInt32(protocol.GetParameter(Parameter.Write.portlistretries_40028)));
						break;
					}
			}
		}
	}
}