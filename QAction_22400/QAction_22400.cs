using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Skyline.DataMiner.Scripting;
using SLNetMessages = Skyline.DataMiner.Net.Messages;

/// <summary>
/// DataMiner QAction Class: Parse Key.
/// </summary>
public static class QAction
{
	/// <summary>
	/// The QAction entry point.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
	public static void Run(SLProtocol protocol)
	{
		try
		{
			object[] columns = (object[])protocol.NotifyProtocol((int)SLNetMessages.NotifyType.NT_GET_TABLE_COLUMNS, Parameter.Tcpconnectiontable.tablePid, new uint[] { Parameter.Tcpconnectiontable.Idx.tcpconnectiontableinstance_22401 });
			object[] instancesColumn = (object[])columns[0];

			List<object> keys = new List<object>();
			List<object> localType = new List<object>();
			List<object> localAddress = new List<object>();
			List<object> localPort = new List<object>();

			List<object> remoteType = new List<object>();
			List<object> remoteAddress = new List<object>();
			List<object> remotePort = new List<object>();

			for (int i = 0; i < instancesColumn.Length; i++)
			{
				string instance = Convert.ToString(instancesColumn[i]);
				keys.Add(instance);
				string[] split = instance.Split('.');

				if (split.Length < 4)
				{
					continue;
				}

				localType.Add(split[0]);
				int count = Convert.ToInt32(split[1]) + 2;

				List<object> listlocalIp = new List<object>();
				for (int j = 2; j < count; j++)
				{
					listlocalIp.Add(split[j]);
				}

				localAddress.Add(String.Join(".", listlocalIp));

				localPort.Add(split[count]);
				count++;

				remoteType.Add(split[count]);
				count++;

				int countRemote = Convert.ToInt32(split[count]);
				count++;

				countRemote = countRemote + count;

				List<object> listRemoteIp = new List<object>();
				for (int j = count; j < countRemote; j++)
				{
					listRemoteIp.Add(split[j]);
				}

				remoteAddress.Add(String.Join(".", listRemoteIp));

				remotePort.Add(split[countRemote]);
			}

			bool localLengthOk = localPort.Count == localType.Count && localType.Count == localAddress.Count;
			bool remoteLengthOk = remotePort.Count == remoteType.Count && remoteType.Count == remoteAddress.Count;

			if (localLengthOk && remoteLengthOk && localAddress.Count == remoteAddress.Count)
			{
				object[] pidsSet = new object[]
				{
					Parameter.Tcpconnectiontable.tablePid,
					Parameter.Tcpconnectiontable.Pid.tcpconnectionlocalipaddresstype_22404,
					Parameter.Tcpconnectiontable.Pid.tcpconnectionlocalipaddress_22405,
					Parameter.Tcpconnectiontable.Pid.tcpconnectionlocalipaddressport_22406,
					Parameter.Tcpconnectiontable.Pid.tcpconnectionremoteipaddresstype_22407,
					Parameter.Tcpconnectiontable.Pid.tcpconnectionremoteipaddress_22408,
					Parameter.Tcpconnectiontable.Pid.tcpconnectionremoteipaddressport_22409,
				};
				object[] valuesSet = new object[]
				{
					keys.ToArray(),
					localType.ToArray(),
					localAddress.ToArray(),
					localPort.ToArray(),
					remoteType.ToArray(),
					remoteAddress.ToArray(),
					remotePort.ToArray(),
				};

				protocol.NotifyProtocol((int)SLNetMessages.NotifyType.NT_FILL_ARRAY_WITH_COLUMN, pidsSet, valuesSet);
			}
		}
		catch (Exception ex)
		{
			protocol.Log("QA" + protocol.QActionID + "|" + protocol.GetTriggerParameter() + "|Run|Exception thrown:" + Environment.NewLine + ex, LogType.Error, LogLevel.NoLogging);
		}
	}
}