
using System;
using System.IO;
using System.Text;
using Skyline.DataMiner.Scripting;
using System.Globalization;
public class QAction
{
    public static void Run(SLProtocol protocol)
    {
        ////protocol.Log(8,5,"QA122 - Start");
        try
        {
            int iPID = protocol.GetTriggerParameter();
            protocol.SetParameterIndexByKey(110, protocol.RowKey(), iPID - 120, protocol.GetParameter(iPID));
            protocol.CheckTrigger(iPID - 10);
        }
        catch (Exception e)
        {
            protocol.Log("QA" + protocol.QActionID + "|Normalize Process|Error/Message: " + e, LogType.Error, LogLevel.NoLogging);
        }
        ////protocol.Log(8,5,"QA122 - Stop");
    }
}
