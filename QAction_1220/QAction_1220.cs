
using System;
using System.IO;
using System.Text;
using System.Net;
using Skyline.DataMiner.Scripting;
using System.Globalization;

public class QAction
{
    public static void Run(SLProtocol protocol)
    {
        try
        {
            Object[] column = (Object[])protocol.NewRow();

            if ((column[3] != null) && (column[0] != null))
            {
                string myKey = Convert.ToString(((Object[])column[0])[0]);
                string myDispKey = Convert.ToString(((Object[])column[3])[0]) + "." + myKey;
                protocol.SetParameterIndexByKey(1220, myKey, 5, myDispKey);
            }
        }
        catch (Exception e)
        {
            protocol.Log("QA" + protocol.QActionID + "|Set Processor DisplayKey|Error/Message: " + e, LogType.Error, LogLevel.NoLogging);
        }
    }
}
