using System;
using System.Collections.Generic;
using Skyline.DataMiner.Scripting;

public class QAction
{
	public enum MonitoringState
	{
		Enabled = 1,
		Disabled = 2,
	}

	/// <summary>
	/// Change All Interface Monitoring.
	/// </summary>
	/// <param name="protocol">Link with Skyline DataMiner.</param>
	public static void Run(SLProtocolExt protocol)
	{
		try
		{
			int value = Convert.ToInt32(protocol.Write.Changeallinterfacemonitoringstates);
			switch (value)
			{
				case 1: // Refresh
					protocol.CheckTrigger(21); // Poll Interface Table
					break;
				case 2:
					ProcessOperationalState(protocol);
					protocol.CheckTrigger(21); // Poll Interface Table
					break;
				case 3: // disable all
					SetAllInterfaceMonitoringStates(protocol, MonitoringState.Disabled);
					protocol.CheckTrigger(21); // Poll Interface Table
					break;
				case 4: // enable all
					SetAllInterfaceMonitoringStates(protocol, MonitoringState.Enabled);
					protocol.CheckTrigger(21); // Poll Interface Table
					break;
				default:
					return;
			}
		}
		catch (Exception ex)
		{
			protocol.Log("QA" + protocol.QActionID + "|Message|Button 59|" + ex, LogType.Error, LogLevel.NoLogging);
		}
	}

	public static void ProcessOperationalState(SLProtocolExt protocol)
	{
		object[] intfTable = (object[])protocol.NotifyProtocol(321, Parameter.Interfacetableconfig.tablePid, new UInt32[] { 0, 6 });
		object[] intfIndex = (object[])intfTable[0];
		object[] operationStateColumn = (object[])intfTable[1];

		List<object> valueList = new List<object>();

		foreach (var state in operationStateColumn)
		{
			int numState = Convert.ToInt32(state);
			if (numState == 1 || numState == 3)
			{
				valueList.Add(1);
			}
			else
			{
				valueList.Add(2);
			}
		}

		if (valueList.Count > 0) // Update Poll table
		{
			protocol.FillArrayWithColumn(Parameter.Interfacetableconfig.tablePid, Parameter.Interfacetableconfig.Pid.monitorinterfacetableconfig, intfIndex, valueList.ToArray());
		}
	}

	public static void SetAllInterfaceMonitoringStates(SLProtocolExt protocol, MonitoringState state)
	{
		string[] keys = protocol.GetKeys(Parameter.Interfacetableconfig.tablePid);
		List<object> keysToUpdate = new List<object>();
		List<object> valuesToUpdate = new List<object>();

		foreach (string key in keys)
		{
			keysToUpdate.Add(key);
			valuesToUpdate.Add(Convert.ToInt32(state));
		}

		if (keysToUpdate.Count > 0) // Update Poll table
		{
			protocol.FillArrayWithColumn(Parameter.Interfacetableconfig.tablePid, Parameter.Interfacetableconfig.Pid.monitorinterfacetableconfig, keysToUpdate.ToArray(), valuesToUpdate.ToArray());
		}
	}
}