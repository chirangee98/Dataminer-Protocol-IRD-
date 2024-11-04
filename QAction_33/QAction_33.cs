using System;
using Skyline.DataMiner.Scripting;

//Check row when monstatus changes
public class QAction
{
    public static void Run(SLProtocol protocol)
    {
        try
        {
            Object[] row = (Object[])protocol.GetRow(Parameter.Processvalidationtable.tablePid, protocol.RowKey());
            string processKey = Convert.ToString(row[0]);
            string processStatus = Convert.ToString(row[1]);
            string monStatus = Convert.ToString(row[2]);
            if (monStatus == "Monitored")
            {
                string svalidateProcess = sharedMethods.sharedMethods.validateProcess(protocol, processKey);

                if (processStatus != svalidateProcess)
                {
                    row[1] = svalidateProcess;
                    protocol.SetRow(Parameter.Processvalidationtable.tablePid, processKey, row);
                }
            }
            else if (monStatus == "Delete")
            {
				protocol.DeleteRow(Parameter.Processvalidationtable.tablePid,  Convert.ToString(processKey));
            }
            else
            {
                row[1] = "Not Monitored";
                protocol.SetRow(Parameter.Processvalidationtable.tablePid, processKey, row);
            }
        }
        catch (Exception e)
        {
            protocol.Log("QA" + protocol.QActionID + "|Check Row on Monitoring Status|Error/Message: " + e, LogType.Error, LogLevel.NoLogging);
        }
    }
}