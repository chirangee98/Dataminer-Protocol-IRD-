using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using Skyline.DataMiner.Scripting;

public class QAction
{
    /// <summary>
    /// FunctionName
    /// </summary>
    /// <param name="protocol">Link with Skyline DataMiner</param>
    public static void Run(SLProtocol protocol)
    {
        try
        {
            int iTriggerParam = protocol.GetTriggerParameter();
            int iToParam = iTriggerParam + 20;

            object dNewValue = protocol.GetParameter(iTriggerParam);

            string sInstace = (string)protocol.RowKey();

            protocol.SetParameters(new int[] { 15, iToParam }, new object[] { sInstace, dNewValue });
        }
        catch (Exception e)
        {
            protocol.Log("QA" + protocol.QActionID + "|Write Data|Error/Message: " + e, LogType.Error, LogLevel.NoLogging);
        }
    }
}
