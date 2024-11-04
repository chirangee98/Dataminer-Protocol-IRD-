using System;
using Skyline.DataMiner.Scripting;

/// <summary>
/// DataMiner QAction Class.
/// </summary>
public class QAction
{
	private DiskiotableQActionRow row;

	/// <summary>
	/// The QAction entry point.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
	public void Run(SLProtocol protocol)
	{
		try
		{
			var rows = protocol.RowCount(Parameter.Diskiotable.tablePid);
			var seconds = (Environment.TickCount & 0x7FFFFFFF) / 1000;

			var previousSeconds = Convert.ToUInt32(protocol.GetParameter(Parameter.tickcounter_10151));
			var diffTime = seconds - previousSeconds;
			var uptime = Convert.ToDouble(protocol.GetParameter(Parameter.sysuptimeprocessed_102));

			for (int i = 0; i < rows; i++)
			{
				row = new DiskiotableQActionRow((object[])protocol.GetRow(Parameter.Diskiotable.tablePid, i));

				CalculateRateReads(protocol, diffTime, uptime);
				CalculateRateWrites(protocol, diffTime, uptime);
				CalculateReadAccessRate(protocol, diffTime);
				CalculateWriteAccessRate(protocol, diffTime);

				protocol.SetRow(Parameter.Diskiotable.tablePid, i, row.ToObjectArray());					
			}
			
			protocol.SetParameter(Parameter.tickcounter_10151, seconds);
		}
		catch (Exception e)
		{
			protocol.Log("QA" + protocol.QActionID + "|Fill in Custom Params - Disk IO|Error/Message: " + e, LogType.Error, LogLevel.NoLogging);
		}
	}

	private void CalculateWriteAccessRate(SLProtocol protocol, double diffTime)
	{
		if (row.Diskiowrites_10110 == null)
		{
			return;
		}

		var accessWrites = Convert.ToUInt32(row.Diskiowrites_10110);
		if (diffTime > 0)
		{
			if (row.Diskiowritesbefore_10114 != null)
			{
				var previousAccessWrites = Convert.ToUInt32(row.Diskiowritesbefore_10114);
				if (accessWrites >= previousAccessWrites)
				{
					row.Diskiowritesrate_10113 = (accessWrites - previousAccessWrites) / diffTime;
				}
				else
				{
					protocol.Log(8, 5, "Handling Writes Rate Overflow");
				}
			}
		}
		else
		{
			protocol.Log(8, 5, "Handling Time Overflow");
		}

		row.Diskiowritesbefore_10114 = accessWrites;
	}

	private void CalculateReadAccessRate(SLProtocol protocol, double diffTime)
	{
		if (row.Diskioreads_10109 == null)
		{
			return;
		}

		var accessReads = Convert.ToUInt32(row.Diskioreads_10109);
		if (diffTime > 0)
		{
			if (row.Diskioreadsbefore_10112 != null)
			{
				var previousAccessReads = Convert.ToUInt32(row.Diskioreadsbefore_10112);
				if (accessReads >= previousAccessReads)
				{
					row.Diskioreadsrate_10111 = (accessReads - previousAccessReads) / diffTime;
				}
				else
				{
					protocol.Log(8, 5, "Handling Reads Rate Overflow");
				}
			}
		}
		else
		{
			protocol.Log(8, 5, "Handling Time Overflow");
		}

		row.Diskioreadsbefore_10112 = accessReads;
	}

	private void CalculateRateWrites(SLProtocol protocol, double diffTime, double uptime)
	{
		if (row.Diskionwritten_10106 == null)
		{
			return;
		}

		var bytesWritten = Convert.ToUInt32(row.Diskionwritten_10106);
		if (diffTime > 0)
		{
			if (row.Diskionwrittenbefore_10108 != null)
			{
				var previousBytesWritten = Convert.ToUInt32(row.Diskionwrittenbefore_10108);
				if (bytesWritten >= previousBytesWritten)
				{
					double diffWritten = bytesWritten - previousBytesWritten;
					double rateWritten = diffWritten / diffTime / (1000 / 8);

					row.Diskionwrittenrate_10107 = rateWritten;
					row.Diskioaveragesecondwrite_10116 = bytesWritten != 0 ? 1000 * (diffWritten * uptime) / bytesWritten : 0;
				}
				else
				{
					protocol.Log("QA" + protocol.QActionID + "|Handling Written Rate Overflow", LogType.Error, LogLevel.NoLogging);
				}
			}
		}
		else
		{
			protocol.Log("QA" + protocol.QActionID + "|Handling Time Overflow", LogType.Error, LogLevel.NoLogging);
		}

		row.Diskionwrittenbefore_10108 = bytesWritten;
	}

	private void CalculateRateReads(SLProtocol protocol, double diffTime, double uptime)
	{
		if (row.Diskionread_10103 == null)
		{
			return;
		}

		var bytesRead = Convert.ToUInt32(row.Diskionread_10103);
		if (diffTime > 0)
		{
			if (row.Diskionreadbefore_10105 != null)
			{		
				var previousBytesRead = Convert.ToUInt32(row.Diskionreadbefore_10105);
				if (bytesRead >= previousBytesRead)
				{
					double diffRead = bytesRead - previousBytesRead;
					double rateRead = diffRead / diffTime / (1000 / 8);

					row.Diskionreadrate_10104 = rateRead;
					row.Diskioaveragesecondread_10115 = bytesRead != 0 ? 1000 * (diffRead * uptime) / bytesRead : 0;
				}
				else
				{
					protocol.Log("QA" + protocol.QActionID + "|Handling Read Rate Overflow", LogType.DebugInfo, LogLevel.NoLogging);
				}
			}
		}
		else
		{
			protocol.Log("QA" + protocol.QActionID + "|Handling Time Overflow", LogType.DebugInfo, LogLevel.NoLogging);
		}

		row.Diskionreadbefore_10105 = bytesRead;
	}
}