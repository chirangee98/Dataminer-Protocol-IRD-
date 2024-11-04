using System;
using System.Collections.Generic;
using Skyline.DataMiner.Scripting;

/// <summary>
/// DataMiner QAction Class.
/// </summary>
public class QAction
{
	/// <summary>
	/// Interpret all Interfaces.
	/// </summary>
	/// <param name="protocol">Link with Skyline DataMiner.</param>
	public static void Run(SLProtocolExt protocol)
	{
		try
		{
			int[] indexes = new[]
				{
					Parameter.Interfacetableconfig.Idx.indexinterfacetableconfig_801,
					Parameter.Interfacetableconfig.Idx.operationalstatusinterfacetableconfig_808,
					Parameter.Interfacetableconfig.Idx.monitorinterfacetableconfig_818,
				};

			var monitor = (object[])protocol.NotifyProtocol(
				321,
				Parameter.Interfacetableconfig.tablePid,
				indexes);

			var monitorState = (object[])monitor[2];
			if (monitorState.Length > 0)
			{
				var stateValue = Convert.ToString(monitorState[0]);
				if (!string.IsNullOrEmpty(stateValue))
				{
					return;
				}
			}

			var indeces = (object[])monitor[0];
			var rowStatus = (object[])monitor[1];
			List<object> states = new List<object>();

			foreach (object state in rowStatus)
			{
				int iState = Convert.ToInt32(state);
				states.Add(iState.Equals(2) ? 2 : 1);
			}

			if (states.Count > 0)
			{
				protocol.FillArrayWithColumn(Parameter.Interfacetableconfig.tablePid, Parameter.Interfacetableconfig.Pid.monitorinterfacetableconfig_818, indeces, states.ToArray());
			}
		}
		catch (Exception ex)
		{
			protocol.Log("QA" + protocol.QActionID + "|Message|" + ex.Message, LogType.Error, LogLevel.NoLogging);
		}
	}
}