using System;
using Skyline.DataMiner.Scripting;

public class QAction
{
	public static void Run(SLProtocol protocol)
	{
        try
		{
            int iTriggerParam = protocol.GetTriggerParameter();
            Object[] column = (Object[])protocol.NewRow();

            if ((column[0] == null) || (column[1] == null))
            {
				protocol.Log("QA"+protocol.QActionID+"|New column created is null.", LogType.Error, LogLevel.NoLogging);
				return;
			}

            string instance = Convert.ToString(((Object[])column[0])[0]);
            if (protocol.GetKeyPosition(360, instance) != 0 && protocol.GetKeyPosition(400, instance) != 0)
            {
                ////protocol.Log(8,5,"QA350 - Instance : " + instance + " exists in table 400");
                int value = Convert.ToInt32(((Object[])column[1])[0]);
                object[] oSetRow = new object[10];

                if (iTriggerParam == 350) { oSetRow[8] = value; }
                if (iTriggerParam == 355) { oSetRow[9] = value; }

                object[] oRow_400 = (object[])protocol.GetRow(400, instance);
                string sizeUnit_s = Convert.ToString(oRow_400[7]);
                string totalSize_s = Convert.ToString(oRow_400[8]);
                string usedSize_s = Convert.ToString(oRow_400[9]);

                if (sizeUnit_s != string.Empty && totalSize_s != string.Empty && usedSize_s != string.Empty)
                {
                    int sizeUnit = Convert.ToInt32(oRow_400[7]);
                    int totalSize = Convert.ToInt32(oRow_400[8]);
                    int usedSize = Convert.ToInt32(oRow_400[9]);

                    ////protocol.Log(8,5,"Computing HD free size with : Unit Size = " + sizeUnit + " - Total Size = " + totalSize + " - Used Size = " +  usedSize);

                    double remainingSize = (Convert.ToDouble(totalSize - usedSize)) * (Convert.ToDouble(sizeUnit)) / Convert.ToDouble(1024);
                    double storageSize = (Convert.ToDouble(totalSize)) * (Convert.ToDouble(sizeUnit)) / Convert.ToDouble(1024);
                    double storageUsed = 0;

                    oSetRow[4] = remainingSize;
                    oSetRow[5] = storageSize;

                    if (totalSize != 0) { storageUsed = Math.Floor((1000 * Convert.ToDouble(usedSize)) / Convert.ToDouble(totalSize)); }

                    ////protocol.Log(8,5,"QA350 - => Remaining (MB) = " + remainingSize + " - Storage Size = " + storageSize + " - Storage Used (%)" + storageUsed);

                    if (storageUsed >= 0 && storageUsed <= 1000)
                    {
                        oSetRow[6] = storageUsed;
                    }
                    else
                    {
                        oSetRow[6] = -1;
                    }
                }

                protocol.SetRow(Parameter.Storagetable.tablePid, instance, oSetRow);                  
            }
        }
        catch (Exception e)
        {
            protocol.Log("QA" + protocol.QActionID + "|Process Storage Table|Error/Message: " + e, LogType.Error, LogLevel.NoLogging);
        }
    }
}