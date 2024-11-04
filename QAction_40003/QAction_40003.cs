using System;
using System.Net;
using Skyline.DataMiner.Scripting;

public class QAction
{
    public static void Run(SLProtocol protocol)
    {
        string myAddress = Convert.ToString(protocol.GetParameter(Parameter.Write.triggerupdateaddress_40003));
        string targetIp = "";

        IPAddress[] myIpAddresses = Dns.GetHostAddresses(myAddress); ;

        if (myIpAddresses.Length > 0)
            targetIp = Convert.ToString(myIpAddresses[0]);

        protocol.SetParameter(Parameter.pollingipaddress_40001, targetIp);
    }
}