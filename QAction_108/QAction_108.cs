using System;
using System.Globalization;
using Skyline.DataMiner.Scripting;

/// <summary>
/// DataMiner QAction Class.
/// </summary>
public class QAction
{
	private const int notAvailable = -1;

	private enum Triggers
	{
		LocalTimeSNMP = 108,
		RomDateSNMP = 50951
	}

	/// <summary>
	/// The QAction entry point.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
	public static void Run(SLProtocol protocol)
	{
        var trigger = protocol.GetTriggerParameter();
        var paramValue = Convert.ToString(protocol.GetParameter(trigger));

        try
        {
            if (string.IsNullOrWhiteSpace(paramValue))
            {
				return;
			}

            switch (trigger)
            {
                case (int)Triggers.LocalTimeSNMP:
					{
						SetLocalTimeSnmp(protocol, paramValue);
						break;
					}

				case (int)Triggers.RomDateSNMP:
					{
						SetRomDateSnmp(protocol, paramValue);
						break;
					}

				default:
				{		
					protocol.Log("QA" + protocol.QActionID + "|Unknown Trigger id", LogType.Error, LogLevel.NoLogging);
					break;
				}
            }
        }
        catch (Exception e)
        {
            protocol.Log("QA" + protocol.QActionID + "|Set Local Time|Error/Message: " + e, LogType.Error, LogLevel.NoLogging);
        }
    }

	private static void SetRomDateSnmp(SLProtocol protocol, string paramValue)
	{
		if (!paramValue.Equals(notAvailable.ToString()))
		{
			DateTime date = DateTime.ParseExact(paramValue, "MM/dd/yyyyK", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
			protocol.SetParameter(Parameter.cpqsm2cntlrromdate_50953, date.ToOADate());
		}
		else
		{
			protocol.SetParameter(Parameter.cpqsm2cntlrromdate_50953, notAvailable);
		}
	}

	private static void SetLocalTimeSnmp(SLProtocol protocol, string paramValue)
	{
		protocol.SetParameter(Parameter.localtime_109, sharedMethods.sharedMethods.getDateFromHexValue(paramValue));
	}
}		