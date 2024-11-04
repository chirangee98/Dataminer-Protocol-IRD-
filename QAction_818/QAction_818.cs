
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Skyline.DataMiner.Scripting;

public class QAction
{
	/// <summary>
	/// FunctionName
	/// </summary>
	/// <param name="protocol">Link with Skyline DataMiner</param>
	public static void Run(SLProtocolExt protocol)
	{
		protocol.CheckTrigger(21); //Polls the Interface Table
	}
}
