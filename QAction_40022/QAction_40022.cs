using System;
using Skyline.DataMiner.Scripting;

public class QAction{
	public static void Run(SLProtocol protocol){
        try {
            // Get modified port
            string sRowKey = protocol.RowKey();

            // get new status
            int myValue = Convert.ToInt32(protocol.GetParameter(Parameter.Write.portliststatus_40022));

            // Copy read to write and update result parameters
            if (protocol.GetKeyPosition(Parameter.Portlist.tablePid, sRowKey) != 0) {

                multiThreadMethods.multiThreadMethods.setRowValue(protocol, sRowKey, 3, myValue);

                if (myValue == 1) { 
                    //Test Enabled =>  Configure result parameters to "Undefined"
                    protocol.SetParameter(Parameter.Write.triggerrowundefined_40005, sRowKey);
                }
                else{
                    // test Disabled => Configure result parameters to "Disabled"
                    protocol.SetParameter(Parameter.Write.triggerrowdisabled_40004, sRowKey);
                }
            }
        }
        catch (Exception e){
            protocol.Log("QA" + protocol.QActionID + "|Update Status|Error/Message: " + e, LogType.Error, LogLevel.NoLogging);
        }
    }
}		