using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

using Skyline.DataMiner.Scripting;

public static class Extensions
{
	/// <summary>
	/// Converts an object to the desired type.
	/// </summary>
	/// <typeparam name="T">Type of the result.</typeparam>
	/// <param name="obj">Object to convert.</param>
	/// <returns>The converted object.</returns>
	public static T ChangeType<T>(this object obj) where T : IConvertible
	{
		if (obj == null)
		{
			return default(T);
		}

		if (typeof(T).IsEnum)
		{
			return (T)Enum.ToObject(typeof(T), obj.ChangeType<int>());
		}
		else if (typeof(T) == typeof(DateTime))
		{
			object date = DateTime.FromOADate(Convert.ToDouble(obj));
			return (T)date;
		}
		else
		{
			return (T)Convert.ChangeType(obj, typeof(T));
		}
	}

	/// <summary>
	/// Checks if a QAction that has a certain sleep time should run. If so, execute the QAction code.
	/// </summary>
	/// <param name="protocol">Skyline.DataMiner.Scripting.SLProtocol instance.</param>
	/// <param name="intervalParamId">
	/// Id of the parameter that holds the QAction sleep time in seconds.
	/// </param>
	/// <param name="lastRunParamId">
	/// Id of the parameter that holds the date when the QAction was last executed.
	/// </param>
	/// <param name="codeQAction">System.Action object with the QAction code.</param>
	/// <returns>True if the QAction was executed;otherwise false.</returns>
	public static bool ExecuteWaitingQAction(this SLProtocol protocol, int intervalParamId, int lastRunParamId, Action codeQAction)
	{
		int sleepTime;
		double lastRun;
		protocol.GetParameters(new[] { (uint)intervalParamId, (uint)lastRunParamId }, out sleepTime, out lastRun);

		var lastRunDate = DateTime.FromOADate(lastRun);

		if (codeQAction.ExecuteWithWaiting(TimeSpan.FromSeconds(sleepTime), lastRunDate))
		{
			protocol.SetParameter(lastRunParamId, DateTime.Now.ToOADate());
			return true;
		}

		return false;
	}

	/// <summary>
	/// Checks if some code that has a certain sleep time should run. If so, the code is executed.
	/// </summary>
	/// <param name="action">System.Action object with the code to execute.</param>
	/// <param name="sleepTime">TimeSpan with the time between consecutive code executions.</param>
	/// <param name="lastRunDate">DateTime with the date when the code was lat executed.</param>
	/// <returns>True if the code is executed;otherwise false.</returns>
	public static bool ExecuteWithWaiting(this Action action, TimeSpan sleepTime, DateTime lastRunDate)
	{
		if ((DateTime.Now - lastRunDate).TotalSeconds > sleepTime.TotalSeconds)
		{
			action();

			return true;
		}

		return false;
	}

	/// <summary>
	/// Gets the desired parameters and converts to the given types.
	/// </summary>
	/// <typeparam name="T1">Type of the fist parameter.</typeparam>
	/// <typeparam name="T2">Type of the second parameter.</typeparam>
	/// <typeparam name="T3">Type of the third parameter.</typeparam>
	/// <typeparam name="T4">Type of the fourth parameter.</typeparam>
	/// <typeparam name="T5">Type of the fifth parameter.</typeparam>
	/// <typeparam name="T6">Type of the sixth parameter.</typeparam>
	/// <typeparam name="T7">Type of the secenth parameter.</typeparam>
	/// <param name="protocol">Skyline.DataMiner.Scripting.SLProtocol instance.</param>
	/// <param name="paramIds">Array with the ids of the Parameters to fetch.</param>
	/// <param name="param1">Out variable with the first parameter value.</param>
	/// <param name="param2">Out variable with the second parameter value.</param>
	/// <param name="param3">Out variable with the third parameter value.</param>
	/// <param name="param4">Out variable with the fourth parameter value.</param>
	/// <param name="param5">Out variable with the fifth parameter value.</param>
	/// <param name="param6">Out variable with the sixth parameter value.</param>
	/// <param name="param7">Out variable with the seventh parameter value.</param>
	/// <exception cref="ArgumentException">
	/// If the length of the paramIds is different from the number of out parameters.
	/// </exception>
	public static void GetParameters<T1, T2, T3, T4, T5, T6, T7>(
		this SLProtocol protocol,
		uint[] paramIds,
		out T1 param1,
		out T2 param2,
		out T3 param3,
		out T4 param4,
		out T5 param5,
		out T6 param6,
		out T7 param7)
		where T1 : IConvertible
		where T2 : IConvertible
		where T3 : IConvertible
		where T4 : IConvertible
		where T5 : IConvertible
		where T6 : IConvertible
		where T7 : IConvertible
	{
		if (paramIds.Length != 7)
		{
			throw new ArgumentOutOfRangeException("paramIds", "paramIds need to have the same length as the number of out parameters");
		}

		var parameters = (object[])protocol.GetParameters(paramIds);

		param1 = parameters[0].ChangeType<T1>();
		param2 = parameters[1].ChangeType<T2>();
		param3 = parameters[2].ChangeType<T3>();
		param4 = parameters[3].ChangeType<T4>();
		param5 = parameters[4].ChangeType<T5>();
		param6 = parameters[5].ChangeType<T6>();
		param7 = parameters[6].ChangeType<T7>();
	}

	/// <summary>
	/// Gets the desired parameters and converts to the given types.
	/// </summary>
	/// <typeparam name="T1">Type of the fist parameter.</typeparam>
	/// <typeparam name="T2">Type of the second parameter.</typeparam>
	/// <typeparam name="T3">Type of the third parameter.</typeparam>
	/// <typeparam name="T4">Type of the fourth parameter.</typeparam>
	/// <typeparam name="T5">Type of the fifth parameter.</typeparam>
	/// <typeparam name="T6">Type of the sixth parameter.</typeparam>
	/// <param name="protocol">Skyline.DataMiner.Scripting.SLProtocol instance.</param>
	/// <param name="paramIds">Array with the ids of the Parameters to fetch.</param>
	/// <param name="param1">Out variable with the first parameter value.</param>
	/// <param name="param2">Out variable with the second parameter value.</param>
	/// <param name="param3">Out variable with the third parameter value.</param>
	/// <param name="param4">Out variable with the fourth parameter value.</param>
	/// <param name="param5">Out variable with the fifth parameter value.</param>
	/// <param name="param6">Out variable with the sixth parameter value.</param>
	/// <exception cref="ArgumentException">
	/// If the length of the paramIds is different from the number of out parameters.
	/// </exception>
	public static void GetParameters<T1, T2, T3, T4, T5, T6>(
	this SLProtocol protocol,
	uint[] paramIds,
	out T1 param1,
	out T2 param2,
	out T3 param3,
	out T4 param4,
	out T5 param5,
	out T6 param6)
	where T1 : IConvertible
	where T2 : IConvertible
	where T3 : IConvertible
	where T4 : IConvertible
	where T5 : IConvertible
	where T6 : IConvertible
	{
		if (paramIds.Length != 6)
		{
			throw new ArgumentOutOfRangeException("paramIds", "paramIds need to have the same length as the number of out parameters");
		}

		var parameters = (object[])protocol.GetParameters(paramIds);

		param1 = parameters[0].ChangeType<T1>();
		param2 = parameters[1].ChangeType<T2>();
		param3 = parameters[2].ChangeType<T3>();
		param4 = parameters[3].ChangeType<T4>();
		param5 = parameters[4].ChangeType<T5>();
		param6 = parameters[5].ChangeType<T6>();
	}

	/// <summary>
	/// Gets the desired parameters and converts to the given types.
	/// </summary>
	/// <typeparam name="T1">Type of the fist parameter.</typeparam>
	/// <typeparam name="T2">Type of the second parameter.</typeparam>
	/// <param name="protocol">Skyline.DataMiner.Scripting.SLProtocol instance.</param>
	/// <param name="paramIds">Array with the ids of the Parameters to fetch.</param>
	/// <param name="param1">Out variable with the first parameter value.</param>
	/// <param name="param2">Out variable with the second parameter value.</param>
	/// <exception cref="ArgumentException">
	/// If the length of the paramIds is different from the number of out parameters.
	/// </exception>
	public static void GetParameters<T1, T2>(this SLProtocol protocol, uint[] paramIds, out T1 param1, out T2 param2)
		where T1 : IConvertible
		where T2 : IConvertible
	{
		if (paramIds.Length != 2)
		{
			throw new ArgumentException("paramIds need to have the same length as the number of out parameters", "paramIds");
		}

		var parameters = (object[])protocol.GetParameters(paramIds);

		param1 = parameters[0].ChangeType<T1>();
		param2 = parameters[1].ChangeType<T2>();
	}
}

/// <summary>
/// DataMiner QAction Class: Ping Function.
/// </summary>
public class QAction
{
	/// <summary>
	/// Execute Ping.
	/// </summary>
	/// <param name="protocol">Link with Skyline DataMiner.</param>
	public static void Run(SLProtocol protocol)
	{
		protocol.ExecuteWaitingQAction(
			Parameter.pingcycle_50002,
			Parameter.pingpreviousexecution_50010,
			() =>
			{
				try
				{
					using (Ping ping = new Ping())
					{
						string ipAddress;
						int timeout;
						int numberOfPackets;
						int numberOfRequests;
						string requestsBuffer;
						string cyclesBuffer;

						uint[] pids = new uint[]
						{
							50022, Parameter.pingtimeout_50004, Parameter.pingnumber_50006,
							Parameter.pingrequestshistory_50047, Parameter.ping_lastrequestsbuffer_50045, Parameter.ping_lastcyclesbuffer_50046
						};

						protocol.GetParameters(
							pids,
							out ipAddress,
							out timeout,
							out numberOfPackets,
							out numberOfRequests,
							out requestsBuffer,
							out cyclesBuffer);

						var result = new PingReply[numberOfPackets];

						for (int i = 0; i < numberOfPackets; i++)
						{
							result[i] = ping.Send(ipAddress, timeout);
						}

						var successResults = result.Where(x => x.Status == IPStatus.Success).ToArray();

						var allSuccess = successResults.Count() == result.Count();
						int status = allSuccess ? 1 : 0;

						double avgDailySuccess;
						double avgWeeklySuccess;
						double avgMonthlySuccess;
						double requestsPacketLoss;
						double cyclesPacketLoss;

						CalculateAverageValues(
							protocol,
							numberOfPackets,
							result.Count(x => x.Status == IPStatus.Success),
							out avgDailySuccess,
							out avgWeeklySuccess,
							out avgMonthlySuccess);

						double avgSuccess = ((double)successResults.Count() / numberOfPackets) * 100.0;

						CalculatePacketLoss(
							ref requestsBuffer,
							ref cyclesBuffer,
							result,
							numberOfRequests,
							100 - avgSuccess,
							out requestsPacketLoss,
							out cyclesPacketLoss);

						object[] setObject = new object[]
						{
								status,
								successResults.Any() ? successResults.Average(x => x.RoundtripTime) : -1,
								successResults.Any() ? successResults.Min(x => x.RoundtripTime) : -1,
								successResults.Any() ? successResults.Max(x => x.RoundtripTime) : -1,
								avgSuccess,
								avgDailySuccess * 100.0,
								avgWeeklySuccess * 100.0,
								avgMonthlySuccess * 100.0,
								requestsPacketLoss,
								cyclesPacketLoss,
								requestsBuffer,
								cyclesBuffer
						};

						protocol.SetParameters(
							new[]
							{
								Parameter.pingresult_50012,
								Parameter.pingmeanrtt_50014,
								Parameter.pingminrtt_50016,
								Parameter.pingmaxrtt_50017,
								Parameter.pingavgsuccess_50018,
								Parameter.ping_dailyavailability_50040,
								Parameter.ping_weeklyavailability_50041,
								Parameter.ping_monthlyavailability_50042,
								Parameter.ping_packetlossrequests_50043,
								Parameter.ping_packetlosscycles_50044,
								Parameter.ping_lastrequestsbuffer_50045,
								Parameter.ping_lastcyclesbuffer_50046
							},
							setObject);
					}
				}
				catch (Exception e)
				{
					protocol.Log("QA" + protocol.QActionID + "Run|Failed to Execute Ping due to Exception: " + Environment.NewLine + e, LogType.Error, LogLevel.NoLogging);
				}
			});
	}

	public static void CalculateAverageValues(
		SLProtocol protocol,
		int numberOfPackets,
		int numberOfSuccessPings,
		out double avgDailySuccess,
		out double avgWeeklySuccess,
		out double avgMonthlySuccess)
	{
		long dailyCounter;
		long weeklyCounter;
		long monthlyCounter;

		long numDailySuccess;
		long numWeeklySuccess;
		long numMonthlySuccess;

		double firstUpdate;

		uint[] getPids = new uint[]
		{
			Parameter.dailytotalcounter_50051, Parameter.weeklytotalcounter_50054, Parameter.monthlytotalcounter_50057, Parameter.dailysuccesscounter_50052,
			Parameter.weeklysuccesscounter_50055, Parameter.monthlysuccesscounter_50058, Parameter.pingfirstdailyupdatetime_50039
		};

		protocol.GetParameters(
			getPids,
			out dailyCounter,
			out weeklyCounter,
			out monthlyCounter,
			out numDailySuccess,
			out numWeeklySuccess,
			out numMonthlySuccess,
			out firstUpdate);

		// Reset values
		ResetValues(
			protocol,
			firstUpdate,
			ref dailyCounter,
			ref weeklyCounter,
			ref monthlyCounter,
			ref numDailySuccess,
			ref numWeeklySuccess,
			ref numMonthlySuccess);

		dailyCounter += numberOfPackets;
		weeklyCounter += numberOfPackets;
		monthlyCounter += numberOfPackets;

		numDailySuccess += numberOfSuccessPings;
		numWeeklySuccess += numberOfSuccessPings;
		numMonthlySuccess += numberOfSuccessPings;

		avgDailySuccess = (double)numDailySuccess / dailyCounter;
		avgWeeklySuccess = (double)numWeeklySuccess / weeklyCounter;
		avgMonthlySuccess = (double)numMonthlySuccess / monthlyCounter;

		int[] setPids = new[]
		{
			Parameter.dailytotalcounter_50051, Parameter.weeklytotalcounter_50054, Parameter.monthlytotalcounter_50057,
			Parameter.dailysuccesscounter_50052, Parameter.weeklysuccesscounter_50055, Parameter.monthlysuccesscounter_50058
		};

		protocol.SetParameters(
			setPids,
			new object[] { dailyCounter, weeklyCounter, monthlyCounter, numDailySuccess, numWeeklySuccess, numMonthlySuccess });
	}

	public static void ResetValues(
		SLProtocol protocol,
		double firstUpdate,
		ref long dailyCounter,
		ref long weeklyCounter,
		ref long monthlyCounter,
		ref long numDailySuccess,
		ref long numWeeklySuccess,
		ref long numMonthlySuccess)
	{
		if (firstUpdate <= 0)
		{
			protocol.SetParameter(Parameter.pingfirstdailyupdatetime_50039, DateTime.Now.ToOADate());
		}
		else
		{
			DateTime now = DateTime.Now;
			DateTime firstUpdateDate = DateTime.FromOADate(firstUpdate);

			if (firstUpdateDate.Day != now.Day)
			{
				// Update Daily values
				dailyCounter = 0;
				numDailySuccess = 0;

				// Check if it is possible to update the weekly values
				if (firstUpdateDate.DayOfWeek == DayOfWeek.Monday)
				{
					weeklyCounter = 0;
					numWeeklySuccess = 0;
				}

				// Check if it is possible to update the monthly values
				if (now.Day == 1)
				{
					monthlyCounter = 0;
					numMonthlySuccess = 0;
				}

				protocol.SetParameter(Parameter.pingfirstdailyupdatetime_50039, DateTime.Now.ToOADate());
			}
		}
	}

	private static void CalculatePacketLoss(ref string requestsBuffer, ref string cyclesBuffer, PingReply[] result, int numberOfRequests, double avgLoss, out double requestsPacketLoss, out double cyclesPacketLoss)
	{
		double[] results = new double[result.Length];

		for (int i = 0; i < result.Length; i++) results[i] = result[i].Status == IPStatus.Success ? 0 : 1;

		string[] reqBuffer = ManageBuffer(requestsBuffer, numberOfRequests, results);
		string[] cBuffer = ManageBuffer(cyclesBuffer, 10, new[] { avgLoss });

		int iCount = reqBuffer.Count(e => e.Equals("1"));
		requestsPacketLoss = (iCount * 100) / reqBuffer.Length;

		cyclesPacketLoss = cBuffer.Select(e => Convert.ToDouble(e)).Average();

		requestsBuffer = String.Join(";", reqBuffer);
		cyclesBuffer = String.Join(";", cBuffer);
	}

	private static string[] ManageBuffer(string buffer, int maxValue, double[] values)
	{
		Queue<string> queue = !String.IsNullOrWhiteSpace(buffer) ? new Queue<string>(buffer.Split(';')) : new Queue<string>();

		for (int i = 0; i < values.Length; i++)
		{
			if (queue.Count >= maxValue)
			{
				int valuesToDelete = queue.Count - maxValue;

				for (int j = 0; j < valuesToDelete + 1; j++)
				{
					queue.Dequeue();
				}
			}

			queue.Enqueue(Convert.ToString(values[i]));
		}

		return queue.ToArray();
	}
}
