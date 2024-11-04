using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Skyline.DataMiner.Scripting;

/// <summary>
/// DataMiner QAction Class.
/// </summary>
public class QAction
{
	/// <summary>
	/// MenuContext
	/// </summary>
	/// <param name="protocol">Link with Skyline DataMiner.</param>
	/// <param name="extraData">Extra input data.</param>
	public static void Run(SLProtocolExt protocol, object extraData)
	{
		var trigger = protocol.GetTriggerParameter();
		try
		{
			var values = extraData as string[];
			switch (trigger)
			{
				case Parameter.Write.processcounter_contextmenu_79: // Context Menu 
					{
						if (values == null || values.Length < 3)
							return;
						ProcessCounter(protocol, values);
						break;
					}

				case 76: // Update on Process Counters table after group
					{
						UpdateProcessCounterTable_AfterGroup(protocol);
						break;
					}

				case 74: // Update on Storage table after group
					{
						UpdateStorageTable_AfterGroup(protocol);
						break;
					}

				case Parameter.Write.storageavailability_contextmenu_504:// Context menu
					{
						if (values == null || values.Length < 3)
							return;
						StorageState(protocol, values);
						break;
					}

				case Parameter.Write.mountavailability_contextmenu_1049:// Context menu
					{
						if (values == null || values.Length < 3)
							return;
						AvailabilityState(protocol, values[1], values[2]);
						break;
					}

				case 75: // Update on Linux Monitor table after group
					{
						UpdateLinuxMonitor_AfterGroup(protocol);
						break;
					}

				default:
					protocol.Log("QA" + protocol.QActionID + "|Process Context Menu|Unimplemented trigger: " + trigger, LogType.Error, LogLevel.NoLogging);
					break;
			}
		}
		catch (Exception ex)
		{
			protocol.Log("QA" + protocol.QActionID + "|Process Context Menu|Error/Message: " + ex, LogType.Error, LogLevel.NoLogging);
		}
	}

	private static void UpdateLinuxMonitor_AfterGroup(SLProtocolExt protocol)
	{
		var mountPaths = (object[])((object[])protocol.NotifyProtocol(321, Parameter.Linuxmonitoreddisks.tablePid, new uint[] { 2 }))[0];
		var availNames = (object[])((object[])protocol.NotifyProtocol(321, Parameter.Mountavailability.tablePid, new uint[] { 0 }))[0];

		var pathList = new List<string>();
		foreach (var path in mountPaths)
		{
			pathList.Add(Convert.ToString(path));
		}

		var tablerowComposed = new List<QActionTableRow>();
		foreach (var name in availNames)
		{
			var realName = Convert.ToString(name);
			if (pathList.Contains(realName))
			{
				tablerowComposed.Add(new MountavailabilityQActionRow
				{
					Mountavailabilitymountname_1041 = realName,
					Mountavailabilityavailability_1042 = Convert.ToInt32(true),
				});
			}
			else
			{
				tablerowComposed.Add(new MountavailabilityQActionRow
				{
					Mountavailabilitymountname_1041 = realName,
					Mountavailabilityavailability_1042 = Convert.ToInt32(false),
				});
			}

		}
		protocol.mountavailability.FillArray(tablerowComposed);
	}

	private static void UpdateProcessCounterTable_AfterGroup(SLProtocolExt protocol)
	{
		try
		{
			var oaTaskManagerColumns = (object[])protocol.NotifyProtocol(
				321,
				Parameter.Taskmanager.tablePid,
				new uint[]
				{
					Parameter.Taskmanager.Idx.taskmanagerprocessnameindex_83,
					Parameter.Taskmanager.Idx.taskmanagerprocessrunpath_97,
					Parameter.Taskmanager.Idx.taskmanagerprocesscpu_84,
					Parameter.Taskmanager.Idx.taskmanagerprocessmemusage_86,
					Parameter.Taskmanager.Idx.taskmanagerrunparameters_99
				});

			var oaTaskNames = (object[])oaTaskManagerColumns[0];
			var oaTaskRunPaths = (object[])oaTaskManagerColumns[1];
			var oaTaskCPU = (object[])oaTaskManagerColumns[2];
			var oaTaskMemUsage = (object[])oaTaskManagerColumns[3];
			var oaTaskRunArguments = (object[])oaTaskManagerColumns[4];

			var oaProcessCounterColumns = (object[])protocol.NotifyProtocol(
				321,
				Parameter.Processcounter.tablePid,
				new uint[]
				{
					Parameter.Processcounter.Idx.processcounterindex_71,
					Parameter.Processcounter.Idx.processcounterdisplaykey_69,
					Parameter.Processcounter.Idx.processcounterrunpath_73,
					Parameter.Processcounter.Idx.processcounterrunarguments_63
				});
			var oaProcessIndex = (object[])oaProcessCounterColumns[0];
			var oaProcessNames = (object[])oaProcessCounterColumns[1];
			var oaProcessRunPaths = (object[])oaProcessCounterColumns[2];
			var oaProcessRunArguments = (object[])oaProcessCounterColumns[3];

			var oaCountResult = new object[oaProcessIndex.Length];
			var oaProcessCPUResult = new object[oaProcessIndex.Length];
			var oaProcessMemoryResult3 = new object[oaProcessIndex.Length];
			var oaProcessIdentifier = new object[oaProcessIndex.Length];


			if (oaTaskManagerColumns == null && oaProcessNames == null)
			{
				return;
			}

			NormalizeTaskManager(protocol, oaTaskManagerColumns);

			for (int i = 0; i < oaProcessNames.Length; i++)
			{
				string processName = Convert.ToString(oaProcessNames[i]);
				string processRunPath = Convert.ToString(oaProcessRunPaths[i]);
				string processRunArgument = Convert.ToString(oaProcessRunArguments[i]);

				if (string.IsNullOrEmpty(processName))
					return;

				int countMe = 0;
				double CPUSum = 0;
				int memSum = 0;
				for (int j = 0; j < oaTaskNames.Length; j++)
				{

					string sProcessRunPath = Convert.ToString(oaTaskRunPaths[j]);
					string sProcessRunArgument = Convert.ToString(oaTaskRunArguments[j]);
					string sTaskName = Convert.ToString(oaTaskNames[j]);
					double CPULoad = Convert.ToDouble(oaTaskCPU[j]);
					int MemLoad = Convert.ToInt32(oaTaskMemUsage[j]);

					bool runPathMatch = Regex.IsMatch(sProcessRunPath, WildCardToRegular(processRunPath));
					bool processNameMatch = Regex.IsMatch(sTaskName, WildCardToRegular(processName));
					bool argumentsPathMatch = Regex.IsMatch(sProcessRunArgument, WildCardToRegular(processRunArgument));

					bool includeInCalculation = false;

					if (runPathMatch && processNameMatch && argumentsPathMatch)
					{
						includeInCalculation = true;
					}

					if (includeInCalculation)
					{
						countMe++;
						CPUSum += CPULoad;
						memSum += MemLoad;
					}
				}
				oaCountResult[i] = countMe;
				oaProcessCPUResult[i] = CPUSum;
				oaProcessMemoryResult3[i] = memSum;
				oaProcessIdentifier[i] = oaProcessNames[i] + ":" + oaProcessRunPaths[i];
			}

			if (oaProcessIndex.Length > 0)
			{
				protocol.NotifyProtocol(
					220,
					new object[]
					{
						Parameter.Processcounter.tablePid,
						Parameter.Processcounter.Pid.processcounterinstancecount_72,
						Parameter.Processcounter.Pid.processcounterprocesscpu_64,
						Parameter.Processcounter.Pid.processcounterprocessmemusage_66,
						Parameter.Processcounter.Pid.processcounterprocessidentifier_67,
						false
					},
					new object[] { oaProcessIndex, oaCountResult, oaProcessCPUResult, oaProcessMemoryResult3, oaProcessIdentifier });
			}
		}
		catch (Exception ex)
		{
			protocol.Log("QA" + protocol.QActionID + "|UpdateTaskManagerTable_AfterGroup: " + ex, LogType.Error, LogLevel.NoLogging);
		}
	}

	private static void UpdateStorageTable_AfterGroup(SLProtocolExt protocol)
	{
		var oaStorageNames = (object[])((object[])protocol.NotifyProtocol(
			321,
			Parameter.Storagetable.tablePid,
			new uint[] { Parameter.Storagetable.Idx.storagedescription_406 }))[0];

		var oaStorageAvailabilityColumns = (object[])protocol.NotifyProtocol(
			321,
			Parameter.Storageavailability.tablePid,
			new uint[] { Parameter.Storageavailability.Idx.mountnamemountstorage_501, Parameter.Storageavailability.Idx.descriptionmountstorage_505 });
		var oaStorageMountNames = (object[])oaStorageAvailabilityColumns[0];
		var oaStorageMountDescriptions = (object[])oaStorageAvailabilityColumns[1];

		if (oaStorageNames == null && oaStorageAvailabilityColumns == null)
		{
			return;
		}

		var paths = new List<string>();
		foreach (var path in oaStorageNames)
		{
			paths.Add(Convert.ToString(path));
		}

		var StorageRow = new List<QActionTableRow>();
		for (int i = 0; i < oaStorageMountNames.Length; i++)
		{
			var mountName = Convert.ToString(oaStorageMountNames[i]);
			var description = Convert.ToString(oaStorageMountDescriptions[i]);
			if (paths.Contains(mountName))
			{
				StorageRow.Add(new StorageavailabilityQActionRow
				{
					Mountnamemountstorage = mountName,
					Availabilitymountstorage = Convert.ToInt32(true),
					Descriptionmountstorage = description,
				});
			}
			else
			{
				StorageRow.Add(new StorageavailabilityQActionRow
				{
					Mountnamemountstorage = mountName,
					Availabilitymountstorage = Convert.ToInt32(false),
					Descriptionmountstorage = description,
				});
			}
		}
		protocol.storageavailability.FillArray(StorageRow);
	}

	public static void StorageState(SLProtocolExt protocol, string[] values)
	{
		var command = values[1];
		var storageName = values[2];

		switch (command)
		{
			case "add":
				var state = false; // Not present by default
				var counter = 0;
				var description = values[3];
				object[] mountNames = (object[])((object[])protocol.NotifyProtocol(
					321,
					Parameter.Storagetable.tablePid,
					new uint[] { Parameter.Storagetable.Idx.storagedescription_406 }))[0];

				var myStorageInstance = CheckForInstance(storageName);
				bool storageInstance = myStorageInstance.state;
				storageName = myStorageInstance.value;
				var myMountTable = new List<QActionTableRow>();
				foreach (var item in mountNames)
				{
					string name = Convert.ToString(item);
					if (!string.IsNullOrEmpty(name))
					{
						if (storageInstance && storageName.Length <= name.Length && name.Substring(0, storageName.Length).Equals(storageName))
						{
							myMountTable.Add(new StorageavailabilityQActionRow
							{
								Mountnamemountstorage = name,
								Availabilitymountstorage = 1,
								Descriptionmountstorage = description,
							});
						}
						else if (name.Equals(storageName))
						{
							if (!state)
							{
								state = !state; // item present
								break;
							}
						}
						else
						{
							// do nothing
						}
					}

					counter++;
				}

				if (storageInstance)
				{
					protocol.storageavailability.FillArrayNoDelete(myMountTable);
				}
				else
				{
					var storageRow = new StorageavailabilityQActionRow
					{
						Mountnamemountstorage_501 = storageName,
						Availabilitymountstorage_502 = Convert.ToInt32(state),
						Descriptionmountstorage_505 = description,
					};
					protocol.storageavailability.SetRow(storageRow, true);
				}
				break;

			case "remove":
				{
					protocol.storageavailability.DeleteRow(storageName);
					break;
				}
		}
	}

	public static void AvailabilityState(SLProtocolExt protocol, string command, string mountName)
	{
		switch (command)
		{
			case "add":
				bool state = false; // Not present by default
				int counter = 0;
				var mountNames = (object[])((object[])protocol.NotifyProtocol(
					321,
					Parameter.Linuxmonitoreddisks.tablePid,
					new uint[] { Parameter.Linuxmonitoreddisks.Idx.linuxmonitoreddisksmountpath_1057 }))[0];

				var myMonitoredDisks = CheckForInstance(mountName);
				bool monitoredInstance = myMonitoredDisks.state;
				var myMountAvailabilityRow = new List<QActionTableRow>();
				mountName = myMonitoredDisks.value;

				foreach (var item in mountNames)
				{
					var name = Convert.ToString(item);
					if (!string.IsNullOrEmpty(name))
					{
						if (monitoredInstance && mountName.Length <= name.Length && name.Substring(0, mountName.Length).Equals(mountName))
						{
							myMountAvailabilityRow.Add(
							new MountavailabilityQActionRow
							{
								Mountavailabilitymountname_1041 = name,
								Mountavailabilityavailability_1042 = 1,
							});
						}
						else if (name.Equals(mountName))
						{
							if (!state)
							{
								state = !state; // item present
								break;
							}
						}
						else
						{
							// do nothing
						}
					}

					counter++;
				}

				if (monitoredInstance)
				{
					protocol.mountavailability.FillArrayNoDelete(myMountAvailabilityRow);
				}
				else
				{
					var availabilityRow = new MountavailabilityQActionRow
					{
						Mountavailabilityavailability_1042 = Convert.ToInt32(state),
						Mountavailabilitymountname_1041 = mountName,
					};
					protocol.mountavailability.SetRow(availabilityRow, true);
				}

				break;
			case "remove":
				protocol.mountavailability.DeleteRow(mountName);
				break;
		}
	}

	public static void ProcessCounter(SLProtocolExt protocol, string[] values)
	{
		string command = values[1];
		string processName = values[2];

		switch (command)
		{
			case "add":
				string runPath = values[3];
				string runArgument = values[4];
				object[] oaProcessCounterColumns = ((object[])protocol.NotifyProtocol(
					321,
					Parameter.Processcounter.tablePid,
					new uint[]
					{
						Parameter.Processcounter.Idx.processcounterdisplaykey_69,
						Parameter.Processcounter.Idx.processcounterrunpath_73 ,
						Parameter.Processcounter.Idx.processcounterrunarguments_63
					}));
				object[] ProcessNames = (object[])oaProcessCounterColumns[0];
				object[] ProcessRunPaths = (object[])oaProcessCounterColumns[1];
				object[] ProcessRunArguments = (object[])oaProcessCounterColumns[2];

				bool doubleDetected = false;

				if (string.IsNullOrWhiteSpace(processName) || string.IsNullOrWhiteSpace(runPath))
				{
					protocol.ShowInformationMessage("The Process Name or Process Run Path was empty or a space. The add will not be executed.");
					break;
				}

				// Check if combinations of run path, run argument and process name already exists
				for (int j = 0; !doubleDetected && j < ProcessNames.Length; j++)
				{
					if (Convert.ToString(ProcessNames[j]).Equals(processName) && Convert.ToString(ProcessRunPaths[j]).Equals(runPath) && Convert.ToString(ProcessRunArguments[j]).Equals(runArgument))
					{
						doubleDetected = true;
					}
				}

				if (!doubleDetected)
				{
					object[] oaTaskManagerColumns = (object[])protocol.NotifyProtocol(
						321,
						Parameter.Taskmanager.tablePid,
						new uint[]
						{
							Parameter.Taskmanager.Idx.taskmanagerprocessnameindex_83,
							Parameter.Taskmanager.Idx.taskmanagerprocessrunpath_97,
							Parameter.Taskmanager.Idx.taskmanagerprocesscpu_84,
							Parameter.Taskmanager.Idx.taskmanagerprocessmemusage_86,
							Parameter.Taskmanager.Idx.taskmanagerrunparameters_99});

					object[] oaProcessNames = (object[])oaTaskManagerColumns[0];
					object[] oaProcessRunPath = (object[])oaTaskManagerColumns[1];
					object[] oaProcessCPU = (object[])oaTaskManagerColumns[2];
					object[] oaProcessMemoryUsage = (object[])oaTaskManagerColumns[3];
					object[] oaProcessRunArgument = (object[])oaTaskManagerColumns[4];

					if (oaTaskManagerColumns != null && oaTaskManagerColumns.Length > 2)
					{
						int counter = 0;

						bool runpathWildcard = false;
						bool runArgumentWildcard = false;
						if (runPath.Equals("*"))
						{
							runpathWildcard = true;
						}

						if (runArgument.Equals("*"))
						{
							runArgumentWildcard = true;
						}

						string newRunPath = runPath;
						string newProcessname = processName;
						string newRunArgument = runArgument;

						int MemSum = 0;
						double CPUSum = 0;

						for (int i = 0; i < oaProcessNames.Length; i++)
						{
							string sProcessName = Convert.ToString(oaProcessNames[i]);
							string sProcessRunPath = Convert.ToString(oaProcessRunPath[i]);
							string sProcessRunArgument = Convert.ToString(oaProcessRunArgument[i]);

							double CPULoad = Convert.ToDouble(oaProcessCPU[i]);
							int MemLoad = Convert.ToInt32(oaProcessMemoryUsage[i]);

							bool runPathMatch = Regex.IsMatch(sProcessRunPath, WildCardToRegular(newRunPath));
							bool processNameMatch = Regex.IsMatch(sProcessName, WildCardToRegular(newProcessname));
							bool runArgumentMatch = Regex.IsMatch(sProcessRunArgument, WildCardToRegular(newRunArgument));

							if (!string.IsNullOrEmpty(sProcessName) && !string.IsNullOrEmpty(sProcessRunPath))
							{

								bool includeInCalculation = false;

								if (runPathMatch && processNameMatch && runArgumentMatch)
								{
									includeInCalculation = true;
								}

								if (includeInCalculation)
								{
									counter++;
									MemSum += MemLoad;
									CPUSum += CPULoad;
								}
							}
						}

						if (newRunPath.EndsWith("*"))
						{
							runPath = newRunPath;
						}

						if (newRunArgument.EndsWith("*"))
						{
							runArgument = newRunArgument;
						}

						if (runpathWildcard)
						{
							runPath = "*";
						}

						if (runArgumentWildcard)
						{
							runArgument = "*";
						}

						var tableRow = new ProcesscounterQActionRow
						{
							Processcounterdisplaykey_69 = processName,
							Processcounterinstancecount_72 = counter,
							Processcounterrunpath_73 = runPath,
							Processcounterrunarguments_63 = runArgument,
							Processcounterprocesscpu_64 = CPUSum,
							Processcounterprocessmemusage_66 = MemSum,
							Processcounterprocessidentifier_67 = processName + ":" + runPath,
						};
						protocol.processcounter.AddRow(tableRow);
					}
				}
				else
				{
					protocol.ShowInformationMessage("The combinations of Run Path, Run Argument and Process Name already exists. The add will not be executed.");
				}

				break;
			case "remove":
				protocol.processcounter.DeleteRow(processName);
				break;
		}
	}

	public static void NormalizeTaskManager(SLProtocolExt protocol, object[] taskManagerTable)
	{
		object[] oNormalizeTaskManagerTableKeys = (object[])((object[])protocol.NotifyProtocol(
			321,
			Parameter.Normalizetaskmanager.tablePid,
			new uint[] { Parameter.Normalizetaskmanager.Idx.normalizetaskmanagerprocessname_111 }))[0];
		object[] oTaskManagerTableKeys = taskManagerTable;

		object[] oToDeleteKeys = oNormalizeTaskManagerTableKeys.Except(oTaskManagerTableKeys).ToArray();
		protocol.DeleteRow(Parameter.Normalizetaskmanager.tablePid, Array.ConvertAll(oToDeleteKeys, x => Convert.ToString(x)));
	}

	private static Instance CheckForInstance(string processName)
	{
		bool instance = false;
		if (processName.Length > 0)
		{
			if (processName.LastIndexOf('*') == (processName.Length - 1))
			{
				// looks for the body.
				processName = processName.Substring(0, processName.Length - 1);
				instance = !instance;
			}
		}

		return new Instance() { value = processName, state = instance };
	}

	class Instance
	{
		public string value
		{
			get; set;
		}

		public bool state
		{
			get; set;
		}

		public string vprocessPath
		{
			get; set;
		}

		public bool rPInstance
		{
			get; set;
		}
	}

	private static String WildCardToRegular(String value)
	{
		return "^" + Regex.Escape(value).Replace("\\*", ".*") + "$";
	}
}