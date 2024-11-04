using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Skyline.DataMiner.Scripting;

/// <summary>
/// DataMiner QAction Class: Fill HP Polling IP.
/// </summary>
public class QAction
{
	public enum PollingIPStatus
	{
		Disabled,
		Enabled
	}

	/// <summary>
	/// The QAction entry point.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
	public static void Run(SLProtocol protocol)
	{
		try
		{
			int iTriggerPID = protocol.GetTriggerParameter();

			switch (iTriggerPID)
			{
				case Parameter.Write.hppollingip_50250:
					int iHPPollingIpStatus = Convert.ToInt32(protocol.GetParameter(Parameter.hppollingipstatus_50160));
					if (iHPPollingIpStatus == (int)PollingIPStatus.Enabled)
					{
						string sHPPollingIP = Convert.ToString(protocol.GetParameter(Parameter.Write.hppollingip_50250 ));

						protocol.SetParameter(Parameter.hppollingip_50150, sHPPollingIP);
						protocol.CheckTrigger(50150);
					}
					else if(iHPPollingIpStatus == (int)PollingIPStatus.Disabled)
					{
						protocol.ShowInformationMessage("'HP Polling IP Status' is Disabled and the IP can't be set to the new HP IP. To set it to the new HP IP, first Enable HP Polling IP Status.");
					}

					break;
				case Parameter.Write.hppollingipstatus_50260:
					int iWriteHPPollingIpStatus = Convert.ToInt32(protocol.GetParameter(Parameter.Write.hppollingipstatus_50260));
					if (iWriteHPPollingIpStatus == (int)PollingIPStatus.Disabled)
					{
						string sPollingIP = Convert.ToString(protocol.GetParameter(40000));

						protocol.SetParameter(Parameter.hppollingip_50150, sPollingIP);
						protocol.ShowInformationMessage("'HP Polling IP' is set to the default Element IP.");
					}

					break;
				default:
					break;
			}
		}
		catch (Exception ex)
		{
			protocol.Log("QA" + protocol.QActionID + "|" + protocol.GetTriggerParameter() + "|Run|Exception thrown:" + Environment.NewLine + ex, LogType.Error, LogLevel.NoLogging);
		}
	}
}