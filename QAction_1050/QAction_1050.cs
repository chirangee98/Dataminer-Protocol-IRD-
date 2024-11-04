using System;
using System.Collections.Generic;
using Skyline.DataMiner.Scripting;

public class QAction
{
    /// <summary>
    /// Calculate Disk Used Space Percentage.
    /// </summary>
    /// <param name="protocol">Link with Skyline DataMiner.</param>
    public static void Run(SLProtocolExt protocol)
    {
        object[] diskInfoTable = (object[])protocol.NotifyProtocol(
            321,
            Parameter.Linuxmonitoreddisks.tablePid /*1050*/,
            new int[] { 1, 6, 8 });
        object[] diskInfoKeys = (object[])diskInfoTable[0];
        object[] diskInfoTotalSize = (object[])diskInfoTable[1];
        object[] diskInfoUsedSpace = (object[])diskInfoTable[2];
        List<object> diskValues = new List<object>();
        try
        {
            for (int i = 0; i < diskInfoKeys.Length; i++)
            {
                int totalSpace = Convert.ToInt32(diskInfoTotalSize[i]);
                int usedSpacePercentage = Convert.ToInt32(diskInfoUsedSpace[i]);
                diskValues.Add(usedSpacePercentage * totalSpace / (100 * 1048576));
            }

            protocol.linuxmonitoreddisks.SetColumn(
                Parameter.Linuxmonitoreddisks.Idx.linuxmonitoreddisksusedspaceabsolute_1067,
                Array.ConvertAll(diskInfoKeys, Convert.ToString),
                diskValues.ToArray());
            
        }
        catch (Exception e)
        {
            protocol.Log("QA" + protocol.QActionID + "|Calculate Disk Used Space|Error/Message: " + e.ToString(), LogType.Error, LogLevel.NoLogging);
        }
    }
}