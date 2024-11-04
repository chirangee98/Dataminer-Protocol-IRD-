
using System;
using Skyline.DataMiner.Scripting;
using System.Collections;
using System.Collections.Generic;

public class QAction
{
	public static void Run(SLProtocol protocol)
	{
        try
        {
            
            object[] oaColumns = (object[])protocol.NotifyProtocol(321, 80, new UInt32[] { 0 });

            List<string> slKeys = new List<string>();
            for (int i = 0; i < ((object[])oaColumns[0]).Length; i++)
            {
                slKeys.Add(Convert.ToString(((object[])oaColumns[0])[i]));
            }

            protocol.DeleteRow(80, slKeys.ToArray());
        }
        catch (Exception e)
        {
            protocol.Log("QA" + protocol.QActionID + "|Delete Task Message|Error/Message: " + e, LogType.Error, LogLevel.NoLogging);
        }

    }
}
