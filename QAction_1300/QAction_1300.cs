using System;
using Skyline.DataMiner.Scripting;

/// <summary>
/// DataMiner QAction Class.
/// </summary>
public class QAction
{
	private const int notAvailableNumeric = -1;

	/// <summary>
	/// The QAction entry point.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
	public static void Run(SLProtocol protocol)
	{
        try
		{
			// New values
			double totalSum = 0, param1310 = 0, param1311 = 0, param1312 = 0, param1313 = 0, param1314 = 0, param1315 = 0, param1316 = 0;

			// Retrieve all the separate parameters to calculate the total load (new values)
			var currentIds = new uint[]
			{
				Parameter.rawcpuuser_1310, Parameter.rawcpunice_1311,
				Parameter.rawcpusystem_1312, Parameter.rawcpuidle_1313,
				Parameter.rawcpuwait_1314, Parameter.rawcpukernel_1315, Parameter.rawcpuinterrupt_1316
			};

			var currentParams = (object[])protocol.GetParameters(currentIds);

			if (Convert.ToString(currentParams[0]) != string.Empty)
			{
				param1310 = Convert.ToDouble(currentParams[0]);
			}

			if (Convert.ToString(currentParams[1]) != string.Empty)
			{
				param1311 = Convert.ToDouble(currentParams[1]);
			}

			if (Convert.ToString(currentParams[2]) != string.Empty)
			{
				param1312 = Convert.ToDouble(currentParams[2]);
			}

			if (Convert.ToString(currentParams[3]) != string.Empty)
			{
				param1313 = Convert.ToDouble(currentParams[3]);
			}

			if (Convert.ToString(currentParams[4]) != string.Empty)
			{
				param1314 = Convert.ToDouble(currentParams[4]);
			}

			if (Convert.ToString(currentParams[5]) != string.Empty)
			{
				param1315 = Convert.ToDouble(currentParams[5]);
			}

			if (Convert.ToString(currentParams[6]) != string.Empty)
			{
				param1316 = Convert.ToDouble(currentParams[6]);
			}

			// Previous values
			double param1320 = 0, param1321 = 0, param1322 = 0, param1323 = 0, param1324 = 0, param1325 = 0, param1326 = 0;

			var oldIds = new uint[]
			{
				Parameter.rawcpuuserbuf_1320, Parameter.rawcpunicebuf_1321,
				Parameter.rawcpusystembuf_1322, Parameter.rawcpuidlebuf_1323,
				Parameter.rawcpuwaitbuf_1324, Parameter.rawcpukernelbuf_1325, Parameter.rawcpuinterruptbuf_1326
			};

			var oldParams = (object[])protocol.GetParameters(oldIds);
			var notAvailable = false;

			// Retrieve all the separate parameters to calculate the total load (old values)
			if (Convert.ToString(oldParams[0]) != string.Empty)
			{
				param1320 = Convert.ToDouble(oldParams[0]);
			}

			if (Convert.ToString(oldParams[1]) != string.Empty)
			{
				param1321 = Convert.ToDouble(oldParams[1]);
			}

			if (Convert.ToString(oldParams[2]) != string.Empty)
			{
				param1322 = Convert.ToDouble(oldParams[2]);
			}

			if (Convert.ToString(oldParams[3]) != string.Empty)
			{
				param1323 = Convert.ToDouble(oldParams[3]);
			}

			if (Convert.ToString(oldParams[4]) != string.Empty)
			{
				param1324 = Convert.ToDouble(oldParams[4]);
			}

			if (Convert.ToString(oldParams[5]) != string.Empty)
			{
				param1325 = Convert.ToDouble(oldParams[5]);
			}

			if (Convert.ToString(oldParams[6]) != string.Empty)
			{
				param1326 = Convert.ToDouble(oldParams[6]);
			}

			if (IsAnyParameterEmpty(protocol))
			{
				notAvailable = true;
			}

			totalSum = (param1310 - param1320) + (param1311 - param1321) +
						(param1312 - param1322) + (param1313 - param1323) +
						(param1314 - param1324) + (param1315 - param1325) + (param1316 - param1326);

			// To avoid division by 0
			if (string.Equals(Convert.ToString(0), Convert.ToString(totalSum)))
			{
				totalSum = 1;
			}

			protocol.SetParameters(
				new[]
				{
					Parameter.rawcpuuserbuf_1320, Parameter.rawcpunicebuf_1321,
					Parameter.rawcpusystembuf_1322, Parameter.rawcpuidlebuf_1323,
					Parameter.rawcpuwaitbuf_1324, Parameter.rawcpukernelbuf_1325, Parameter.rawcpuinterruptbuf_1326
				},
				new object[] { param1310, param1311, param1312, param1313, param1314, param1315, param1316 });

			if (!notAvailable)
			{
				param1310 = ((param1310 - param1320) / totalSum) * 100;
				param1311 = ((param1311 - param1321) / totalSum) * 100;
				param1312 = ((param1312 - param1322) / totalSum) * 100;
				param1313 = ((param1313 - param1323) / totalSum) * 100;
				param1314 = ((param1314 - param1324) / totalSum) * 100;
				param1315 = ((param1315 - param1325) / totalSum) * 100;
				param1316 = ((param1316 - param1326) / totalSum) * 100;
			}
			else
			{
				param1310 = notAvailableNumeric;
				param1311 = notAvailableNumeric;
				param1312 = notAvailableNumeric;
				param1313 = notAvailableNumeric;
				param1314 = notAvailableNumeric;
				param1315 = notAvailableNumeric;
				param1316 = notAvailableNumeric;
			}

			protocol.SetParameters(
				new[]
				{
					Parameter.cpurawuser_1301, Parameter.cpurawnice_1302,
					Parameter.cpurawsystem_1303, Parameter.cpurawidle_1304,
					Parameter.cpurawwait_1305, Parameter.cpurawkernel_1306, Parameter.cpurawinterrupt_1307
				},
				new object[] { param1310, param1311, param1312, param1313, param1314, param1315, param1316 }
			);
		}
		catch (Exception e)
        {
            protocol.Log("QA" + protocol.QActionID + "|Set Raw CPU Data|Error/Message: " + e, LogType.Error, LogLevel.NoLogging);
        }
    }

	private static bool IsAnyParameterEmpty(SLProtocol protocol)
	{
		return protocol.IsEmpty(Parameter.rawcpuuserbuf_1320) || protocol.IsEmpty(Parameter.rawcpunicebuf_1321) ||
				protocol.IsEmpty(Parameter.rawcpusystembuf_1322) || protocol.IsEmpty(Parameter.rawcpuidlebuf_1323) ||
				protocol.IsEmpty(Parameter.rawcpuwaitbuf_1324) || protocol.IsEmpty(Parameter.rawcpukernelbuf_1325) ||
				protocol.IsEmpty(Parameter.rawcpuinterruptbuf_1326);
	}
}