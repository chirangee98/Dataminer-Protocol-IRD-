using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Skyline.DataMiner.Scripting;
public static class ProtocolExtension
{
	/// <summary>
	/// Gets the key (or another column) of the rows which have a specified value in a specified column.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
	/// <param name="tablePid">Parameter ID of the table.</param>
	/// <param name="returnedColumnIdx">IDX of the column to be returned.</param>
	/// <param name="filterColumnIdx">IDX of the column to be used to check if the row contains the specified value.</param>
	/// <param name="filterValue">Value to be used to check if the rows should be returned.</param>
	/// <returns>The values of the column specified in <paramref name="returnedColumnIdx"/> for the rows that matches.</returns>
	public static IEnumerable<string> GetKeysForValue(this SLProtocol protocol, int tablePid, uint returnedColumnIdx, uint filterColumnIdx, string filterValue)
	{
		object[] getColumns = (object[])protocol.NotifyProtocol(321, tablePid, new uint[] { returnedColumnIdx, filterColumnIdx });
		if (getColumns != null && getColumns.Length > 1)
		{
			object[] getKeys = (object[])getColumns[0];
			object[] getValues = (object[])getColumns[1];

			if (getKeys != null && getValues != null)
			{
				int rowCount = getKeys.Length;
				if (rowCount != getValues.Length)
				{
					throw new InvalidOperationException("The total number of primary keys is different than the total number of values in the table.");
				}

				for (int i = 0; i < rowCount; i++)
				{
					string getValue = Convert.ToString(getValues[i], CultureInfo.InvariantCulture);

					if (getValue.Equals(filterValue))
					{
						yield return Convert.ToString(getKeys[i]);
					}
				}
			}
		}
	}
}

public class QAction
{
	/// <summary>
	/// ProcessNameOrPathChange QAction.
	/// </summary>
	/// <param name="protocol">Link with Skyline DataMiner.</param>
	public static void Run(SLProtocol protocol)
	{
		try
		{
			string rowKey = protocol.RowKey();
			int trigParam = protocol.GetTriggerParameter();
			string sNewValue = Convert.ToString(protocol.GetParameter(trigParam));

			string displayKey;
			string path;
			string name;
			if (trigParam == 269)
			{
				// name change
				string currentPath = Convert.ToString(protocol.GetParameterIndexByKey(Parameter.Processcounter.tablePid, rowKey, 3));
				displayKey = sNewValue + ":" + currentPath;
				path = currentPath;
				name = sNewValue;
			}
			else
			{
				// runpath Change
				string currentName = Convert.ToString(protocol.GetParameterIndexByKey(Parameter.Processcounter.tablePid, rowKey, 4));
				displayKey = currentName + ":" + sNewValue;
				path = sNewValue;
				name = currentName;
			}

			bool displayKeyDoesntExist = ProtocolExtension.GetKeysForValue(protocol, Parameter.Processcounter.tablePid, 0, 6, displayKey).Count() == 0;

			if (displayKeyDoesntExist)
			{
				object[] row = new object[7];
				row[0] = rowKey;
				row[1] = null;
				row[2] = path;
				row[3] = name;
				row[4] = null;
				row[5] = null;
				row[6] = displayKey;
				protocol.SetRow(Parameter.Processcounter.tablePid, rowKey, row);
				protocol.CheckTrigger(80);
			}
			else
			{
				protocol.ShowInformationMessage("The combination Process Name, Process Run Path already exists. The set will not be executed.");
			}
		}
		catch (Exception ex)
		{
			protocol.Log("QA" + protocol.QActionID + "|Run|Error Message: " + ex, LogType.Error, LogLevel.NoLogging);
		}
	}
}