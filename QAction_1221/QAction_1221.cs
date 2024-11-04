using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Skyline.DataMiner.Scripting;

public class QAction
{
	/// <summary>
	/// Calculate Total CPU.
	/// </summary>
	/// <param name="protocol">Link with Skyline DataMiner.</param>
	public static void Run(SLProtocolExt protocol)
	{
		try
		{
			object[] oTable = (object[])protocol.NotifyProtocol(
				321,
				Parameter.Processortable.tablePid/*1220*/,
				new UInt32[] { Parameter.Processortable.Idx.processorinstance_1221, Parameter.Processortable.Idx.processorload_1223 });

			// Number of cores can cause false data, as in a real test, a device reported 1 core but it has 8 processors.
			double dNumberCores = Convert.ToDouble(protocol.GetParameter(Parameter.numberofcores_11));

			List<int> lCpuLoad = new List<int>(Array.ConvertAll<object, int>((object[])oTable[1], new Converter<object, int>(Convert.ToInt32)));
			double dNumberProcessors = lCpuLoad.Count;
			int totalCpu = lCpuLoad.Sum();
			if (dNumberProcessors > 0)
			{
				if(dNumberCores < dNumberProcessors)
				{
					dNumberCores = dNumberProcessors;
				}

				protocol.SetParameters(
					new[] { Parameter.totalprocessorload_10, Parameter.numberofcores_11 }, 
					new object[] { Convert.ToDouble(totalCpu) / dNumberProcessors, dNumberCores }
				);
			}
			else
			{
				protocol.Log("QA" + protocol.QActionID + "|CalculateTotalCPU|Incorrect provided value for Number of Cores, value not set.", LogType.DebugInfo, LogLevel.NoLogging);
			}
		}
		catch(Exception e)
		{
			protocol.Log("QA" + protocol.QActionID + "|CalculateTotalCPU|" + e, LogType.Error, LogLevel.NoLogging);
		}
	}
}