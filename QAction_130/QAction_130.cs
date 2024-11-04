
//  Process Software Info
using System;
using System.IO;
using System.Text;
using Skyline.DataMiner.Scripting;
using System.Globalization;
using sharedMethods;

public class QAction
{
	public static void Run(SLProtocol protocol)
	{
        try {
            String sKey = protocol.RowKey();
            if ((sKey != "") && (protocol.GetKeyPosition(130, sKey) != 0))
            {
                String sRawDate = Convert.ToString(protocol.GetParameterIndexByKey(130, sKey, 3));
                protocol.SetParameterIndexByKey(130, sKey, 4, sharedMethods.sharedMethods.getDateFromHexValue(sRawDate));

            }
        }
        catch (Exception e)
        {
            protocol.Log("QA" + protocol.QActionID + "|Process Software Info|Error/Message: " + e, LogType.Error, LogLevel.NoLogging);
        }
    }    
}
    
