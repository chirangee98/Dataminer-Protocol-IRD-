using System;
using System.Net;
using System.Net.Sockets;
using multiThreadMethods;
using Skyline.DataMiner.Scripting;

public class QAction
{
    public static void Run(SLProtocol protocol)
    {
        String sRowKey = protocol.RowKey();
        String ElementID = Convert.ToString(protocol.GetParameter(2));
        String UniqueID = ElementID + "-" + sRowKey;

        lock (multiThreadMethods.multiThreadMethods.provideKeyLock(UniqueID))
        {
            object[] oRow = (object[])protocol.GetRow(Parameter.Portlist.tablePid, sRowKey);
            object[] oValues = (object[])protocol.GetParameters(new UInt32[] { Parameter.pollingipaddress_40001, Parameter.pollingperiod_40031, Parameter.pollingexecution_40033 });

            Int32 iPorStatus = Convert.ToInt32(oValues[2]);

            //Because of a bug in the mutithreading functionality of dataminer we have to check if the 
            //key in the table still exists. If not we don't have to perform any QAction.
            if (Convert.ToString(oRow[0]).Equals(string.Empty))
                return;

            if (Convert.ToString(oRow[8]).Equals(string.Empty))
                protocol.SetParameterIndexByKey(Parameter.Portlist.tablePid, sRowKey, 9, 4);// Set a default value of 4 retries.

            if ((iPorStatus == 1) && (Convert.ToString(protocol.GetParameter(Parameter.pollingwasenabled_3)) == ""))
            {
                string sProtocolWasEnabled = "true";
                protocol.SetParameter(Parameter.pollingwasenabled_3, sProtocolWasEnabled);
                Int32 iPeriod = Convert.ToInt32(oValues[1]);
                protocol.SetParameterIndexByKey(Parameter.Portlist.tablePid, sRowKey, 10, iPeriod);
            }

            if ((Convert.ToInt32(oRow[9]) <= 0) && iPorStatus == 1)
            {
                CurrState state = new CurrState();

                state.Ipaddress = IPAddress.Parse(Convert.ToString(oValues[0]));

                state.Port = Convert.ToInt32(oRow[0]);
                state.Timeout = Convert.ToInt64(oRow[1]);
                state.Retries = Convert.ToInt32(oRow[8]);

                double flgDelay = 0;
                int flgNbrSucceed = 0;

                if (state.Retries != 0)
                {
                    for (int i = 0; i <= state.Retries; i++)
                    {
                        IPEndPoint myIpe = new IPEndPoint(state.Ipaddress, state.Port);
                        Socket mySocket = new Socket(myIpe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                        long err;
                        if (long.TryParse(Convert.ToString(state.Timeout), out err))
                        {
                            mySocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 2000);
                        }
                        else
                        {
                            mySocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, state.Timeout);
                        }

                        bool flgSucceed = false;
                        DateTime dtBefore = DateTime.Now;
                        long interval = state.Timeout;

						if (i==0)
						{
							protocol.SetParameterIndexByKey(Parameter.Portlist.tablePid, sRowKey, 8, Convert.ToString(dtBefore));
						}
                            
                        try
                        {
                            mySocket.Connect(myIpe);

                            if (mySocket.Connected)
                            {
                                //protocol.Log(8, 5, "TTrue");
                                flgSucceed = true;
                                flgNbrSucceed++;
                                state.Comment = state.Comment + "Ok|";
                                //protocol.Log(8, 5, "Connected so Dispose Timer");
                            }
                            else
                            {
                                state.Comment = state.Comment + "Not connected|";
                            }
                        }
                        catch (Exception e)
                        {
                            //protocol.Log(8, 5, "EException");

                            flgSucceed = false;

                            DateTime dtAfter = DateTime.Now;
                            if (flgSucceed)
                            {
                                flgDelay = flgDelay + ((TimeSpan)(dtAfter - dtBefore)).TotalMilliseconds;
                            }

                            //protocol.Log(8, 5, "Not Connected so Dispose Timer");
                            state.Comment = state.Comment + e.Message + "|";
                        }
                        finally
                        {

                            if (mySocket != null)
                            {
                                mySocket.Close();
                                mySocket.Dispose();
                            }
                        }
                    }
                }

                FillData(protocol, sRowKey, state, flgDelay, flgNbrSucceed);
            }
            else
            {
                //If the polling status is disabled, reset the polling previous polling state.
                if (iPorStatus != 1)
                    protocol.SetParameter(Parameter.pollingwasenabled_3, "");

                //If the polling status is enabled, and the polling period is not yet expired, decrement by 1.
                if (iPorStatus == 1)
                {
                    Int32 iPeriod = Convert.ToInt32(oRow[9]);
                    iPeriod = iPeriod - 1;
                    protocol.SetParameterIndexByKey(Parameter.Portlist.tablePid, sRowKey, 10, iPeriod);
                }
            }
        }
    }

    private static void FillData(SLProtocol protocol, string rowKey, CurrState state, double myDelay, int mySuccess)
    {
        try
        {
            object[] oSetRow = new object[10];
            // Setting succeed flag
            oSetRow[3] = 100 * mySuccess / (1 + state.Retries);

            double delay = -2;
            if (mySuccess > 0)
                delay = myDelay / mySuccess;
            oSetRow[4] = delay;

            // Setting 
            oSetRow[5] = Convert.ToString(protocol.GetParameterIndexByKey(Parameter.Portlist.tablePid, rowKey, 8));
            oSetRow[6] = state.Comment;

            //if the polling period expires, reset the period to the original time.
            Int32 iPeriod = Convert.ToInt32(protocol.GetParameter(Parameter.pollingperiod_40031));
            oSetRow[9] = iPeriod - 1;

            protocol.SetRow(Parameter.Portlist.tablePid, rowKey, oSetRow);

        }
        catch (Exception e)
        {
            protocol.Log("QA" + protocol.QActionID + "|Monitor Multi-threading Port|Error/Message: " + e, LogType.Error, LogLevel.NoLogging);
        }
    }
}