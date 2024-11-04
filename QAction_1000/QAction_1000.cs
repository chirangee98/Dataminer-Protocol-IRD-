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
        try
		{
			var emptyTotal = protocol.IsEmpty(Parameter.memtotalswap_1000);
			var emptyAvail = protocol.IsEmpty(Parameter.memavailswap_1001);

			if (emptyTotal && emptyAvail)
			{
				return;
			}

			protocol.SetParameter(Parameter.memusageswap_1002, GetUsageMemory(protocol));
		}
		catch (Exception e)
        {
            protocol.Log("QA" + protocol.QActionID + "|Set Swap Memory Usage|Error/Message: " + e, LogType.Error, LogLevel.NoLogging);
        }
    }

	private static double GetUsageMemory(SLProtocol protocol)
	{
		var swapMemoryDetails = (object[])protocol.GetParameters(new uint[] {Parameter.memtotalswap_1000, Parameter.memavailswap_1001});

		var totalMemory = Convert.ToDouble(swapMemoryDetails[0]);
		var availMemory = Convert.ToDouble(swapMemoryDetails[1]);
		var usageMemory = Convert.ToDouble(-10);

		return !totalMemory.Equals(0) ? 1000 * (1 - (availMemory/totalMemory)) : usageMemory;
	}
}