
using System;
using System.Collections;
using Skyline.DataMiner.Scripting;

public class QAction
{
    public static void Run(SLProtocol protocol)
    {
        try
        {
            int iTrigger = protocol.GetTriggerParameter();
            string sKey = protocol.RowKey();
            object oValue = protocol.GetParameter(iTrigger);

            protocol.SetParameters(new int[] { 11000, (iTrigger + 10) }, new object[] { sKey, oValue });
        }
        catch (Exception e)
        {
            protocol.Log("QA" + protocol.QActionID + "|Set External Error Fix OID|Error/Message: " + e, LogType.Error, LogLevel.NoLogging);
        }
    }
}

