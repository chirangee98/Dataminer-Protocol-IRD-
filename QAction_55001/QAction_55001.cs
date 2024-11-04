using Skyline.DataMiner.Scripting;

public class QAction
{
    public static void Run(SLProtocol protocol)
    {
        string rowentry = (string)protocol.GetParameter(Parameter.Write.removeexternaldataentry_8301);
		protocol.DeleteRow(Parameter.Externaldataoverview.tablePid, "" + rowentry);
    }
}