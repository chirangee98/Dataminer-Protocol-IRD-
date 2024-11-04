using System;
using Skyline.DataMiner.Scripting;

public class QAction
{
	/// <summary>
	/// Write BW
	/// </summary>
	/// <param name="protocol">Link with Skyline Dataminer</param>
	public static void Run(SLProtocol protocol)
	{
        try
		{
            var rowKey = protocol.RowKey();
			var row = new InterfacetableconfigQActionRow((object[])protocol.GetRow(Parameter.Interfacetableconfig.tablePid, rowKey));
            if (row == null)
            {
				return;
			}

            var interfaceBW = Convert.ToString(row.Ifinterfacebandwidth_805);
            if (interfaceBW.Equals("0"))
            {
				long interfaceBWValue = Convert.ToInt64(protocol.GetParameter(Parameter.Write.interfacesinterfacebandwidth_655));
				protocol.SetParameterIndexByKey(Parameter.Interfacestable.tablePid, rowKey, 7, interfaceBWValue * 1000000);
            }
            else
            {
                protocol.Log(8, 5, "Cannot set BW for IF " + rowKey + ": ");
            }
            
        }
        catch (Exception e)
        {
            protocol.Log("QA" + protocol.QActionID + "|Write Bandwidth|Error/Message: " + e, LogType.Error, LogLevel.NoLogging);
        }
    }
}