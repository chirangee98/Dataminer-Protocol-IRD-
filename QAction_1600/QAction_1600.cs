
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Skyline.DataMiner.Scripting;
using System.Linq;

public class QAction
{
    /// <summary>
    /// fill table with exception values
    /// </summary>
    /// <param name="protocol">Link with Skyline DataMiner</param>
    public static void Run(SLProtocol protocol)
    {

        int iTriggered = protocol.GetTriggerParameter();

        object[] oKeys = (object[])((object[])protocol.NotifyProtocol(321, iTriggered, new UInt32[] { 0 }))[0];
        List<int> lColumnIndex = new List<int>();
        try
        {
            if (oKeys != null)
            {

                object[] oRow = (object[])protocol.GetRow(iTriggered, Convert.ToString(oKeys[0]));

                if (oRow != null)
                {
                    for (int j = 0; j < oRow.Length; j++)
                    {
                        if (Convert.ToString(oRow[j]) == "")
                            lColumnIndex.Add(j);
                    }

                    foreach (int index in lColumnIndex)
                    {
                        object[] oArrayOfMinusOne = new object[oKeys.Length];
                        oArrayOfMinusOne = oArrayOfMinusOne.Select(x => (object)-1).ToArray();
                        protocol.NotifyProtocol(220, new object[] { iTriggered, iTriggered + 1 + index }, new object[] { oKeys, oArrayOfMinusOne });
                    }
                }
            }
        }
        catch (Exception e)
        {
            protocol.Log("QA" + protocol.QActionID + "|Fill Table With Exception Values|Error/Message: " + e, LogType.Error, LogLevel.NoLogging);
        }
    }
}
