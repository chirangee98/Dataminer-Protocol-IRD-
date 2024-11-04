using System;
using System.IO;
using System.Linq;
using Skyline.DataMiner.Scripting;

public class QAction
{	
	public void ChangePath(SLProtocol protocol)
	{
		//get the csv files from this path
		string DocPath = Convert.ToString(protocol.GetParameter(Parameter.pathofthefiles_2011)) +"\\";
		if (Directory.Exists(DocPath))
		{	
			string[] csvfilePaths = Directory.GetFiles(DocPath, "*.csv").Where(f => f.EndsWith(".csv")).Select(f => Path.GetFileName(f)).ToArray();
			protocol.SetParameter(Parameter.csvfilelist_2003, string.Join(";", csvfilePaths));
		}
	}
}