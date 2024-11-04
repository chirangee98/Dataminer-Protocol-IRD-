using System;
using System.Text.RegularExpressions;
using Skyline.DataMiner.Scripting;

public class QAction
{
	// Update real memory usage
	public static void Run(SLProtocol protocol)
	{
		try
		{
			if (!protocol.IsEmpty(Parameter.memtotalreal_1003) && !protocol.IsEmpty(Parameter.memavailreal_1004))
			{
				object[] dValues = (object[])protocol.GetParameters(new UInt32[] { Parameter.memtotalreal_1003, Parameter.memavailreal_1004, Parameter.membuffer_1007, Parameter.memcached_1008, Parameter.memorycalculationchange_40 });
				double totalMemory = Convert.ToDouble(dValues[0]);
				double availMemory = Convert.ToDouble(dValues[1]);
				double availBuffer = Convert.ToDouble(dValues[2]);
				double availCached = Convert.ToDouble(dValues[3]);
				int formula = Convert.ToInt16(dValues[4]);

				double totalAvailPhys = GetAvailablePhysicalMemory(protocol, availMemory, availBuffer, availCached, formula);

				double usageMemory = 0;
				double usedMemory = 0;

				if (totalMemory > 0)
				{
					usageMemory = 1000 * (1 - (totalAvailPhys / totalMemory));
					usedMemory = totalMemory - totalAvailPhys;
					double memUsage = totalAvailPhys - availCached;
					double memUsagePerc = -1;

					if (totalAvailPhys > 0)
						memUsagePerc = (memUsage / totalAvailPhys) * 100;

					protocol.SetParameters(new[] { Parameter.memusagereal_1005, Parameter.memusedreal_1006, Parameter.availablephysicalmemory_1009, Parameter.memoryusage_1011, Parameter.percentagememoryusage_1012 }, new object[] { usageMemory, usedMemory, totalAvailPhys, memUsage, memUsagePerc });
				}
			}
			else
			{
                protocol.SetParameters(new[] { Parameter.memusagereal_1005, Parameter.memusedreal_1006, Parameter.availablephysicalmemory_1009, Parameter.memoryusage_1011, Parameter.percentagememoryusage_1012 }, new object[] { -1, -1, -1, -1, -1 });
			}
		}
		catch (Exception e)
		{
			protocol.Log("QA" + protocol.QActionID + "|Set Real Memory Usage|Error/Message: " + e, LogType.Error, LogLevel.NoLogging);
		}
	}

	public static double GetAvailablePhysicalMemory(SLProtocol protocol, double availMemory, double availBuffer, double availCached, int formula)
	{
		double totalAvailPhys;
		bool bUseNewFolmula = false;
		if (formula == 2)
		{
			bUseNewFolmula = AuotoCheckFormula(protocol);
		}

		if (formula == 1 || bUseNewFolmula)
		{
			totalAvailPhys = availMemory; // -> between Red Hat EL net-snmp releases 5.7.2-43 and 5.7.2-46 a new Formula (totalAvailableMem = memAvailReal) was used
		}
		else
		{
			totalAvailPhys = availMemory + availBuffer + availCached; // totalAvailableMem = memAvailReal + memBuffer + memCached
		}

		return totalAvailPhys;
	}

	public static bool AuotoCheckFormula(SLProtocol protocol)
	{
		object[] oSWName = (object[])((object[])protocol.NotifyProtocol(321, Parameter.Softwareinfo.tablePid, new[] { 1 }))[0];
		bool bRhelVer77Above = CheckKernelVersion(protocol);
		bool bNetSnmVerNew = false;
		bool bUseNewFolmula;

		for (int i = 0; i < oSWName.Length; i++)
		{
			string sSoftWName = Convert.ToString(oSWName[i]);
			string sCheckString = "net-snmp-5";
			int iIndex = sSoftWName.IndexOf(sCheckString);

			if (iIndex != -1)
			{
				bNetSnmVerNew = CheckNetSnmpVersion(sSoftWName, sCheckString);
				break;
			}
		}

		if (bRhelVer77Above && bNetSnmVerNew)
		{
			bUseNewFolmula = true; // for RHEL 7.7+ versions totalAvailableMem = memAvailReal
		}
		else
		{
			bUseNewFolmula = false; // for RHEL versions below 7.7 totalAvailableMem = memAvailReal + memBuffer + memCached
		}

		return bUseNewFolmula;
	}

	public static bool CheckKernelVersion(SLProtocol protocol)
	{
		string sDescription = Convert.ToString(protocol.GetParameter(Parameter.sysdescr_100));
		string pattern = @"(\d+\.)(\d+\.)(\d+\-)(\d+)"; // RHEL Kernel version format: A.B.C-D
		bool bRhelVer77Above = false;
		Match kernelVersion = Regex.Match(sDescription, pattern);
		if (kernelVersion.Success)
		{
			string[] sKVersions = kernelVersion.Value.Split('-');
			Version kernelVersion1;
			if (Version.TryParse(sKVersions[0], out kernelVersion1))
			{
				// RHEL 7.7 Kernel version: 3.10.0-1062
				int iCompValue = kernelVersion1.CompareTo(new Version("3.10.0"));
				int iVerValue;
				int.TryParse(sKVersions[1], out iVerValue);

				if (iCompValue > 0 || (iCompValue == 0 && iVerValue >= 1062))
				{
					bRhelVer77Above = true;
				}
			}
		}
		else
		{
			protocol.Log("QA" + protocol.QActionID + "|Run|Kernel version {in A.B.C-D format} not found from description: " + sDescription, LogType.DebugInfo, LogLevel.NoLogging);
		}

		return bRhelVer77Above;
	}

	private static bool CheckNetSnmpVersion(string sSoftWName, string sCheckString)
	{
		bool bNetSnmVerNew = false;
		string[] sSnmpVer = sSoftWName.Substring(sCheckString.Length - 1).Split('-');
		Version snmpVerPart1;

		if (Version.TryParse(sSnmpVer[0], out snmpVerPart1))
		{
			int iCompValue = snmpVerPart1.CompareTo(new Version("5.7.2"));

			string sPattern = @"(\d+)";
			int iSnmpVerPart2 = 0;
			Match snmpVer2 = Regex.Match(sSnmpVer[1], sPattern);
			if (snmpVer2.Success)
			{
				iSnmpVerPart2 = Convert.ToInt32(snmpVer2.Value);
			}

			if (iCompValue == 0 && iSnmpVerPart2 >= 43 && iSnmpVerPart2 <= 46)
			{
				bNetSnmVerNew = true;
			}
		}

		return bNetSnmVerNew;
	}
}