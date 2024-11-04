using Skyline.DataMiner.Scripting;

public class QAction
{
    public static void Run(SLProtocol protocol)
    {
        string newKey = (string)protocol.GetParameter(Parameter.addexternaldataentry_8200);
        if (protocol.GetKeyPosition(Parameter.Externaldataoverview.tablePid, newKey) == 0)
        {
			protocol.AddRow(Parameter.Externaldataoverview.tablePid, newKey);
        }
    }
}