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
        FSM fsm = new FSM();
        int count = 0;

        public void adj_RIB_Out()
        {
            int i = 0;
            Variables.Adj_RIB_Out.Clear();
            Variables.InterAS_IP.Clear();
            foreach (DataRow row in Variables.Data.Rows)
            {
                if (row.Field<int>(2) == 1 || row.Field<int>(4) == 1)
                {
                    if (row.Field<int>(5) == 1)
                    {
                        i++;
                        Tuple<string> neighbourAS = new Tuple<string>(Variables.PrefixAS2);
                        //Tuple consists of connection count, network, N_AS, Next_Hop, NH_AS, EGP/IGP,Local AS_PREFIX, Neighbour AS_prefix

                        Tuple<int, string, int, string, int, int, string, Tuple<string>> Adj_RIB_Out =
                                new Tuple<int, string, int, string, int, int, string, Tuple<string>>(row.Field<int>(0), row.Field<string>(1), row.Field<int>(2),
                        row.Field<string>(3), row.Field<int>(4), row.Field<int>(5), Variables.PrefixAS1, neighbourAS);
                        Variables.Adj_RIB_Out.Add(i, Adj_RIB_Out);
                        //storing inter AS ip
                        Variables.InterAS_IP.Add(i, row.Field<string>(1));
                        // ... Write value of first field as integer.
                       
                    }
                }

                // Local policy to find adj_RIB_Out AS2
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

                        //storing inter AS IP
                        Variables.InterAS_IP.Add(i, row.Field<string>(3));
                        // ... Write value of first field as integer.
                                            
                    }
                }
                // Local policy to find adj_RIB_Out AS3
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
                        //storing inter AS ip
                        Variables.InterAS_IP.Add(i, row.Field<string>(1));
                        Variables.Adj_RIB_Out.Add(i, Adj_RIB_Out);
                        // ... Write value of first field as integer.

                    }
                }

            }

        }

        //pathAttribute is the combination of attribute(origin), attribute length, attrFlag and attrTypeCode
        public void pathAttribute()
        {
            Variables.PathAttribute.Clear();
            int i = 0;
            foreach (KeyValuePair<int, Tuple<int, string, int, string, int, int, string, Tuple<string>>> entry in GlobalVariables.Adj_RIB_Out)
            {
                i++;
                Tuple<int, string, int, ushort> pathAttribute = new Tuple<int, string, int, ushort>(entry.Value.Item2.Length, entry.Value.Item2, entry.Value.Item6, (ushort)entry.Value.Item6);
                Variables.PathAttribute.Add(i, pathAttribute);
                // Console.WriteLine("conection count: "+entry.Value.Item1 + " Network: " + entry.Value.Item2 + " N_AS: "+ entry.Value.Item3 + " NEXT_HOP: "+ entry.Value.Item4 
                //   +" NH_AS: "+ entry.Value.Item5 + " IGP/EGP: "+ entry.Value.Item6 + " nlrPrefix: "+ entry.Value.Item7);
                // do something with entry.Value or entry.Key
            }

        }
        public void networkLayerReachibility()
        {
            Variables.NLRI.Clear();
            int i = 0;
            foreach (KeyValuePair<int, Tuple<int, string, int, string, int, int, string, Tuple<string>>> entry in GlobalVariables.Adj_RIB_Out)
            {
                i++;
                Tuple<int, string> nlri = new Tuple<int, string>(entry.Value.Rest.Item1.Length, entry.Value.Rest.Item1);
                Variables.NLRI.Add(i, nlri);
                // Console.WriteLine(" Prefix Length: " + entry.Value.Rest.Item1.Length + " Prifix: " + entry.Value.Rest.Item1);
            }
        }
        public void pathSegment()
        {
            Variables.PathSegment.Clear();
            int i = 0;
            foreach (KeyValuePair<int, Tuple<int, string, int, string, int, int, string, Tuple<string>>> entry in GlobalVariables.Adj_RIB_Out)
            {
                i++;
                Tuple<int, string> pathSegment = new Tuple<int, string>(1, "" + entry.Value.Item3 + "" + entry.Value.Item5);
                Variables.PathSegment.Add(i, pathSegment);
                // Console.WriteLine("PathSegment: " + "" + entry.Value.Item3 + "" + entry.Value.Item5);
            }
        }



        public void sendUpdateMsg(int i)
        {
            Listener bgpListner = new Listener();
            Speaker bgpSpeaker = new Speaker();

            foreach (KeyValuePair<int, Tuple<int, string, int, string, int, int, string, Tuple<string>>> entry in Variables.Adj_RIB_Out)
            {
                //switch (entry.Key)
                if (i == entry.Key)
                {
                    switch (i)
                    {
                        case 1:
                            Tuple<int, string> nlri = Variables.NLRI[entry.Key];
                            Tuple<int, string> pathSegment = Variables.PathSegment[entry.Key];
                            //pathAttribute is the combination of attribute length, attribute(origin), attrFlag and attrTypeCode
                            Tuple<int, string, int, ushort> pathAttribute = Variables.PathAttribute[entry.Key];
                            //Tuple consists of connection count, network, N_AS, Next_Hop, NH_AS, EGP/IGP, AS_prefix
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
                                                //  Console.WriteLine("Listner IP: {0}| Speaker IP: {1}", IPAddress.Parse(((IPEndPoint)listner1.Value.LocalEndPoint).Address.ToString()),
                                                // IPAddress.Parse(((IPEndPoint)listner1.Value.RemoteEndPoint).Address.ToString()));

                                                bgpListner.SendSpeaker(updatePacket.BGPmessage, listner.Value, "Update");
                                                FSM.BGPUpdateMsgSent(Variables.True);
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
                                                //Console.WriteLine("Speaker IP: {0}| Listner IP: {1}", IPAddress.Parse(((IPEndPoint)speaker.Value.LocalEndPoint).Address.ToString()),
                                                //  IPAddress.Parse(((IPEndPoint)speaker.Value.RemoteEndPoint).Address.ToString()));
                                                bgpSpeaker.SendListener(updatePacket.BGPmessage, speaker.Value, "Update");
                                                FSM.BGPUpdateMsgSent(Variables.True);
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
                            //pathAttribute is the combination of attribute length, attribute(origin), attrFlag and attrTypeCode
                            pathAttribute = Variables.PathAttribute[entry.Key];
                            //Tuple consists of connection count, network, N_AS, Next_Hop, NH_AS, EGP/IGP, AS_prefix
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
                            updatePacket = new UpdateMessage ((UInt16)Variables.Withdrawl_Length, Variables.Withdrawl_IP_Address, (ushort)adj_RIB_Out.Item7.Length,
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
                                            // Socket listnerSocket = listner.Value;
                                            //   Console.WriteLine("Listner IP: {0}| Speaker IP: {1}", IPAddress.Parse(((IPEndPoint)listner.Value.LocalEndPoint).Address.ToString()),
                                            // IPAddress.Parse(((IPEndPoint)listner.Value.RemoteEndPoint).Address.ToString()));

                                            bgpListner.SendSpeaker(updatePacket.BGPmessage, listner.Value, "Update");
                                            FSM.BGPUpdateMsgSent(Variables.True);
                                        }

                                    }

                                    /**
                                    //Console.WriteLine("This is my Connection of Speaker: " + speakerListner.Key+ "AS value: "+ speakerListner.Value.Item4);
                                    Socket listnerSocket = GlobalVariables.listnerSocket_Dictionary[speakerListner.Key];
                                    BGPListner bgpListner = new BGPListner();
                                    //bgpListner.SendSpeaker(updatePacket.BGPmessage, listnerSocket, "Update");
                                    **/
                                }
                                if ((adj_RIB_Out.Item4 == speakerListner.Value.Item1) && (speakerListner.Value.Item2 == 2) && (speakerListner.Value.Item4 == 2))
                                {
                                    foreach (KeyValuePair<int, Socket> speaker in Variables.SpeakerSocketDictionary)
                                    {
                                        if ((speakerListner.Value.Item1 == "" + IPAddress.Parse(((IPEndPoint)speaker.Value.LocalEndPoint).Address.ToString())) &&
                                            (speakerListner.Value.Item3 == "" + IPAddress.Parse(((IPEndPoint)speaker.Value.RemoteEndPoint).Address.ToString())))
                                        {

                                            //Socket speakerSocket = speaker.Value;
                                            //Console.WriteLine("Speaker IP: {0}| Listner IP: {1}", IPAddress.Parse(((IPEndPoint)speaker.Value.LocalEndPoint).Address.ToString()),
                                            //IPAddress.Parse(((IPEndPoint)speaker.Value.RemoteEndPoint).Address.ToString()));

                                            if (!speakerListner.Value.Item3.Equals("127.2.0.5"))
                                            {
                                                count++;
                                                if (count < 2)
                                                {
                                                    bgpSpeaker.SendListener(updatePacket.BGPmessage, speaker.Value, "Update");
                                                    FSM.BGPUpdateMsgSent(Variables.True);
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
                            //pathAttribute is the combination of attribute length, attribute(origin), attrFlag and attrTypeCode
                            pathAttribute = Variables.PathAttribute[entry.Key];
                            //Tuple consists of connection count, network, N_AS, Next_Hop, NH_AS, EGP/IGP, AS_prefix
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

                            updatePacket = new UpdateMessage((UInt16)Variables.Withdrawl_Length, Variables.Withdrawl_IP_Address, (ushort)adj_RIB_Out.Item7.Length,
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
                                            //Socket listnerSocket = listner.Value;
                                            //Console.WriteLine("Listner IP: {0}| Speaker IP: {1}", IPAddress.Parse(((IPEndPoint)listner.Value.LocalEndPoint).Address.ToString()),
                                            //   IPAddress.Parse(((IPEndPoint)listner.Value.RemoteEndPoint).Address.ToString()));
                                            //BGPListner bgpListner = new BGPListner();
                                            bgpListner.SendSpeaker(updatePacket.BGPmessage, listner.Value, "Update");
                                            FSM.BGPUpdateMsgSent(Variables.True);

                                        }
                                    }

                                    //Console.WriteLine("This is my Connection of Speaker: " + speakerListner.Key + "AS value: " + speakerListner.Value.Item4);
                                    //Socket listnerSocket = GlobalVariables.listnerSocket_Dictionary[speakerListner.Key];

                                }
                                if ((adj_RIB_Out.Item2 == speakerListner.Value.Item1) && (speakerListner.Value.Item2 == 2) && (speakerListner.Value.Item4 == 2))
                                {
                                    foreach (KeyValuePair<int, Socket> speaker in Variables.SpeakerSocketDictionary)
                                    {
                                        if ((speakerListner.Value.Item1 == "" + IPAddress.Parse(((IPEndPoint)speaker.Value.LocalEndPoint).Address.ToString())) &&
                                            (speakerListner.Value.Item3 == "" + IPAddress.Parse(((IPEndPoint)speaker.Value.RemoteEndPoint).Address.ToString())))
                                        {
                                            //Socket speakerSocket = speaker.Value;
                                            //Console.WriteLine("Speaker IP: {0}| Listner IP: {1}", IPAddress.Parse(((IPEndPoint)speaker.Value.LocalEndPoint).Address.ToString()),
                                            //  IPAddress.Parse(((IPEndPoint)speaker.Value.RemoteEndPoint).Address.ToString()));
                                            //BGPSpeaker bgpSpeaker = new BGPSpeaker();

                                            // if (!speakerListner.Value.Item3.Equals("127.2.0.4"))
                                            // {
                                            bgpSpeaker.SendListener(updatePacket.BGPmessage, speaker.Value, "Update");
                                            FSM.BGPUpdateMsgSent(Variables.True);

                                            // }    
                                        }

                                    }
                                    //Console.WriteLine("This is my Connection of Speaker: " + speakerListner.Key);
                                    //Socket speakerSocket = GlobalVariables.SpeakerSocket_Dictionary[speakerListner.Key];

                                }


                            }
                            break;
                        case 4:
                            nlri = Variables.NLRI[entry.Key];
                            pathSegment = Variables.PathSegment[entry.Key];
                            //pathAttribute is the combination of attribute length, attribute(origin), attrFlag and attrTypeCode
                            pathAttribute = Variables.PathAttribute[entry.Key];
                            //Tuple consists of connection count, network, N_AS, Next_Hop, NH_AS, EGP/IGP, AS_prefix
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

                            updatePacket = new UpdateMessage((UInt16)Variables.Withdrawl_Length, Variables.Withdrawl_IP_Address, (ushort)adj_RIB_Out.Item7.Length,
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
                                            //Console.WriteLine("Listner IP: {0}| Speaker IP: {1}", IPAddress.Parse(((IPEndPoint)listner.Value.LocalEndPoint).Address.ToString()),
                                            // IPAddress.Parse(((IPEndPoint)listner.Value.RemoteEndPoint).Address.ToString()));

                                            bgpListner.SendSpeaker(updatePacket.BGPmessage, listner.Value, "Update");
                                            FSM.BGPUpdateMsgSent(Variables.True);
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

                                            //Socket speakerSocket = speaker.Value;
                                            //Console.WriteLine("Speaker IP: {0}| Listner IP: {1}", IPAddress.Parse(((IPEndPoint)speaker.Value.LocalEndPoint).Address.ToString()),
                                            // IPAddress.Parse(((IPEndPoint)speaker.Value.RemoteEndPoint).Address.ToString()));

                                            bgpSpeaker.SendListener(updatePacket.BGPmessage, speaker.Value, "Update");
                                            FSM.BGPUpdateMsgSent(Variables.True);
                                        }
                                    }
                                }
                            }

                            break;

                    }


                }
            }
        }
        public void sendNotifyMsg(int adj, int AS, string error)
        {
            Listener bgpListner = new Listener();
            Speaker bgpSpeaker = new Speaker();
            String adjRIBItem;
            Tuple<int, string> nlri = Variables.NLRI[adj];
            Tuple<int, string> pathSegment = Variables.PathSegment[adj];
            //pathAttribute is the combination of attribute length, attribute(origin), attrFlag and attrTypeCode
            Tuple<int, string, int, ushort> pathAttribute = Variables.PathAttribute[adj];
            //Tuple consists of connection count, network, N_AS, Next_Hop, NH_AS, EGP/IGP, AS_prefix
            Tuple<int, string, int, string, int, int, string, Tuple<string>> adj_RIB_Out = Variables.Adj_RIB_Out[adj];

            if (adj < 4)
            {
                adjRIBItem = adj_RIB_Out.Item2;
            }
            else
            {
                adjRIBItem = adj_RIB_Out.Item4;
            }

            if (Variables.WithdrawnRoutes.ContainsKey(adj))
            {
                Tuple<string, int> withdrawlInfo = Variables.WithdrawnRoutes[adj];
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
                if ((adjRIBItem == speakerListner.Value.Item3) && (speakerListner.Value.Item2 == AS) && (speakerListner.Value.Item4 == AS))
                {
                    foreach (KeyValuePair<int, Socket> listner in Variables.ListenerSocketDictionary)
                    {
                        try
                        {
                            if ((speakerListner.Value.Item3 == "" + IPAddress.Parse(((IPEndPoint)listner.Value.LocalEndPoint).Address.ToString())) &&
                                (speakerListner.Value.Item1 == "" + IPAddress.Parse(((IPEndPoint)listner.Value.RemoteEndPoint).Address.ToString())))
                            {
                                //  Console.WriteLine("Listner IP: {0}| Speaker IP: {1}", IPAddress.Parse(((IPEndPoint)listner1.Value.LocalEndPoint).Address.ToString()),
                                // IPAddress.Parse(((IPEndPoint)listner1.Value.RemoteEndPoint).Address.ToString()));

                                bgpListner.SendSpeaker(notifyPacket.BGPmessage, listner.Value, "Notify");
                                FSM.BGPNotifyMsgSent(Variables.True);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }
                if ((adjRIBItem == speakerListner.Value.Item1) && (speakerListner.Value.Item2 == AS) && (speakerListner.Value.Item4 == AS))
                {
                    foreach (KeyValuePair<int, Socket> speaker in Variables.SpeakerSocketDictionary)
                    {
                        try
                        {
                            if ((speakerListner.Value.Item1 == "" + IPAddress.Parse(((IPEndPoint)speaker.Value.LocalEndPoint).Address.ToString())) &&
                                (speakerListner.Value.Item3 == "" + IPAddress.Parse(((IPEndPoint)speaker.Value.RemoteEndPoint).Address.ToString())))
                            {
                                //Console.WriteLine("Speaker IP: {0}| Listner IP: {1}", IPAddress.Parse(((IPEndPoint)speaker.Value.LocalEndPoint).Address.ToString()),
                                //  IPAddress.Parse(((IPEndPoint)speaker.Value.RemoteEndPoint).Address.ToString()));
                                bgpSpeaker.SendListener(notifyPacket.BGPmessage, speaker.Value, "Notify");
                                FSM.BGPNotifyMsgSent(Variables.True);
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
