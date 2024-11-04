using System;
using System.Collections.Generic;
using System.Globalization;
using Skyline.DataMiner.Net;
using Skyline.DataMiner.Scripting;

public class QAction
{
	public static void Run(SLProtocolExt protocol, object inputTable)
	{
		try
		{
			if (inputTable == null)
			{
				protocol.Log(string.Format("QA{0}|Run|Interface table is null", protocol.QActionID), LogType.Error, LogLevel.NoLogging);
				return;
			}

			IF intfConfigTable = new IF((object[])inputTable);

			int iRowCount = intfConfigTable.iRowCount;

			if (iRowCount == 0)
			{
				return;
			}

			object[] ip_Addr = (object[])protocol.NotifyProtocol(
				321,
				Parameter.Ipaddrtable.tablePid,
				new[] { Parameter.Ipaddrtable.Idx.ipaddrentaddr_151, Parameter.Ipaddrtable.Idx.ipaddrentifindex_152, Parameter.Ipaddrtable.Idx.ipaddrentnetmask_153 });

			object[] ip_Route = (object[])protocol.NotifyProtocol(
				321,
				Parameter.Iproutetable.tablePid,
				new[] { Parameter.Iproutetable.Idx.iproutedest_161, Parameter.Iproutetable.Idx.iprouteifindex_162, Parameter.Iproutetable.Idx.iproutenexthop_163 });

			object[] ifxColumn = (object[])protocol.NotifyProtocol(
				321,
				Parameter.Ifxtable.tablePid,
				new[] { Parameter.Ifxtable.Idx.ifinstance_901, Parameter.Ifxtable.Idx.ifhighspeed_916 });

			string[] ifxInstance = Array.ConvertAll((object[])ifxColumn[0], x => Convert.ToString(x));
			object[] ifxHighSpeed = (object[])ifxColumn[1];

			InterfaceInfo interfaceTable = new InterfaceInfo(protocol);

			for (int i = 0; i < iRowCount; i++)
			{
				if (Convert.ToInt32(intfConfigTable.configColumn[i]) != 1)
					continue;
				string rowK = Convert.ToString(intfConfigTable.oCol_Index[i]);

				Int32 iBW = GetBandwidthValue(Convert.ToDouble(intfConfigTable.oCol_IF_BW[i]), ifxInstance, ifxHighSpeed, rowK);

				int iCounterType = 0;

				int intfIdx = Array.IndexOf(interfaceTable.Instance, rowK);
				ProcessInterfaceData(intfConfigTable, interfaceTable, i, ref iBW, ref iCounterType, rowK, intfIdx);

				intfConfigTable.oBandwidth[i] = Convert.ToInt64(iBW) * 1000000;

				ProcessRateCalculation(intfConfigTable, interfaceTable, i, rowK, iBW, iCounterType, intfIdx);

				// Routing
				intfConfigTable.Ip_Route(i, rowK, ip_Addr, ip_Route);
			}

			UpdateInterfaceTable(protocol, intfConfigTable);

		}
		catch (Exception e)
		{
			protocol.Log(string.Format("QA{0}: Exception:{1}", protocol.QActionID, e), LogType.Error, LogLevel.NoLogging);
		}
	}

	private static void ProcessInterfaceData(IF intfConfigTable, InterfaceInfo interfaceTable, int i, ref int iBW, ref int iCounterType, string rowK, int intfIdx)
	{
		string sDescr = GetDescription(intfConfigTable, i, rowK);

		int iState = GetState(intfConfigTable, i);
		if (intfIdx != -1)
		{
			iCounterType = Convert.ToInt32(interfaceTable.CounterType[intfIdx]);

			if (iBW == 0)
			{
				iBW = Convert.ToInt32(interfaceTable.Bandwidth[intfIdx]);
			}

			string existDesc = Convert.ToString(interfaceTable.DisplayKey[intfIdx]);
			intfConfigTable.oDisplayKey[i] = existDesc != sDescr ? sDescr : existDesc;

			int existStatus = Convert.ToInt32(interfaceTable.Status[intfIdx]);
			intfConfigTable.oState[i] = existStatus != iState ? iState : existStatus;
		}
		else
		{
			intfConfigTable.oDisplayKey[i] = sDescr;
			intfConfigTable.oState[i] = iState;
		}
	}

	private static void UpdateInterfaceTable(SLProtocolExt protocol, IF intfConfigTable)
	{
		List<QActionTableRow> myInterfaceRows = new List<QActionTableRow>();
		int or = 0;
		foreach (var indixx in intfConfigTable.oCol_Descr)
		{
			if (Convert.ToInt32(intfConfigTable.configColumn[or]) == 1)
			{
				myInterfaceRows.Add(new InterfacestableQActionRow
				{
					Interfacesifindex_605 = intfConfigTable.oCol_Index[or],
					Interfacesifdescr_606 = intfConfigTable.oCol_Descr[or],
					Interfacesiftype_607 = intfConfigTable.oCol_IF_Type[or],
					Interfacesphysaddress_608 = intfConfigTable.oCol_IF_MAC[or],
					Interfacesadminstatus_609 = intfConfigTable.oCol_Admin[or],
					Interfacesoperstatus_610 = intfConfigTable.oCol_Oper[or],
					Interfacesinterfacebandwidth_615 = intfConfigTable.oBandwidth[or],
					Interfacesifinoctets_620 = intfConfigTable.oInOct[or],
					Interfacesifoutoctets_621 = intfConfigTable.oOutOct[or],
					Interfacesifinucastpkts_626 = intfConfigTable.oCol_RxUPack[or],
					Interfacesifoutucastpkts_627 = intfConfigTable.oCol_TxUPack[or],
					Interfacesinterface_idx__604 = intfConfigTable.oDisplayKey[or],
					Interfacesinterfacestatus_614 = intfConfigTable.oState[or],
					Interfacesinrate_611 = intfConfigTable.oInRate[or],
					Interfacesoutrate_612 = intfConfigTable.oOutRate[or],
					Interfacestotalrate_613 = intfConfigTable.oTotalRate[or],
					Interfacesrxutilization_616 = intfConfigTable.oRxDataRate[or],
					Interfacestxutilization_617 = intfConfigTable.oTxDataRate[or],
					Interfacesifinoctetstime_622 = intfConfigTable.oInOct_Time[or],
					Interfacesifoutoctetstime_623 = intfConfigTable.oOutOct_Time[or],
					Interfacesipaddress_642 = intfConfigTable.oIpaddr[or],
					Interfacesipsubnet_643 = intfConfigTable.oSubnet[or],
					Interfacesdefaultipgateway_644 = intfConfigTable.oDefaultIpGateway[or],
					Interfacestotalutilization_645 = intfConfigTable.oTotalDataRate[or],
					Interfacesifcountertype_646 = intfConfigTable.oCounterType[or],
				});
			}

			or++;
		}

		if (myInterfaceRows.Count > 0)
			protocol.interfacestable.FillArray(myInterfaceRows.ToArray());
		else
			protocol.CheckTrigger(601);
	}

	private static void ProcessRateCalculation(IF intfConfigTable, InterfaceInfo interfaceTable, int i, string rowK, int iBW, int iCounterType, int intfIdx)
	{
		bool b64bit = false;
		int intfXIdx = Array.IndexOf(interfaceTable.IntfXInstance, rowK);
		if (intfXIdx != -1)
		{
			string sIfX_InOct = Convert.ToString(interfaceTable.InXOctets[intfXIdx]);
			if (!string.IsNullOrEmpty(sIfX_InOct))
			{
				b64bit = true;
			}
		}

		if (b64bit || iCounterType == 2)
		{
			intfConfigTable.Calc_64bit(i, interfaceTable, iCounterType, intfXIdx, iBW, intfIdx);
		}
		else
		{
			intfConfigTable.Calc_32bit(i, interfaceTable, iBW, intfIdx);
		}
	}

	private static string GetDescription(IF intfTable, int i, string rowK)
	{
		string sDescr = Convert.ToString(intfTable.oCol_Descr[i]);
		if (!string.IsNullOrEmpty(sDescr))
		{
			sDescr += "." + rowK;
		}
		else
		{
			sDescr = rowK;
		}

		return sDescr;
	}

	private static int GetState(IF intfTable, int i)
	{
		int adminState = 0;
		int operState = 0;
		int iState = -1;

		if (intfTable.oCol_Admin[i] != null && intfTable.oCol_Oper != null)
		{
			adminState = Convert.ToInt32(intfTable.oCol_Admin[i]);
			operState = Convert.ToInt32(intfTable.oCol_Oper[i]);
		}

		if (adminState == 0 && operState == 0)
		{
			return iState;
		}

		switch (adminState)
		{
			case 2:
				iState = 3;
				break;
			case 3:
				iState = 4;
				break;
			default:
				iState = GetStateFromOperationState(operState);
				break;
		}

		return iState;
	}

	private static int GetStateFromOperationState(int operState)
	{
		int iState;
		switch (operState)
		{
			case 1:
				iState = 1;
				break;
			case 2:
				iState = 2;
				break;
			case 3:
				iState = 4;
				break;
			default:
				iState = -1;
				break;
		}

		return iState;
	}

	private static Int32 GetBandwidthValue(double intfSpeed, string[] ifxInstance, object[] ifxHighSpeed, string rowKey)
	{
		const double SpeedLimit = 4294.967295;
		int bandwidth = 0;

		if (intfSpeed < SpeedLimit)
		{
			bandwidth = Convert.ToInt32(intfSpeed);
		}

		if (ifxInstance != null)
		{
			int idx = Array.IndexOf(ifxInstance, rowKey);
			if (idx != -1)
			{
				bandwidth = Convert.ToInt32(ifxHighSpeed[idx]);
			}
		}

		return bandwidth;
	}

	public class IF
	{
		public Object[] oaColumns, oCol_Index, oCol_Descr, oCol_Admin, oCol_Oper, oCol_IF_BW, oCol_IF_Type, oCol_IF_MAC, oCol_RxUPack, oCol_TxUPack;
		public Object[] oInRate, oOutRate, oTotalRate, oRxDataRate, oTxDataRate, oTotalDataRate;
		public Object[] oIpaddr, oSubnet, oDefaultIpGateway;
		public Object[] oInOct_Time, oOutOct_Time, oInOct, oOutOct;
		public Object[] oBandwidth, oCounterType;
		public Object[] configColumn;
		public Object[] oDisplayKey;
		public Object[] oState;

		public int iRowCount = 0;

		public IF(object[] inputTable)
		{
			oaColumns = inputTable;
			oCol_Index = (object[])oaColumns[Parameter.Interfacetableconfig.Idx.indexinterfacetableconfig];  // keys
			oCol_Descr = (object[])oaColumns[Parameter.Interfacetableconfig.Idx.descriptioninterfacetableconfig];
			oCol_IF_Type = (object[])oaColumns[Parameter.Interfacetableconfig.Idx.ififtype];
			oCol_IF_BW = (object[])oaColumns[Parameter.Interfacetableconfig.Idx.ifinterfacebandwidth];
			oCol_IF_MAC = (object[])oaColumns[Parameter.Interfacetableconfig.Idx.ifphysaddress];
			oCol_Admin = (object[])oaColumns[Parameter.Interfacetableconfig.Idx.ifadminstatus];
			oCol_Oper = (object[])oaColumns[Parameter.Interfacetableconfig.Idx.operationalstatusinterfacetableconfig];
			oCol_RxUPack = (object[])oaColumns[Parameter.Interfacetableconfig.Idx.ififinucastpkts];
			oCol_TxUPack = (object[])oaColumns[Parameter.Interfacetableconfig.Idx.ififoutucastpkts];
			configColumn = (object[])oaColumns[Parameter.Interfacetableconfig.Idx.monitorinterfacetableconfig_818];

			if (oCol_Index == null)
			{
				return;
			}

			iRowCount = oCol_Index.Length;
			oInRate = new Object[iRowCount];
			oOutRate = new Object[iRowCount];
			oTotalRate = new Object[iRowCount];
			oRxDataRate = new Object[iRowCount];
			oTxDataRate = new Object[iRowCount];
			oTotalDataRate = new Object[iRowCount];

			oInOct_Time = new Object[iRowCount];
			oOutOct_Time = new Object[iRowCount];
			oInOct = new Object[iRowCount];
			oOutOct = new Object[iRowCount];

			oIpaddr = new Object[iRowCount];
			oSubnet = new Object[iRowCount];
			oDefaultIpGateway = new Object[iRowCount];

			oBandwidth = new Object[iRowCount];
			oCounterType = new Object[iRowCount];

			oDisplayKey = new Object[iRowCount];
			oState = new Object[iRowCount];
		}

		public void Calc_32bit(int i, InterfaceInfo interfaceTable, object bw, int idx)
		{
			oCounterType[i] = 1;

			// In data rate
			ProcessInputDataRate(i, interfaceTable, idx);

			// Out data rate
			string sNewTxByte = Convert.ToString(((object[])oaColumns[Parameter.Interfacetableconfig.Idx.ififoutoctets])[i]);
			if (string.IsNullOrEmpty(sNewTxByte))
			{
				return;
			}

			ProcessOutputDataRate(i, interfaceTable, idx, sNewTxByte);

			// Total Rate
			string sRxDataRate = Convert.ToString(oInRate[i]);
			string sTxDataRate = Convert.ToString(oOutRate[i]);

			if (!string.IsNullOrEmpty(sRxDataRate) && !string.IsNullOrEmpty(sTxDataRate))
			{
				double totalRate = Convert.ToDouble(sRxDataRate) + Convert.ToDouble(sTxDataRate); // no set to param with seq:div1000 so no *1000  needed
				if (totalRate >= 0)
				{
					oTotalRate[i] = totalRate;
				}
			}

			CalcPercBW(i, interfaceTable, sRxDataRate, sTxDataRate, bw, idx); // 32
		}

		public void Calc_64bit(int i, InterfaceInfo interfacetable, int iCounterType, int intfXidx, object bw, int idx)
		{
			oCounterType[i] = 2;

			if (intfXidx == -1)
			{
				return;
			}

			ProcessInputDataRate(i, interfacetable, iCounterType, intfXidx, idx);

			// Out data rate
			string sNewTxByte = Convert.ToString(interfacetable.OutXOctets[intfXidx]);

			if (string.IsNullOrEmpty(sNewTxByte))
			{
				return;
			}

			ProcessOutputDataRate(i, interfacetable, iCounterType, intfXidx, idx);

			// Total Rate
			string sRxDataRate = Convert.ToString(oInRate[i]);
			string sTxDataRate = Convert.ToString(oOutRate[i]);

			if (!string.IsNullOrEmpty(sRxDataRate) && !string.IsNullOrEmpty(sTxDataRate))
			{
				decimal dRxDataRate = Convert.ToDecimal(oInRate[i]);
				decimal dTxDataRate = Convert.ToDecimal(oOutRate[i]);

				Int64 totalRate = Convert.ToInt64(dRxDataRate) + Convert.ToInt64(dTxDataRate); // no set to param with seq:div1000 so no *1000  needed
				if (totalRate >= 0)
				{
					oTotalRate[i] = totalRate;
				}
			}

			CalcPercBW(i, interfacetable, sRxDataRate, sTxDataRate, bw, idx); // 64
		}

		public void Ip_Route(int i, string rowK, object[] ip_Addr, object[] ip_RouteCol)
		{
			oIpaddr[i] = "-1";
			oSubnet[i] = "-1";

			GetIpAddressAndSubnet(i, rowK, ip_Addr);

			oDefaultIpGateway[i] = "-1";

			if ((object[])ip_RouteCol[0] == null)
			{
				return;
			}

			for (int k = 0; k < ((object[])ip_RouteCol[0]).Length; k++)
			{
				if (Convert.ToString(((object[])ip_RouteCol[0])[k]).Equals(rowK))
				{
					string mask = Convert.ToString(((object[])ip_RouteCol[2])[k]);
					if (mask == "0.0.0.0")
						oDefaultIpGateway[i] = Convert.ToString(((object[])ip_RouteCol[1])[k]);
				}
			}
		}

		private static double CalcBitrate_32bit(double iOctets, double iOldOctets, Double iTimeDiff)
		{
			double iOctetsNew = iOctets - iOldOctets;

			if (iOctetsNew < 0)
			{
				iOctetsNew = iOctetsNew + 4294967295;
			}

			double iBitrate = iOctetsNew / iTimeDiff; // octets / ms
			return iBitrate;
		}

		private static UInt64 CalcBitrate_64bit(UInt64 iOctets, UInt64 iOldOctets, Double iTimeDiff)
		{
			UInt64 iOctetsNew = iOctets - iOldOctets;

			// WrapAround
			if (iOctetsNew < 0)
			{
				iOctetsNew = iOctetsNew + UInt64.MaxValue;
			}

			UInt64 iBitrate = iOctetsNew / Convert.ToUInt64(iTimeDiff);
			return iBitrate;
		}

		private void ProcessOutputDataRate(int i, InterfaceInfo interfaceTable, int idx, string sNewTxByte)
		{
			DateTime currTime = DateTime.Now;
			double sCurrTime = Convert.ToDouble(currTime.Ticks / 10000);
			double newTxByte = Convert.ToDouble(sNewTxByte);

			if (idx != -1)
			{
				string sPreviousTxTime = Convert.ToString(interfaceTable.TxOctetsTimeFlag[idx]);
				string sPreviousTxByte = Convert.ToString(interfaceTable.TxOctets[idx]);
				if (!string.IsNullOrEmpty(sPreviousTxTime) && !string.IsNullOrEmpty(sPreviousTxByte))
				{
					double previousTxTime = Convert.ToDouble(sPreviousTxTime);
					double previousTxByte = Convert.ToDouble(sPreviousTxByte);

					double dtxBitRate = 8 * CalcBitrate_32bit(newTxByte, previousTxByte, sCurrTime - previousTxTime);

					if (dtxBitRate >= 0)
						oOutRate[i] = dtxBitRate;
				}
			}
			oOutOct_Time[i] = Convert.ToString(sCurrTime);
			oOutOct[i] = newTxByte;
		}

		private void ProcessOutputDataRate(int i, InterfaceInfo interfacetable, int iCounterType, int intfXidx, int idx)
		{
			decimal dNewTxByte = Convert.ToDecimal(interfacetable.OutXOctets[intfXidx]);

			DateTime newCurrTime = DateTime.Now;
			double newSCurrTime = Convert.ToDouble(newCurrTime.Ticks / 10000);  // ms
			UInt64 newTxByte = Convert.ToUInt64(dNewTxByte);
			if (iCounterType == 2 && idx != -1)
			{
				string sPreviousTxTime = Convert.ToString(interfacetable.TxOctetsTimeFlag[idx]);

				string sPreviousTxByte = Convert.ToString(interfacetable.TxOctets[idx]);

				if (!string.IsNullOrEmpty(sPreviousTxTime) && !string.IsNullOrEmpty(sPreviousTxByte))
				{
					decimal dPreviousTxByte = Convert.ToDecimal(interfacetable.TxOctets[idx]);

					double previousTxTime = Convert.ToDouble(sPreviousTxTime);
					UInt64 previousTxByte = Convert.ToUInt64(dPreviousTxByte);

					UInt64 utxBitRate = 8 * CalcBitrate_64bit(newTxByte, previousTxByte, newSCurrTime - previousTxTime);

					if (utxBitRate >= 0)
						oOutRate[i] = utxBitRate;
				}
			}
			oOutOct_Time[i] = Convert.ToString(newSCurrTime);
			oOutOct[i] = newTxByte;
		}

		private void ProcessInputDataRate(int i, InterfaceInfo interfaceTable, int idx)
		{
			string sNewRxByte = Convert.ToString(((object[])oaColumns[Parameter.Interfacetableconfig.Idx.ififinoctets])[i]);
			if (string.IsNullOrEmpty(sNewRxByte))
			{
				return;
			}

			DateTime currTime = DateTime.Now;
			double sCurrTime = Convert.ToDouble(currTime.Ticks / 10000);
			double newRxByte = Convert.ToDouble(sNewRxByte);

			if (idx != -1)
			{
				string sPreviousRxTime = Convert.ToString(interfaceTable.RxOctetsTimeFlag[idx]);
				string sPreviousRxByte = Convert.ToString(interfaceTable.RxOctets[idx]);
				if (!string.IsNullOrEmpty(sPreviousRxTime) && !string.IsNullOrEmpty(sPreviousRxByte))
				{
					double previousRxTime = Convert.ToDouble(sPreviousRxTime);
					double previousRxByte = Convert.ToDouble(sPreviousRxByte);
					double rxBitRate = 8 * CalcBitrate_32bit(newRxByte, previousRxByte, sCurrTime - previousRxTime);
					if (rxBitRate >= 0)
						oInRate[i] = rxBitRate;
				}
			}

			oInOct_Time[i] = Convert.ToString(sCurrTime);
			oInOct[i] = newRxByte;
		}

		private void ProcessInputDataRate(int i, InterfaceInfo interfacetable, int iCounterType, int intfXidx, int idx)
		{
			string sNewRxByte = Convert.ToString(interfacetable.InXOctets[intfXidx]);

			if (string.IsNullOrEmpty(sNewRxByte))
			{
				return;
			}

			decimal dNewRxByte = Convert.ToDecimal(interfacetable.InXOctets[intfXidx]);

			DateTime currTime = DateTime.Now;
			double sCurrTime = Convert.ToDouble(currTime.Ticks / 10000);  // ms
			UInt64 newRxByte = Convert.ToUInt64(dNewRxByte);

			if (iCounterType == 2 && idx != -1)
			{
				string sPreviousRxTime = Convert.ToString(interfacetable.RxOctetsTimeFlag[idx]);

				string sPreviousRxByte = Convert.ToString(interfacetable.RxOctets[idx]);

				if (!string.IsNullOrEmpty(sPreviousRxTime) && !string.IsNullOrEmpty(sPreviousRxByte))
				{
					decimal dPreviousRxByte = Convert.ToDecimal(interfacetable.RxOctets[idx]);

					double previousRxTime = Convert.ToDouble(sPreviousRxTime);
					UInt64 previousRxByte = Convert.ToUInt64(dPreviousRxByte);

					UInt64 rxBitRate = 8 * CalcBitrate_64bit(newRxByte, previousRxByte, sCurrTime - previousRxTime);

					if (rxBitRate >= 0)
						oInRate[i] = rxBitRate;
				}
			}

			oInOct_Time[i] = Convert.ToString(sCurrTime);
			oInOct[i] = newRxByte;
		}

		private void GetIpAddressAndSubnet(int i, string rowK, object[] ip_Addr)
		{
			if ((object[])ip_Addr[1] == null)
			{
				return;
			}

			for (int k = 0; k < ((object[])ip_Addr[1]).Length; k++)
			{
				if (Convert.ToString(((object[])ip_Addr[1])[k]).Equals(rowK))
				{
					string ipAddr = Convert.ToString(((object[])ip_Addr[0])[k]);
					string subnet = Convert.ToString(((object[])ip_Addr[2])[k]);

					oIpaddr[i] = ipAddr;
					oSubnet[i] = subnet;
					break;
				}
			}
		}

		private void CalcPercBW(int i, InterfaceInfo interfaceTable, string sRxDataRate, string sTxDataRate, object bw, int idx)
		{
			string sMaxRxRate = Convert.ToString(bw);

			if (!string.IsNullOrEmpty(sMaxRxRate))
			{
				decimal dMaxRxRate = decimal.Parse(sMaxRxRate, NumberStyles.AllowDecimalPoint);

				Int32 maxRxRate = Convert.ToInt32(dMaxRxRate);  // Mbps
				if (maxRxRate == 0 && idx != -1)
				{
					sMaxRxRate = Convert.ToString(interfaceTable.Bandwidth[idx]);
					dMaxRxRate = decimal.Parse(sMaxRxRate, NumberStyles.AllowDecimalPoint);
					if (!string.IsNullOrEmpty(sMaxRxRate))
					{
						maxRxRate = Convert.ToInt32(dMaxRxRate);
					}
				}

				// In rate percentage
				ProcessRxDataRate(i, sRxDataRate, maxRxRate);

				// Out rate percentage
				ProcessTxDataRate(i, sTxDataRate, maxRxRate);
			}

			// Total rate percentage
			ProcessTotalRate(i);
		}

		private void ProcessTotalRate(int i)
		{
			string sRxRate = Convert.ToString(oRxDataRate[i]);
			string sTxRate = Convert.ToString(oTxDataRate[i]);
			if (!string.IsNullOrEmpty(sRxRate) && !string.IsNullOrEmpty(sTxRate))
			{
				if (Convert.ToDouble(oRxDataRate[i]) != -1 && Convert.ToDouble(oTxDataRate[i]) != -1)
					oTotalDataRate[i] = (double)oRxDataRate[i] + (double)oTxDataRate[i];
				else
					oTotalDataRate[i] = -1;
			}
			else
			{
				oTotalDataRate[i] = -1;
			}
		}

		private void ProcessTxDataRate(int i, string sTxDataRate, int maxRxRate)
		{
			if (!string.IsNullOrEmpty(sTxDataRate))
			{
				double rxRate = Convert.ToDouble(sTxDataRate);  // kbps
				if (maxRxRate != 0)
				{
					double rate = 100 * rxRate / maxRxRate;
					if (rate >= 0 && rate <= 100000)
						oTxDataRate[i] = rate;
					else
						oTxDataRate[i] = -1;
				}
				else
				{
					oTxDataRate[i] = -1;
				}
			}
		}

		private void ProcessRxDataRate(int i, string sRxDataRate, int maxRxRate)
		{
			if (!string.IsNullOrEmpty(sRxDataRate))
			{
				double rxRate = Convert.ToDouble(sRxDataRate);  // kbps
				if (maxRxRate != 0)
				{
					double rate = 100 * rxRate / maxRxRate;
					if (rate >= 0 && rate <= 100000)
						oRxDataRate[i] = rate;
					else
						oRxDataRate[i] = -1;
				}
				else
				{
					oRxDataRate[i] = -1;
				}
			}
		}
	}

	public class InterfaceInfo
	{
		public InterfaceInfo(SLProtocol protocol)
		{
			int[] indexes = new[]
			{
				Parameter.Interfacestable.Idx.interfacesifindex_605,
				Parameter.Interfacestable.Idx.interfacesinterfacebandwidth_615,
				Parameter.Interfacestable.Idx.interfacesinterface_idx__604,
				Parameter.Interfacestable.Idx.interfacesinterfacestatus_614,
				Parameter.Interfacestable.Idx.interfacesifcountertype_646,
				Parameter.Interfacestable.Idx.interfacesifinoctets_620,
				Parameter.Interfacestable.Idx.interfacesifoutoctets_621,
				Parameter.Interfacestable.Idx.interfacesifinoctetstime_622,
				Parameter.Interfacestable.Idx.interfacesifoutoctetstime_623,
			};

			object[] intfColumn = (object[])protocol.NotifyProtocol(321, Parameter.Interfacestable.tablePid, indexes);

			Instance = Array.ConvertAll((object[])intfColumn[0], x => Convert.ToString(x));
			Bandwidth = (object[])intfColumn[1];
			DisplayKey = (object[])intfColumn[2];
			Status = (object[])intfColumn[3];
			CounterType = (object[])intfColumn[4];
			RxOctets = (object[])intfColumn[5];
			TxOctets = (object[])intfColumn[6];
			RxOctetsTimeFlag = (object[])intfColumn[7];
			TxOctetsTimeFlag = (object[])intfColumn[8];

			int[] intfXindexes = new[]
			{
				Parameter.Ifxtable.Idx.ifinstance_901,
				Parameter.Ifxtable.Idx.ifhcinoctets_907,
				Parameter.Ifxtable.Idx.ifhcoutoctets_911,
			};

			object[] intfxColumn = (object[])protocol.NotifyProtocol(321, Parameter.Ifxtable.tablePid, intfXindexes);
			IntfXInstance = Array.ConvertAll((object[])intfxColumn[0], x => Convert.ToString(x));
			InXOctets = (object[])intfxColumn[1];
			OutXOctets = (object[])intfxColumn[2];
		}

		public string[] Instance { get; set; }

		public object[] Bandwidth { get; set; }

		public object[] DisplayKey { get; set; }

		public object[] Status { get; set; }

		public object[] CounterType { get; set; }

		public object[] RxOctets { get; set; }

		public object[] TxOctets { get; set; }

		public object[] RxOctetsTimeFlag { get; set; }

		public object[] TxOctetsTimeFlag { get; set; }

		public string[] IntfXInstance { get; set; }

		public object[] InXOctets { get; set; }

		public object[] OutXOctets { get; set; }
	}
}
