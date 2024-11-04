
using System;
using Skyline.DataMiner.Scripting;
using System.Collections;
using System.Collections.Generic;
using sharedMethods;

public class QAction
{
	public static void Run(SLProtocol protocol)
	{
        //clear table if polling is set to disabled
        //int pollingState = Convert.ToInt16(protocol.GetParameter(1080));

        
        try
        {
            sharedMethods.sharedMethods.DeleteAllRows(protocol, 1050);
        }
        catch (Exception e)
        {
            protocol.Log("QA" + protocol.QActionID + "|Delete Linux Monitored Disks Table|Error/Message: " + e, LogType.Error, LogLevel.NoLogging);
        }
    }

}

