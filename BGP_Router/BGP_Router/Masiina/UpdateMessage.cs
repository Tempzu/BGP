using BGP_Router.BGP;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BGP_Router.Messages;
using System.Net.Sockets;
using System.Net;

namespace BGP_Router.Masiina
{
    public class UpdateMessage
    {
        /* UPDATE message can be only received when the state is established. When an UPDATE message is received, each field needs to be checked.
         * */

        FSM fsm = new FSM();
        int count = 0;

        public void adj_RIB_Out()
        {
            int i = 0;
            Variables.Adj_RIB_Out.Clear();
            Variables.InterAS_IP.Clear();
            foreach (DataRow row in Variables.Data.Rows)
            {
                // Method for finding adjacent peers in AS1
                if (row.Field<int>(2) == 1 || row.Field<int>(4) == 1)
                {
                    if (row.Field<int>(5) == 1)
                    {
                        i++;
                        Tuple<string> neighbourAS = new Tuple<string>(Variables.PrefixAS2);
                        //Tuple consists of connection count, network, N_AS, Next_Hop, NH_AS, EGP/IGP, Local AS_PREFIX, Neighbour AS_prefix
                        Tuple<int, string, int, string, int, int, string, Tuple<string>> Adj_RIB_Out =
                                new Tuple<int, string, int, string, int, int, string, Tuple<string>>(row.Field<int>(0), row.Field<string>(1), row.Field<int>(2),
                        row.Field<string>(3), row.Field<int>(4), row.Field<int>(5), Variables.PrefixAS1, neighbourAS);
                        Variables.Adj_RIB_Out.Add(i, Adj_RIB_Out);
                        Variables.InterAS_IP.Add(i, row.Field<string>(1));
                     }
                }

                // Method for finding adjacent peers in AS2
                if (row.Field<int>(2) == 2 || row.Field<int>(4) == 2)
                {
                    if (row.Field<int>(5) == 1)
                    {
                        i++;
                        //Tuple consists of connection count, network, N_AS, Next_Hop, NH_AS, EGP/IGP, AS_prefix
                        if (row.Field<int>(2) == 1)
                        {
                            Tuple<string> neighbourAS = new Tuple<string>(Variables.PrefixAS1);
                            Tuple<int, string, int, string, int, int, string, Tuple<string>> Adj_RIB_Out =
                                 new Tuple<int, string, int, string, int, int, string, Tuple<string>>(row.Field<int>(0), row.Field<string>(1), row.Field<int>(2),
                         row.Field<string>(3), row.Field<int>(4), row.Field<int>(5), Variables.PrefixAS2, neighbourAS);
                            Variables.Adj_RIB_Out.Add(i, Adj_RIB_Out);
                        }
                        if (row.Field<int>(4) == 3)
                        {
                            Tuple<string> neighbourAS = new Tuple<string>(Variables.PrefixAS3);
                            Tuple<int, string, int, string, int, int, string, Tuple<string>> Adj_RIB_Out =
                                 new Tuple<int, string, int, string, int, int, string, Tuple<string>>(row.Field<int>(0), row.Field<string>(1), row.Field<int>(2),
                         row.Field<string>(3), row.Field<int>(4), row.Field<int>(5), Variables.PrefixAS2, neighbourAS);
                            Variables.Adj_RIB_Out.Add(i, Adj_RIB_Out);
                        }

                        Variables.InterAS_IP.Add(i, row.Field<string>(3));
                                            
                    }
                }
                // Method for finding adjacent peers in AS3
                if (row.Field<int>(2) == 3 || row.Field<int>(4) == 3)
                {
                    if (row.Field<int>(5) == 1)
                    {
                        i++;
                        Tuple<string> neighbourAS = new Tuple<string>(Variables.PrefixAS2);
                        //Tuple consists of connection count, network, N_AS, Next_Hop, NH_AS, EGP/IGP, AS_prefix
                        Tuple<int, string, int, string, int, int, string, Tuple<string>> Adj_RIB_Out =
                                 new Tuple<int, string, int, string, int, int, string, Tuple<string>>(row.Field<int>(0), row.Field<string>(1), row.Field<int>(2),
                         row.Field<string>(3), row.Field<int>(4), row.Field<int>(5), Variables.PrefixAS3, neighbourAS);
                        Variables.InterAS_IP.Add(i, row.Field<string>(1));
                        Variables.Adj_RIB_Out.Add(i, Adj_RIB_Out);
                      
                    }
                }

            }

        }

        //PathAttribute consists of attribute(origin), attribute length, attribute flag and attribute TypeCode
        public void pathAttribute()
        {
            Variables.PathAttribute.Clear();
            int i = 0;
            foreach (KeyValuePair<int, Tuple<int, string, int, string, int, int, string, Tuple<string>>> entry in Variables.Adj_RIB_Out)
            {
                i++;
                Tuple<int, string, int, ushort> pathAttribute = new Tuple<int, string, int, ushort>(entry.Value.Item2.Length, entry.Value.Item2, entry.Value.Item6, (ushort)entry.Value.Item6);
                Variables.PathAttribute.Add(i, pathAttribute);

            }

        }
        public void networkLayerReachibility()
        {
            Variables.NLRI.Clear();
            int i = 0;
            foreach (KeyValuePair<int, Tuple<int, string, int, string, int, int, string, Tuple<string>>> entry in Variables.Adj_RIB_Out)
            {
                i++;
                Tuple<int, string> nlri = new Tuple<int, string>(entry.Value.Rest.Item1.Length, entry.Value.Rest.Item1);
                Variables.NLRI.Add(i, nlri);
            }
        }
        public void pathSegment()
        {
            Variables.PathSegment.Clear();
            int i = 0;
            foreach (KeyValuePair<int, Tuple<int, string, int, string, int, int, string, Tuple<string>>> entry in Variables.Adj_RIB_Out)
            {
                i++;
                Tuple<int, string> pathSegment = new Tuple<int, string>(1, "" + entry.Value.Item3 + "" + entry.Value.Item5);
                Variables.PathSegment.Add(i, pathSegment);
            }
        }


// Different cases for update message sending
        public void sendUpdateMsg(int i)
        {
            Listener bgpListener = new Listener();
            Speaker bgpSpeaker = new Speaker();

            foreach (KeyValuePair<int, Tuple<int, string, int, string, int, int, string, Tuple<string>>> entry in Variables.Adj_RIB_Out)
            {
                if (i == entry.Key)
                {
                    switch (i)
                    {
                        case 1:
                            Tuple<int, string> nlri = Variables.NLRI[entry.Key];
                            Tuple<int, string> pathSegment = Variables.PathSegment[entry.Key];
                            Tuple<int, string, int, ushort> pathAttribute = Variables.PathAttribute[entry.Key];
                            Tuple<int, string, int, string, int, int, string, Tuple<string>> adj_RIB_Out = Variables.Adj_RIB_Out[entry.Key];


                            if (Variables.WithdrawnRoutes.ContainsKey(1))
                            {
                                Tuple<string, int> withdrawlInfo = Variables.WithdrawnRoutes[1];
                                Variables.Withdrawl_IP_Address = withdrawlInfo.Item1;
                                Variables.Withdrawl_Length = withdrawlInfo.Item2;
                            }
                            else
                            {
                                Variables.Withdrawl_IP_Address = "";
                                Variables.Withdrawl_Length = 0;
                            }
                            Update updatePacket = new Update((UInt16)Variables.Withdrawl_Length, Variables.Withdrawl_IP_Address, (ushort)adj_RIB_Out.Item7.Length,
                               adj_RIB_Out.Item7, 24, (UInt32)pathAttribute.Item1, (UInt32)pathAttribute.Item3, (ushort)pathAttribute.Item4, pathAttribute.Item2, 1,
                               (ushort)pathSegment.Item1, pathSegment.Item2, (ushort)nlri.Item1, nlri.Item2);
                            foreach (KeyValuePair<int, Tuple<string, ushort, string, ushort>> speakerListner in Variables.ConnectionSpeakerAs_ListenerAs)
                            {
                                if ((adj_RIB_Out.Item2 == speakerListner.Value.Item3) && (speakerListner.Value.Item2 == 1) && (speakerListner.Value.Item4 == 1))
                                {
                                    foreach (KeyValuePair<int, Socket> listner in Variables.ListenerSocketDictionary)
                                    {
                                        try
                                        {
                                            if ((speakerListner.Value.Item3 == "" + IPAddress.Parse(((IPEndPoint)listner.Value.LocalEndPoint).Address.ToString())) &&
                                                (speakerListner.Value.Item1 == "" + IPAddress.Parse(((IPEndPoint)listner.Value.RemoteEndPoint).Address.ToString())))
                                            {
                                                
                                                bgpListener.SendSpeaker(updatePacket.BGPmessage, listner.Value, "Update");
                                                fsm.BGPUpdateMessageSent(Variables.True);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine(ex.ToString());
                                        }
                                    }
                                }
                                if ((adj_RIB_Out.Item2 == speakerListner.Value.Item1) && (speakerListner.Value.Item2 == 1) && (speakerListner.Value.Item4 == 1))
                                {
                                    foreach (KeyValuePair<int, Socket> speaker in Variables.SpeakerSocketDictionary)
                                    {
                                        try
                                        {
                                            if ((speakerListner.Value.Item1 == "" + IPAddress.Parse(((IPEndPoint)speaker.Value.LocalEndPoint).Address.ToString())) &&
                                                (speakerListner.Value.Item3 == "" + IPAddress.Parse(((IPEndPoint)speaker.Value.RemoteEndPoint).Address.ToString())))
                                            {
                                                bgpSpeaker.SendListener(updatePacket.BGPmessage, speaker.Value, "Update");
                                                fsm.BGPUpdateMessageSent(Variables.True);
                                            }


                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine(ex.ToString());
                                        }
                                    }
                                }
                            }
                            break;
                        case 2:
                            nlri = Variables.NLRI[entry.Key];
                            pathSegment = Variables.PathSegment[entry.Key];
                            pathAttribute = Variables.PathAttribute[entry.Key];
                            adj_RIB_Out = Variables.Adj_RIB_Out[entry.Key];

                            if (Variables.WithdrawnRoutes.ContainsKey(2))
                            {
                                Tuple<string, int> withdrawlInfo = Variables.WithdrawnRoutes[2];
                                Variables.Withdrawl_IP_Address = withdrawlInfo.Item1;
                                Variables.Withdrawl_Length = withdrawlInfo.Item2;

                            }
                            else
                            {
                                Variables.Withdrawl_IP_Address = "";
                                Variables.Withdrawl_Length = 0;
                            }
                            updatePacket = new Update ((UInt16)Variables.Withdrawl_Length, Variables.Withdrawl_IP_Address, (ushort)adj_RIB_Out.Item7.Length,
                               adj_RIB_Out.Item7, 24, (UInt32)pathAttribute.Item1, (UInt32)pathAttribute.Item3, (ushort)pathAttribute.Item4, pathAttribute.Item2, 1,
                               (ushort)pathSegment.Item1, pathSegment.Item2, (ushort)nlri.Item1, nlri.Item2);
                            foreach (KeyValuePair<int, Tuple<string, ushort, string, ushort>> speakerListner in Variables.ConnectionSpeakerAs_ListenerAs)
                            {

                                if ((adj_RIB_Out.Item4 == speakerListner.Value.Item3) && (speakerListner.Value.Item4 == 2) && (speakerListner.Value.Item2 == 2))
                                {
                                    foreach (KeyValuePair<int, Socket> listner in Variables.ListenerSocketDictionary)
                                    {
                                        if ((speakerListner.Value.Item3 == "" + IPAddress.Parse(((IPEndPoint)listner.Value.LocalEndPoint).Address.ToString())) &&
                                            (speakerListner.Value.Item1 == "" + IPAddress.Parse(((IPEndPoint)listner.Value.RemoteEndPoint).Address.ToString())))
                                        {
                                            bgpListener.SendSpeaker(updatePacket.BGPmessage, listner.Value, "Update");
                                            fsm.BGPUpdateMessageSent(Variables.True);
                                        }

                                    }

                             }
                                if ((adj_RIB_Out.Item4 == speakerListner.Value.Item1) && (speakerListner.Value.Item2 == 2) && (speakerListner.Value.Item4 == 2))
                                {
                                    foreach (KeyValuePair<int, Socket> speaker in Variables.SpeakerSocketDictionary)
                                    {
                                        if ((speakerListner.Value.Item1 == "" + IPAddress.Parse(((IPEndPoint)speaker.Value.LocalEndPoint).Address.ToString())) &&
                                            (speakerListner.Value.Item3 == "" + IPAddress.Parse(((IPEndPoint)speaker.Value.RemoteEndPoint).Address.ToString())))
                                        {

                                         if (!speakerListner.Value.Item3.Equals("127.2.0.5"))
                                            {
                                                count++;
                                                if (count < 2)
                                                {
                                                    bgpSpeaker.SendListener(updatePacket.BGPmessage, speaker.Value, "Update");
                                                    fsm.BGPUpdateMessageSent(Variables.True);
                                                }

                                            }
                                        }
                                    }
                                }

                            }

                            break;
                        case 3:
                            nlri = Variables.NLRI[entry.Key];
                            pathSegment = Variables.PathSegment[entry.Key];
                            pathAttribute = Variables.PathAttribute[entry.Key];
                            adj_RIB_Out = Variables.Adj_RIB_Out[entry.Key];

                            if (Variables.WithdrawnRoutes.ContainsKey(2))
                            {
                                Tuple<string, int> withdrawlInfo = Variables.WithdrawnRoutes[2];
                                Variables.Withdrawl_IP_Address = withdrawlInfo.Item1;
                                Variables.Withdrawl_Length = withdrawlInfo.Item2;

                            }
                            else
                            {
                                Variables.Withdrawl_IP_Address = "";
                                Variables.Withdrawl_Length = 0;
                            }

                            updatePacket = new Update((UInt16)Variables.Withdrawl_Length, Variables.Withdrawl_IP_Address, (ushort)adj_RIB_Out.Item7.Length,
                           adj_RIB_Out.Item7, 24, (UInt32)pathAttribute.Item1, (UInt32)pathAttribute.Item3, (ushort)pathAttribute.Item4, pathAttribute.Item2, 1,
                           (ushort)pathSegment.Item1, pathSegment.Item2, (ushort)nlri.Item1, nlri.Item2);
                            foreach (KeyValuePair<int, Tuple<string, ushort, string, ushort>> speakerListner in Variables.ConnectionSpeakerAs_ListenerAs)
                            {

                                if ((adj_RIB_Out.Item2 == speakerListner.Value.Item3) && (speakerListner.Value.Item4 == 2) && (speakerListner.Value.Item2 == 2))
                                {
                                    foreach (KeyValuePair<int, Socket> listner in Variables.ListenerSocketDictionary)
                                    {
                                        if ((speakerListner.Value.Item3 == "" + IPAddress.Parse(((IPEndPoint)listner.Value.LocalEndPoint).Address.ToString())) &&
                                            (speakerListner.Value.Item1 == "" + IPAddress.Parse(((IPEndPoint)listner.Value.RemoteEndPoint).Address.ToString())))
                                        {
                                            bgpListener.SendSpeaker(updatePacket.BGPmessage, listner.Value, "Update");
                                            fsm.BGPUpdateMessageSent(Variables.True);

                                        }
                                    }

                                    
                                }
                                if ((adj_RIB_Out.Item2 == speakerListner.Value.Item1) && (speakerListner.Value.Item2 == 2) && (speakerListner.Value.Item4 == 2))
                                {
                                    foreach (KeyValuePair<int, Socket> speaker in Variables.SpeakerSocketDictionary)
                                    {
                                        if ((speakerListner.Value.Item1 == "" + IPAddress.Parse(((IPEndPoint)speaker.Value.LocalEndPoint).Address.ToString())) &&
                                            (speakerListner.Value.Item3 == "" + IPAddress.Parse(((IPEndPoint)speaker.Value.RemoteEndPoint).Address.ToString())))
                                        {
                                            bgpSpeaker.SendListener(updatePacket.BGPmessage, speaker.Value, "Update");
                                            fsm.BGPUpdateMessageSent(Variables.True);

                                        }

                                    }
                                }


                            }
                            break;
                        case 4:
                            nlri = Variables.NLRI[entry.Key];
                            pathSegment = Variables.PathSegment[entry.Key];
                            pathAttribute = Variables.PathAttribute[entry.Key];
                            adj_RIB_Out = Variables.Adj_RIB_Out[entry.Key];

                            if (Variables.WithdrawnRoutes.ContainsKey(3))
                            {
                                Tuple<string, int> withdrawlInfo = Variables.WithdrawnRoutes[3];
                                Variables.Withdrawl_IP_Address = withdrawlInfo.Item1;
                                Variables.Withdrawl_Length = withdrawlInfo.Item2;
                            }
                            else
                            {
                                Variables.Withdrawl_IP_Address = "";
                                Variables.Withdrawl_Length = 0;
                            }

                            updatePacket = new Update((UInt16)Variables.Withdrawl_Length, Variables.Withdrawl_IP_Address, (ushort)adj_RIB_Out.Item7.Length,
                           adj_RIB_Out.Item7, 24, (UInt32)pathAttribute.Item1, (UInt32)pathAttribute.Item3, (ushort)pathAttribute.Item4, pathAttribute.Item2, 1,
                           (ushort)pathSegment.Item1, pathSegment.Item2, (ushort)nlri.Item1, nlri.Item2);
                            foreach (KeyValuePair<int, Tuple<string, ushort, string, ushort>> speakerListner in Variables.ConnectionSpeakerAs_ListenerAs)
                            {

                                if ((adj_RIB_Out.Item4 == speakerListner.Value.Item3) && (speakerListner.Value.Item4 == 3) && (speakerListner.Value.Item2 == 3))
                                {
                                    foreach (KeyValuePair<int, Socket> listner in Variables.ListenerSocketDictionary)
                                    {
                                        if ((speakerListner.Value.Item3 == "" + IPAddress.Parse(((IPEndPoint)listner.Value.LocalEndPoint).Address.ToString())) &&
                                            (speakerListner.Value.Item1 == "" + IPAddress.Parse(((IPEndPoint)listner.Value.RemoteEndPoint).Address.ToString())))
                                        {
                                            bgpListener.SendSpeaker(updatePacket.BGPmessage, listner.Value, "Update");
                                            fsm.BGPUpdateMessageSent(Variables.True);
                                        }

                                    }
                                }
                                if ((adj_RIB_Out.Item4 == speakerListner.Value.Item1) && (speakerListner.Value.Item4 == 3) && (speakerListner.Value.Item2 == 3))
                                {

                                    foreach (KeyValuePair<int, Socket> speaker in Variables.SpeakerSocketDictionary)
                                    {
                                        if ((speakerListner.Value.Item1 == "" + IPAddress.Parse(((IPEndPoint)speaker.Value.LocalEndPoint).Address.ToString())) &&
                                            (speakerListner.Value.Item3 == "" + IPAddress.Parse(((IPEndPoint)speaker.Value.RemoteEndPoint).Address.ToString())))
                                        {

                                            bgpSpeaker.SendListener(updatePacket.BGPmessage, speaker.Value, "Update");
                                            fsm.BGPUpdateMessageSent(Variables.True);
                                        }
                                    }
                                }
                            }

                            break;

                    }


                }
            }
        }
        public void sendNotifyMsg(int removedRouteN, int AS, string error)
        {
            Listener bgpListener = new Listener();
            Speaker bgpSpeaker = new Speaker();
            String adjRIBItem;
            Console.WriteLine("Route about to be removed: " + removedRouteN);
            /*Console.WriteLine("NLRI ennen kaatuilua:"+Variables.NLRI[removedRouteN]);
            //Tuple<int, string> nlri = Variables.NLRI[removedRouteN];
            Tuple<int, string> nlri = (5, "127.2");
            Console.WriteLine("NLRI: "+ nlri);*/
            //Tuple<int, string> pathSegment = Variables.PathSegment[removedRouteN];
            /*Tuple<int, string, int, ushort> pathAttribute = Variables.PathAttribute[removedRouteN];
            Tuple<int, string, int, string, int, int, string, Tuple<string>> adj_RIB_Out = Variables.Adj_RIB_Out[removedRouteN];

            if (removedRouteN < 4)
            {
                adjRIBItem = adj_RIB_Out.Item2;
                Console.WriteLine("item1: " + adj_RIB_Out.Item1);
                Console.WriteLine("item2: " + adj_RIB_Out.Item2);
                Console.WriteLine("item3: " + adj_RIB_Out.Item3);
                Console.WriteLine("item4: " + adj_RIB_Out.Item4);
                Console.WriteLine("item5: " + adj_RIB_Out.Item5);
                Console.WriteLine("item6: " + adj_RIB_Out.Item6);
                Console.WriteLine("item7: " + adj_RIB_Out.Item7);
            }
            else
            {
                adjRIBItem = adj_RIB_Out.Item4;
            }*/

            if (Variables.WithdrawnRoutes.ContainsKey(removedRouteN))
            {
                Tuple<string, int> withdrawlInfo = Variables.WithdrawnRoutes[removedRouteN];
                Variables.Withdrawl_IP_Address = withdrawlInfo.Item1;
                Variables.Withdrawl_Length = withdrawlInfo.Item2;
            }
            else
            {
                Variables.Withdrawl_IP_Address = "";
                Variables.Withdrawl_Length = 0;
            }
            Notification notifyPacket = new Notification(Variables.ErrorCode, Variables.ErrorSubCode, error);
            foreach (KeyValuePair<int, Tuple<string, ushort, string, ushort>> speakerListner in Variables.ConnectionSpeakerAs_ListenerAs)
            {
                if (/*(adjRIBItem == speakerListner.Value.Item3) && */(speakerListner.Value.Item2 == AS) && (speakerListner.Value.Item4 == AS))
                {
                    foreach (KeyValuePair<int, Socket> listner in Variables.ListenerSocketDictionary)
                    {
                        try
                        {
                            if ((speakerListner.Value.Item3 == "" + IPAddress.Parse(((IPEndPoint)listner.Value.LocalEndPoint).Address.ToString())) &&
                                (speakerListner.Value.Item1 == "" + IPAddress.Parse(((IPEndPoint)listner.Value.RemoteEndPoint).Address.ToString())))
                            {
                                bgpListener.SendSpeaker(notifyPacket.BGPmessage, listner.Value, "Notify");
                                fsm.BGPNotifyMessageSent(Variables.True);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }
                if (/*(adjRIBItem == speakerListner.Value.Item1) && */(speakerListner.Value.Item2 == AS) && (speakerListner.Value.Item4 == AS))
                {
                    foreach (KeyValuePair<int, Socket> speaker in Variables.SpeakerSocketDictionary)
                    {
                        try
                        {
                            if ((speakerListner.Value.Item1 == "" + IPAddress.Parse(((IPEndPoint)speaker.Value.LocalEndPoint).Address.ToString())) &&
                                (speakerListner.Value.Item3 == "" + IPAddress.Parse(((IPEndPoint)speaker.Value.RemoteEndPoint).Address.ToString())))
                            {
                                bgpSpeaker.SendListener(notifyPacket.BGPmessage, speaker.Value, "Notify");
                                fsm.BGPNotifyMessageSent(Variables.True);
                            }


                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }
            }
        }



    }
}
