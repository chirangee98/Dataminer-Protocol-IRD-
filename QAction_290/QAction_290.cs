using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Skyline.DataMiner.Scripting;

public class QAction
{
	/// <summary>
	/// ImportClasses
	/// </summary>
	/// <param name="protocol">Link with Skyline Dataminer</param>
	public static void Run(SLProtocol protocol)
	{
		////
	}
}

public class Converters
{
	/// <summary>
	/// Use when in normal behaviour the convert can fail as well as succeed. The Fail is in this case no 'Exception'
	/// TryParse is used so it will be processed a bit "slower" than Convert in case it's succeeded
	/// </summary>
	/// <param name="obj">The object to parse</param>
	/// <param name="dValue">the double Value</param>
	/// <returns>false if fails</returns>
	public static bool TryParseToInvariantDouble(object obj, out double dValue)
	{
		if (obj.GetType() == typeof(string))
		{ obj = Convert.ToString(obj).Replace(",", "."); }

		return Double.TryParse(Convert.ToString(obj, CultureInfo.InvariantCulture), System.Globalization.NumberStyles.Any, NumberFormatInfo.InvariantInfo, out dValue);

	}

	/// <summary>
	/// Use when in normal behaviour the convert will work. Only in 'Exception' it will fail.
	/// Try-Catch is used so when it fails it will be processed "slower" than the Parse.
	/// </summary>
	/// <param name="obj">The object to convert</param>
	/// <param name="dValue">The double Value</param>
	/// <returns>false if function fails</returns>
	public static bool TryConvertToInvariantDouble(object obj, out double dValue)
	{

		if (obj.GetType() == typeof(string))
		{ obj = Convert.ToString(obj).Replace(",", "."); }

		try
		{
			dValue = Convert.ToDouble(obj, CultureInfo.InvariantCulture);
			return true;
		}
		catch (Exception)
		{
			dValue = 0;
			return false;
		}
	}

	/// <summary>
	/// Use when in normal behaviour the convert will work. Only in 'Exception' it will fail.
	/// Try-Catch is used so when it fails it will be processed "slower" than the Parse.
	/// </summary>
	/// <param name="obj">The object to convert</param>
	/// <param name="dValue">The double Value</param>
	/// <param name="exception">The exception that was thrown when the Convert failed</param>
	/// <returns>false if function fails</returns>
	public static bool TryConvertToInvariantDouble(object obj, out double dValue, out Exception exception)
	{

		if (obj.GetType() == typeof(string))
		{ obj = Convert.ToString(obj).Replace(",", "."); }

		try
		{
			dValue = Convert.ToDouble(obj, CultureInfo.InvariantCulture);
			exception = null;
			return true;
		}
		catch (System.Exception e)
		{
			dValue = 0;
			exception = e;
			return false;
		}
	}
}


public class CSVImport
{
	/// <summary>
	/// read all the rows in the CSV
	/// </summary>
	/// <param name="Path">Path of the csv file</param>
	/// <param name="FileName">Name of the csv file</param>
	/// <returns>list of all the rows in the csv</returns>
	public static IEnumerable<string> GetFileData(string Path, string FileName)
	{
		FileName = FileName.Substring(0, FileName.LastIndexOf('.')) + ".csv";
		string Location = Path + "\\" + FileName;
		if (File.Exists(Location))
		{
			//make sure it can be opened even if its used by another process
			using (FileStream logReader = new FileStream(Location, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				//read the file with encoding => used for chars like 'é'
				using (StreamReader sr = new StreamReader(logReader, Encoding.Default))
				{
					while (!sr.EndOfStream)
					{
						yield return sr.ReadLine();
					}
				}
			}
		}
	}

	/// <summary>
	/// splits the columns in the csv independed of ';' or ',' separator
	/// </summary>
	/// <param name="Row">the row</param>
	/// <param name="separator">the used separator</param>
	/// <returns>array of column data in the row</returns>
	public static string[] SplitCSVRow(string Row)
	{
		//detect the column separator
		char separator = GetUsedSeparator(Row);
		string[] Values = ParseLine(Row, separator);

		return Values;
	}

	public static char GetUsedSeparator(string Row)
	{
		char separator;

		if (Row.Count(c => c == ';') > Row.Count(c => c == ','))
			separator = ';';
		else
			separator = ',';
		return separator;
	}

	private static string[] ParseLine(string sLine, char colSeparator)
	{
		StringBuilder sTemp = new StringBuilder();

		List<string> Parts = new List<string>();
		bool _stringSection = false;
		bool _escapeQuote = false;

		bool _prevCharIsQuote = false;
		bool _realCharFound = false;
		bool _ignoreExtraQuote = false;
		int pos = 0;

		foreach (char c in sLine)
		{
			switch (c)
			{
				case '"':
					{
						if (pos + 1 < sLine.Length)
						{
							if (sLine[pos + 1] == '"')
							{
								//see this char as a real char

								// "" is ", """" is ""
								if (!_ignoreExtraQuote)
								{
									//check for ,"",  or "", or ,""
									bool emptyString = false;
									if (pos + 2 < sLine.Length)
									{
										if (sLine[pos + 2] == colSeparator)
										{
											if (pos - 1 <= 0 || sLine[pos - 1] == colSeparator)
											{
												//we have ,"",  or (start) "",
												//Empty
												emptyString = true;
											}

										}
									}
									else
									{
										// ,"" (end)
										emptyString = true;
									}
									if (!emptyString)
									{
										sTemp.Append(c);
									}
									_ignoreExtraQuote = true;
								}
								else
								{
									_ignoreExtraQuote = false;


								}
								_escapeQuote = false;
								_prevCharIsQuote = false;
								_realCharFound = true;
							}
						}

						if (_escapeQuote && _realCharFound)
							sTemp.Append(c);
						else
							_stringSection = !_stringSection;


						_escapeQuote = !_escapeQuote;

						_prevCharIsQuote = true;
						_realCharFound = false;

						break;
					}

				default:
					{
						if (c == colSeparator && !_stringSection)
						{
							Parts.Add(sTemp.ToString());
							sTemp.Clear();
							if (_prevCharIsQuote)
								_escapeQuote = false;
							break;
						}

						sTemp.Append(c);

						_escapeQuote = false;
						_prevCharIsQuote = false;
						_realCharFound = true;

						_ignoreExtraQuote = false;
						break;
					}
			}

			pos++;
		}

		Parts.Add(sTemp.ToString());
		return Parts.ToArray();
	}
}