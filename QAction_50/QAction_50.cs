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
	/// <param name="deviceType">Device Type object.</param>
	public static void Run(SLProtocol protocol, Object deviceType)
	{
        try
		{
			var deviceData = (object[])deviceType;

			if (deviceData == null || deviceData.Length < 1 || (object[])deviceData[0] == null)
			{
				protocol.Log(string.Format("QA{0}|Run|HR Device table is null", protocol.QActionID), LogType.Error, LogLevel.NoLogging);
				return;
			}

			var rowCount = ((object[])deviceData[0]).Length;

			protocol.SetParameter(Parameter.numberofcores_11, GetNumberOfCores(deviceData, rowCount));
		}
		catch (Exception e)
        {
            protocol.Log("QA" + protocol.QActionID + "|Get Number of Processors|Error/Message: " + e, LogType.Error, LogLevel.NoLogging);
        }
    }

	private static int GetNumberOfCores(object[] deviceData, int rowCount)
	{
		var numberOfCores = 0;

		for (int i = 0; i < rowCount; i++)
		{
			var recOid = (string)((object[])deviceData[1])[i];

			if (recOid.Equals("1.3.6.1.2.1.25.3.1.3"))
			{
				numberOfCores++;
			}
		}

		return numberOfCores.Equals(0) ? 1 : numberOfCores;
	}
}