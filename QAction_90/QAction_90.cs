using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Skyline.DataMiner.Scripting;

/// <summary>
/// DataMiner QAction Class.
/// </summary>
public class QAction
{
	/// <summary>
	/// Process to calculate CPU Usage Load.
	/// </summary>
	/// <param name="protocol">Link with Skyline DataMiner.</param>
	/// <param name="taskTable">Current Task Manager Table object.</param>
	public static void Run(SLProtocolExt protocol, Object taskTable)
	{
		try
		{
			ProcessCpu processCpu = new ProcessCpu
			{
				TotalCpu = 0,
				UpdateCpu = 0,
			};

			DateTime currentDateTime = DateTime.Now;
			var now = Convert.ToDouble(currentDateTime.Ticks);

			var parameterValues = (object[])protocol.GetParameters(new uint[] { Parameter.time_flag_new_20, Parameter.numberofcores_11 });
			var oldTime = Convert.ToDouble(parameterValues[0]);
			var numberOfCores = Convert.ToDouble(parameterValues[1]);
			var deltaTime = now - oldTime;
			protocol.SetParameter(Parameter.time_flag_new_20, now);

			bool notUpdateCpuUsage = oldTime.Equals(0) || now.Equals(oldTime) || numberOfCores.Equals(0);

			int[] taskIndexes = new[]
				{
					Parameter.Taskmanager.Idx.taskmanagerprocessnameindex_83,
					Parameter.Taskmanager.Idx.taskmanagerprocesspid_81,
					Parameter.Taskmanager.Idx.taskmanagerprocesscpu_84,
					Parameter.Taskmanager.Idx.taskmanagerprocesscputime_85,
					Parameter.Taskmanager.Idx.taskmanagerrowstatus_87,
					Parameter.Taskmanager.Idx.taskmanagerpreviousperf_89,
				};

			var taskManagerTable = (object[])protocol.NotifyProtocol(
				321 /*NT_GET_TABLE_COLUMNS*/,
				Parameter.Taskmanager.tablePid,
				taskIndexes);

			// Loop on the task manager table (80)
			if (taskManagerTable == null || (object[])taskManagerTable[0] == null)
			{
				return;
			}

			int[] runIndexes = new[]
				{
					Parameter.Hrswrunperftable.Idx.hrswrunperftable_instance_91,
					Parameter.Hrswrunperftable.Idx.hrswrunperfcpu_92,
					Parameter.Hrswrunperftable.Idx.hrswrunperfpreviouscpu_94,
				};

			var runPerfTable = (object[])protocol.NotifyProtocol(
				321 /*NT_GET_TABLE_COLUMNS*/,
				Parameter.Hrswrunperftable.tablePid,
				runIndexes);

			var runPerfTablePKs = new List<object>(Array.ConvertAll((object[])runPerfTable[0], Convert.ToString));

			var rowCount = ((object[])taskManagerTable[0]).Length;
			var keys = (object[])taskManagerTable[0];
			var processesPID = (object[])taskManagerTable[1];
			var processesCPU = (object[])taskManagerTable[2];
			var processesCPUTime = (object[])taskManagerTable[3];
			var processesState = (object[])taskManagerTable[4];

			List<object>[] listTaskManagerCpu = new List<object>[2];
			List<object>[] listTaskManagerTime = new List<object>[2];
			List<object>[] listHrSWRunPerfCols = new List<object>[2];

			Initialize(listTaskManagerCpu, listTaskManagerTime, listHrSWRunPerfCols);

			for (int i = 0; i < rowCount; i++)
			{
				// Get PID of the process
				var processPID = Convert.ToString(processesPID[i]);

				// Check if process is still present in the SNMP table ( state different than 4) and if the PID exists in CPU table (90)
				int state = Convert.ToInt32(processesState[i]);
				if (state == 4 || String.IsNullOrEmpty(processPID) || !runPerfTablePKs.Contains(processPID))
				{
					continue;
				}

				var processPIDKey = runPerfTablePKs.IndexOf(processPID);
				var previousCPUTime = Convert.ToString(processesCPUTime[i]);
				var newCPUTime = Convert.ToDouble(((object[])runPerfTable[1])[processPIDKey], CultureInfo.InvariantCulture);

				GetProcessCpuTime(keys, listTaskManagerTime, i, previousCPUTime, newCPUTime);

				var prevCPUval = ((object[])runPerfTable[2])[processPIDKey];
				double oldCPUTime = Convert.ToDouble(prevCPUval);

				GetHrSwRunPerfPrevCpu(listHrSWRunPerfCols, processPID, newCPUTime, prevCPUval, oldCPUTime);

				if (notUpdateCpuUsage)
				{
					continue;
				}

				double cpuUsage;
				if (newCPUTime > oldCPUTime && deltaTime > 0 && prevCPUval != null)
				{
					cpuUsage = 10000000 * (newCPUTime - oldCPUTime) / (deltaTime * numberOfCores);
					processCpu.TotalCpu += cpuUsage;
				}
				else
				{
					cpuUsage = 0;
				}

				GerProcessCpuUsage(processCpu, keys, processesCPU, listTaskManagerCpu, i, cpuUsage);
			}

			UpdateTables(protocol, listTaskManagerCpu, listTaskManagerTime, listHrSWRunPerfCols);

			UpdateTotalProcessorLoad(protocol, processCpu);
		}
		catch (Exception e)
		{
			protocol.Log("QA" + protocol.QActionID + "|Set Task Manager CPU|Error/Message: " + e, LogType.Error, LogLevel.NoLogging);
		}
	}

	private static void UpdateTotalProcessorLoad(SLProtocolExt protocol, ProcessCpu processCpu)
	{
		if (processCpu.TotalCpu <= 100 && processCpu.TotalCpu >= 0 && processCpu.UpdateCpu == 1)
		{
			protocol.SetParameter(Parameter.totalprocessorload_10, processCpu.TotalCpu);
		}
	}

	private static void GerProcessCpuUsage(ProcessCpu processCpu, object[] keys, object[] processesCPU, List<object>[] listTaskManagerCpu, int i, double cpuUsage)
	{
		if (string.IsNullOrEmpty(Convert.ToString(processesCPU[i])) || !cpuUsage.Equals(Convert.ToDouble(processesCPU[i])))
		{
			listTaskManagerCpu[0].Add(Convert.ToString(keys[i]));
			listTaskManagerCpu[1].Add(cpuUsage);
			processCpu.UpdateCpu = 1;
		}
	}

	private static void GetHrSwRunPerfPrevCpu(List<object>[] listHrSWRunPerfCols, string processPID, double newCPUTime, object prevCPUval, double oldCPUTime)
	{
		if (prevCPUval == null || !newCPUTime.Equals(oldCPUTime))
		{
			listHrSWRunPerfCols[0].Add(processPID);
			listHrSWRunPerfCols[1].Add(newCPUTime);
		}
	}

	private static void GetProcessCpuTime(object[] keys, List<object>[] listTaskManagerTime, int i, string previousCPUTime, double newCPUTime)
	{
		double iTime = Math.Floor(newCPUTime / 100);
		double iSeconds = iTime % 60;
		iTime = Math.Floor(iTime / 60);
		double iMinutes = iTime % 60;
		iTime = Math.Floor(iTime / 60);
		StringBuilder sbCPU = new StringBuilder();
		sbCPU.Append(iTime.ToString());
		sbCPU.Append(":");
		sbCPU.Append(String.Format("{0:D2}", Convert.ToInt32(iMinutes)));
		sbCPU.Append(":");
		sbCPU.Append(String.Format("{0:D2}", Convert.ToInt32(iSeconds)));
		var strCPU = sbCPU.ToString();

		if (!previousCPUTime.Equals(strCPU))
		{
			listTaskManagerTime[0].Add(Convert.ToString(keys[i]));
			listTaskManagerTime[1].Add(strCPU);
		}
	}

	private static void Initialize(List<object>[] listTaskManagerCpu, List<object>[] listTaskManagerTime, List<object>[] listhrSWRunPerfCols)
	{
		for (int i = 0; i < listTaskManagerCpu.Length; i++)
		{
			listTaskManagerCpu[i] = new List<object>();
		}

		for (int i = 0; i < listTaskManagerTime.Length; i++)
		{
			listTaskManagerTime[i] = new List<object>();
		}

		for (int i = 0; i < listhrSWRunPerfCols.Length; i++)
		{
			listhrSWRunPerfCols[i] = new List<object>();
		}
	}

	private static void UpdateTables(SLProtocolExt protocol, List<object>[] listTaskManagerCpu, List<object>[] listTaskManagerTime, List<object>[] listhrSWRunPerfCols)
	{
		if (listTaskManagerCpu[0].Count > 0)
		{
			object[] columnPids = new object[]
			{
					Parameter.Taskmanager.tablePid,
					Parameter.Taskmanager.Pid.taskmanagerprocesscpu_84,
			};

			object[] columnData = new object[]
			{
					listTaskManagerCpu[0].ToArray(),
					listTaskManagerCpu[1].ToArray(),
			};

			protocol.NotifyProtocol(220, columnPids, columnData);
		}

		if (listTaskManagerTime[0].Count > 0)
		{
			object[] columnPids = new object[]
			{
					Parameter.Taskmanager.tablePid,
					Parameter.Taskmanager.Pid.taskmanagerprocesscputime_85,
			};

			object[] columnData = new object[]
			{
					listTaskManagerTime[0].ToArray(),
					listTaskManagerTime[1].ToArray(),
			};

			protocol.NotifyProtocol(220, columnPids, columnData);
		}

		if (listhrSWRunPerfCols[0].Count > 0)
		{
			object[] columnPids = new object[]
			{
					Parameter.Hrswrunperftable.tablePid,
					Parameter.Hrswrunperftable.Pid.hrswrunperfpreviouscpu_94,
			};

			object[] columnData = new object[]
			{
					listhrSWRunPerfCols[0].ToArray(),
					listhrSWRunPerfCols[1].ToArray(),
			};

			protocol.NotifyProtocol(220, columnPids, columnData);
		}
	}
}

public class ProcessCpu
{
	public double TotalCpu { get; set; }

	public int UpdateCpu { get; set; }
}