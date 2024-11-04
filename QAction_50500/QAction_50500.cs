
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Skyline.DataMiner.Scripting;
using System.Linq;

public class QAction
{
	/// <summary>
	/// fill table with exception values
	/// </summary>
	/// <param name="protocol">Link with Skyline DataMiner</param>
	public static void Run(SLProtocol protocol)
	{
		try
		{
			int iTriggered = protocol.GetTriggerParameter();

			object[] oKeys = (object[])((object[])protocol.NotifyProtocol(321, iTriggered, new UInt32[] { 0 }))[0];
			List<int> lColumnIndex = new List<int>();

			if (oKeys == null)
			{
				return;
			}

			object[] oRow = (object[])protocol.GetRow(iTriggered, Convert.ToString(oKeys[0]));

			if (oRow == null)
			{
				return;
			}

			for (int j = 0; j < oRow.Length - 1; j++)
			{
				if (Convert.ToString(oRow[j]) == "")
					lColumnIndex.Add(j);
			}

			foreach (int index in lColumnIndex)
			{
				object[] oArrayOfMinusOne = new object[oKeys.Length];
				oArrayOfMinusOne = oArrayOfMinusOne.Select(x => (object)-1).ToArray();

				switch (index)
				{
					case 8:
					case 9:
						protocol.NotifyProtocol(220, new object[] { iTriggered, iTriggered + index + 2 }, new object[] { oKeys, oArrayOfMinusOne });
						break;
					case 10:
						protocol.NotifyProtocol(220, new object[] { iTriggered, iTriggered + index + 3 }, new object[] { oKeys, oArrayOfMinusOne });
						break;
					default:
						protocol.NotifyProtocol(220, new object[] { iTriggered, iTriggered + index + 1 }, new object[] { oKeys, oArrayOfMinusOne });
						break;
				}

			}
		}
		catch (Exception e)
		{
			//   protocol.Log(string.Format("QA{0}: Exception:{1}", protocol.QActionID, e), LogType.Error, LogLevel.NoLogging);
			protocol.Log("QA" + protocol.QActionID + "|Add External Data |Error/Message: " + e, LogType.Error, LogLevel.NoLogging);

		}
	}
}
