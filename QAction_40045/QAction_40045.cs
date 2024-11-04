using System;
using Skyline.DataMiner.Scripting;

/// <summary>
/// DataMiner QAction Class.
/// </summary>
public class QAction
{
	/// <summary>
	/// The QAction entry point.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
    public static void Run(SLProtocol protocol)
    {
		var parameterValues = (object[])protocol.GetParameters(new uint[] { Parameter.Write.removeport_40045, 2 /* Element ID */});

        var rowKey = Convert.ToString(parameterValues[0]);
        var elementID = Convert.ToString(parameterValues[1]);
        var uniqueID = elementID + "-" + rowKey;

        try
        {
            lock (multiThreadMethods.multiThreadMethods.provideKeyLock(uniqueID))
            {
                if (protocol.GetKeyPosition(Parameter.Portlist.tablePid, rowKey) != 0)
                {
					protocol.DeleteRow(Parameter.Portlist.tablePid, rowKey);
                }

                multiThreadMethods.multiThreadMethods.removeKeyFromDictionary(uniqueID);
            }
        }
        catch (Exception e)
        {
            protocol.Log("QA" + protocol.QActionID + "|Remove Port from Port List|Error/Message: " + e, LogType.Error, LogLevel.NoLogging);
        }
    }
}