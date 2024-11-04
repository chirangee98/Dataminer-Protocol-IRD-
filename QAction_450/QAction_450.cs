using System;
using Skyline.DataMiner.Scripting;

public class QAction
{
	public static void Run(SLProtocol protocol)
	{
		//clear table if polling is set to disabled
		int pollingState = Convert.ToInt16(protocol.GetParameter(Parameter.pollingstatestoragetable_450));

		if (pollingState == 0)
		{
			sharedMethods.sharedMethods.DeleteAllRows(protocol, Parameter.Storagetable.tablePid);
			sharedMethods.sharedMethods.DeleteAllRows(protocol, Parameter.Storagestoragesize.tablePid);
			sharedMethods.sharedMethods.DeleteAllRows(protocol, Parameter.Storagestoragesize2.tablePid);
			sharedMethods.sharedMethods.DeleteAllRows(protocol, Parameter.Storageinstanceindex.tablePid);
		}
	}
}