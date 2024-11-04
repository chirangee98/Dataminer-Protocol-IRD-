
//
//	
//							
//	
//
using System;
using System.IO;
using Skyline.DataMiner.Scripting;

public class QAction
{
	public static void Run(SLProtocol protocol)
	{
		try
		{
			// Get active param
			int myActParam = (int)protocol.GetTriggerParameter();

			// Get value
			int myActValue = Convert.ToInt32(protocol.GetParameter(myActParam));

			switch (myActParam)
			{
				case 40032:
					//protocol.Log(1,1,"40032-CONFIGURING PERIOD TO " + Convert.ToString(myActValue));
					if (myActValue >= 1)
						// Copy read to write
						protocol.SetParameter(myActParam - 1, myActValue);
					break;
				default:
					//protocol.Log(1,1,"40032-CONFIGURING EXECUTION TO " + Convert.ToString(myActValue));
					// Copy read to write
					protocol.SetParameter(myActParam - 1, myActValue);
					break;

			}
		}
		catch (Exception e)
		{
			protocol.Log("QA" + protocol.QActionID + "|Update Port Monitoring|Error/Message: " + e, LogType.Error, LogLevel.NoLogging);
		}
	}
}
