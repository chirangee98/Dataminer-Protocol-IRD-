using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Skyline.DataMiner.Scripting;

public class QAction
{
	/// <summary>
	/// IF Trap Handling
	/// </summary>
	/// <param name="protocol">Link with Skyline DataMiner</param>
	/// <param name="trapInfo">The Trap information.</param>
	public static void Run(SLProtocol protocol, Object trapInfo)
	{
		if (trapInfo == null)
		{
			return;
		}

		object[] oaBindings = (object[])trapInfo;

		if (oaBindings.Length < 1)
		{
			return;
		}

		object[] oGeneralTrapInfo = (object[])oaBindings[0];

		if (oGeneralTrapInfo.Length == 3)
		{
			string sTrapOID = Convert.ToString(oGeneralTrapInfo[0]);

			switch (sTrapOID)
			{
				//bindings: ifIndex, ifAdminStatus, ifOperStatus
				case "1.3.6.1.6.3.1.1.5.3": //linkDown
				case "1.3.6.1.6.3.1.1.5.4": //linkUp
					{
						string sKey = Convert.ToString(((object[])oaBindings[1])[1]);

						if (protocol.Exists(600, sKey))
						{
							protocol.SetParameterIndexByKey(600, sKey, 5, Convert.ToInt32(((object[])oaBindings[2])[1]));
							protocol.SetParameterIndexByKey(600, sKey, 6, Convert.ToInt32(((object[])oaBindings[3])[1]));
						}
						else
						{
							protocol.Log(String.Format("QA{0}: Index Key: {1} does not exists in Interfaces Table.", protocol.QActionID, sKey), LogType.Error, LogLevel.NoLogging);
						}

						break;
					}
				default:
					// Do nothing
					break;
			}
		}
		else
		{
			protocol.Log(String.Format("QA{0}: (CHECK generalTrapInfo) {1}", protocol.QActionID, "Retrieved invalid general trap information"), LogType.Error, LogLevel.NoLogging);
		}
	}
}
