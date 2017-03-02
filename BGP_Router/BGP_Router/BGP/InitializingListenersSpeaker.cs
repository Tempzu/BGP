using System;
using BGPSimulator.BGPMessage;
using System.Net.Sockets;
using BGPSimulator.FSM;
using System.Net;
using System.Threading;

namespace BGP_Router.BGP
{
    public class InitilizeBGPListenerSpeaker
    {
        public static BGPListener[] bgpListener = new BGPListener[10];
        public static BGPSpeaker[] bgpSpeaker = new BGPSpeaker[15];

        private static AutoResetEvent speakerConnectionRequest = new AutoResetEvent(true);

        //to create 13 speaker for 10 routers
        int m = 0;
        //to create 14 different connection
        int n = 0;
        public ushort AS;

        //to create variables to help in splitting routers into 3 AS
        int as1Last;
        int as2First;
        int as2Last;
        int as3First;
        string temp;

        public void StartListener()
        {
            Console.WriteLine("There is a total of 10 routers which will be split into 3 AS. How many routers do you want in AS1? (1-8)");
            temp = Console.ReadLine();
            try
            {
                as1Last = int.Parse(temp) - 1; //-1 since routers start from 0 
                if (as1Last < 0)
                {
                    Console.WriteLine("The character you typed wasn't in the given bounds, so we set 1 routers in the AS1.");
                    as1Last = 0;
                }
                else if (as1Last > 7)
                {
                    Console.WriteLine("The character you typed wasn't in the given bounds, so we set 8 routers in the AS1.");
                    as1Last = 7;
                }
            }
            catch
            {
                Console.WriteLine("The character you typed wasn't a number, so we set 3 routers to AS1.");
                as1Last = 2;
            }
            if (as1Last < 7)
            {
                as2First = as1Last + 1;
                if (as1Last > 0)
                {
                    Console.WriteLine("How many routers do you want in AS2? (1-" + (8 - as1Last) + "). Rest will be placed AS3.");
                }
                else
                {
                    Console.WriteLine("How many routers do you want in AS2? (1-8). Rest will be placed AS3.");
                }
                temp = Console.ReadLine();
                try
                {
                    as2Last = as1Last + int.Parse(temp);
                    if (as2Last <= as1Last)
                    {
                        Console.WriteLine("The character you typed wasn't in the given bounds, so we set 1 routers in the AS2.");
                        as2Last = as1Last + 1;
                    }
                    else if (as2Last > 8)
                    {
                        Console.WriteLine("The character you typed wasn't in the given bounds, so we set 1 router in the AS3 and rest in AS2.");
                        as2Last = 8;
                    }
                }
                catch
                {
                    Console.WriteLine("The character you typed wasn't a number, so we set 4 routers to AS2.");
                    as2Last = as1Last + 4; ;
                }
                as3First = as2Last + 1;
            }
            else
            {
                Console.WriteLine("One router will be now placed in both remaining AS.");
                as2First = 8;
                as2Last = as2First;
                as3First = 9;
            }


            for (int i = 0; i < 10; i++)
            {

                bgpListener[i] = new BGPListener();
                if (i < as2First)
                {
                    AS = Variables.AS1;
                    Variables.ListenerConnectionAndAS.TryAdd((ushort)i, AS);
                    bgpListener[i].BindListener(Variables.As1_IP_Prefix + i, Variables.ListenerPortNumber, i);
                    Variables.ListenerAS.TryAdd(Variables.As1_IP_Prefix + i, AS);


                }
                else if (i > as1Last && i < as3First)
                {
                    AS = Variables.AS2;
                    Variables.ListenerConnectionAndAS.TryAdd((ushort)i, AS);
                    bgpListener[i].BindListener(Variables.As2_IP_Prefix + i, Variables.ListenerPortNumber, i);
                    Variables.ListenerAS.TryAdd(Variables.As2_IP_Prefix + i, AS);

                }
                else if (i > as2First && i < 10)
                {
                    AS = Variables.AS3;
                    Variables.ListenerConnectionAndAS.TryAdd((ushort)i, AS);
                    bgpListener[i].BindListener(Variables.As3_IP_Prefix + i, Variables.ListenerPortNumber, i);
                    Variables.ListenerAS.TryAdd(Variables.As3_IP_Prefix + i, AS);

                }

                Thread.Sleep(500);
                //recient computers can handle 500 connections
            }


        }


        public void StartListening()
        {


            for (int i = 0; i < 10; i++)
            {
                bgpListener[i].Listen(10);
                bgpListener[i].Accept();
            }


        }
        public void StartSpeaker()
        {
            for (int k = 0; k < 10; k++)
            {


                bgpSpeaker[m] = new BGPSpeaker();

                if (k < as2First)
                {
                    AS = Variables.AS1;
                    if (k == as1Last)
                    {

                        bgpSpeaker[m].BindSpeaker(Variables.As1_IP_Prefix + k, Variables.SpeakerPortNumber, m);
                        Variables.SpeakerAS.TryAdd(Variables.As1_IP_Prefix + k, AS);

                        m++;
                        bgpSpeaker[m] = new BGPSpeaker();
                        bgpSpeaker[m].BindSpeaker(Variables.As1_IP_Prefix + k, Variables.SpeakerPortNumber + 1, m);

                    }
                    else if (k < as1Last)
                    {

                        bgpSpeaker[m].BindSpeaker(Variables.As1_IP_Prefix + k, Variables.SpeakerPortNumber, m);
                        Variables.SpeakerAS.TryAdd(Variables.As1_IP_Prefix + k, AS);

                    }

                }
                else if (k > as1Last && k < as3First)
                {
                    AS = Variables.AS2;
                    if (k == as2First)
                    {
                        bgpSpeaker[m].BindSpeaker(Variables.As2_IP_Prefix + k, Variables.SpeakerPortNumber, m);
                        Variables.SpeakerAS.TryAdd(Variables.As2_IP_Prefix + k, AS);
                        m++;
                        bgpSpeaker[m] = new BGPSpeaker();
                        bgpSpeaker[m].BindSpeaker(Variables.As2_IP_Prefix + k, Variables.SpeakerPortNumber + 1, m);
                    }
                    else if (k == as2Last)
                    {
                        bgpSpeaker[m].BindSpeaker(Variables.As2_IP_Prefix + k, Variables.SpeakerPortNumber, m);
                        Variables.SpeakerAS.TryAdd(Variables.As2_IP_Prefix + k, AS);
                        m++;
                        bgpSpeaker[m] = new BGPSpeaker();
                        bgpSpeaker[m].BindSpeaker(Variables.As2_IP_Prefix + k, Variables.SpeakerPortNumber + 1, m);
                        m++;
                        bgpSpeaker[m] = new BGPSpeaker();
                        bgpSpeaker[m].BindSpeaker(Variables.As2_IP_Prefix + k, Variables.SpeakerPortNumber + 2, m);
                    }
                    else if (as2First < k && k < as2Last)
                    {
                        bgpSpeaker[m].BindSpeaker(Variables.As2_IP_Prefix + k, Variables.SpeakerPortNumber, m);
                        Variables.SpeakerAS.TryAdd(Variables.As2_IP_Prefix + k, AS);
                    }


                }
                else if (k > as2Last && k < 9)
                {
                    AS = Variables.AS3;

                    bgpSpeaker[m].BindSpeaker(Variables.As3_IP_Prefix + k, Variables.SpeakerPortNumber, m);

                    Variables.SpeakerAS.TryAdd(Variables.As3_IP_Prefix + k, AS);


                }
                else if (k == 9)
                {
                    AS = Variables.AS3;
                    bgpSpeaker[m].BindSpeaker(Variables.As3_IP_Prefix + k, Variables.SpeakerPortNumber, m);
                    Variables.SpeakerAS.TryAdd(Variables.As3_IP_Prefix + k, AS);
                    m++;
                    bgpSpeaker[m] = new BGPSpeaker();
                    bgpSpeaker[m].BindSpeaker(Variables.As3_IP_Prefix + k, Variables.SpeakerPortNumber + 1, m);

                }

                //bgpSpeaker[k].Bind("127.1.0.1", 179, m);
                m++;

                Thread.Sleep(500);

            }

        }
        public void SpeakerConnection_Init()
        {
            for (int k = 0; k < 10; k++)
            {

                //creating speakers to AS1
                if (k < as2First)
                {

                    Variables.SpeakerConnectionAndAS.TryAdd((ushort)n, Variables.AS1);
                    Variables.ListenerConnectionCount = n;
                    if (k == as1Last) //creating connection which connects to the first router of AS2
                    {
                        bgpSpeaker[n].Connect(Variables.As2_IP_Prefix + (k + 1), Variables.ListenerPortNumber, k, k + 1);

                        Variables.ConnectionAndListener.TryAdd(n, Variables.As2_IP_Prefix + (k + 1));
                        Variables.ConnectionAndSpeaker.TryAdd(n, Variables.As1_IP_Prefix + k);
                        SendOpenMessageToListener(n);

                        if (1 < as1Last) //more connections are created to AS1 depending on the size of AS1
                        {
                            n++;
                            Variables.SpeakerConnectionAndAS.TryAdd((ushort)n, Variables.AS1);
                            bgpSpeaker[n].Connect(Variables.As1_IP_Prefix + (k - 2), Variables.ListenerPortNumber, k, k - 2);
                            Variables.ConnectionAndListener.TryAdd(n, Variables.As1_IP_Prefix + (k - 2));
                            Variables.ConnectionAndSpeaker.TryAdd(n, Variables.As1_IP_Prefix + k);
                            SendOpenMessageToListener(n);
                        }

                    }
                    else if (0 < as1Last) //creating normal connections to AS1 (n -> n+1 )
                    {
                        bgpSpeaker[n].Connect(Variables.As1_IP_Prefix + (k + 1), Variables.ListenerPortNumber, k, k + 1);
                        Variables.ConnectionAndListener.TryAdd(n, Variables.As1_IP_Prefix + (k + 1));
                        Variables.ConnectionAndSpeaker.TryAdd(n, Variables.As1_IP_Prefix + k);
                        SendOpenMessageToListener(n);

                    }



                }
                else if (k > as1Last && k < as3First) //creating connections to AS2
                {

                    Variables.SpeakerConnectionAndAS.TryAdd((ushort)n, Variables.AS2);
                    Variables.ListenerConnectionCount = n;
                    if (k == as2First && (as2Last - as2First) > 0) //more connections are created to AS2 depending on the size of AS2
                    {
                        bgpSpeaker[n].Connect(Variables.As2_IP_Prefix + (k + 1), Variables.ListenerPortNumber, k, k + 1);
                        Variables.ConnectionAndListener.TryAdd(n, Variables.As2_IP_Prefix + (k + 1));
                        Variables.ConnectionAndSpeaker.TryAdd(n, Variables.As2_IP_Prefix + k);
                        SendOpenMessageToListener(n);
                        if (k == as2First && (as2Last - as2First) > 1)
                        {
                            n++;
                            Variables.SpeakerConnectionAndAS.TryAdd((ushort)n, Variables.AS2);
                            bgpSpeaker[n].Connect(Variables.As2_IP_Prefix + (k + 2), Variables.ListenerPortNumber, k, k + 2);
                            Variables.ConnectionAndListener.TryAdd(n, Variables.As2_IP_Prefix + (k + 2));
                            Variables.ConnectionAndSpeaker.TryAdd(n, Variables.As2_IP_Prefix + k);
                            SendOpenMessageToListener(n);
                        }
                    }

                    else if (k == as2Last) //creating connection which connects to the first router of AS3
                    {
                        bgpSpeaker[n].Connect(Variables.As3_IP_Prefix + (k + 1), Variables.ListenerPortNumber, k, k + 1);
                        Variables.ConnectionAndListener.TryAdd(n, Variables.As3_IP_Prefix + (k + 1));
                        Variables.ConnectionAndSpeaker.TryAdd(n, Variables.As2_IP_Prefix + k);
                        SendOpenMessageToListener(n);


                        if ((as2Last - as2First) > 1) //more connections are created to AS2 depending on the size of AS2
                        {
                            n++;
                            Variables.SpeakerConnectionAndAS.TryAdd((ushort)n, Variables.AS2);
                            bgpSpeaker[n].Connect(Variables.As2_IP_Prefix + (k - 2), Variables.ListenerPortNumber, k, k - 2);
                            Variables.ConnectionAndListener.TryAdd(n, Variables.As2_IP_Prefix + (k - 2));
                            Variables.ConnectionAndSpeaker.TryAdd(n, Variables.As2_IP_Prefix + k);
                            SendOpenMessageToListener(n);
                            if ((as2Last - as2First) > 2)
                            {
                                n++;
                                Variables.SpeakerConnectionAndAS.TryAdd((ushort)n, Variables.AS2);
                                bgpSpeaker[n].Connect(Variables.As2_IP_Prefix + (k - 3), Variables.ListenerPortNumber, k, k - 3);
                                Variables.ConnectionAndListener.TryAdd(n, Variables.As2_IP_Prefix + (k - 3));
                                Variables.ConnectionAndSpeaker.TryAdd(n, Variables.As2_IP_Prefix + k);
                                SendOpenMessageToListener(n);
                            }
                        }

                    }
                    else if (as2First < k && k < as2Last) //creating normal connections to AS2 (n -> n+1 )
                    {
                        bgpSpeaker[n].Connect(Variables.As2_IP_Prefix + (k + 1), Variables.ListenerPortNumber, k, k + 1);

                        Variables.ConnectionAndListener.TryAdd(n, Variables.As2_IP_Prefix + (k + 1));
                        Variables.ConnectionAndSpeaker.TryAdd(n, Variables.As2_IP_Prefix + k);
                        SendOpenMessageToListener(n);
                    }




                }

                else if (k > as2Last && k < 9)
                {

                    Variables.SpeakerConnectionAndAS.TryAdd((ushort)n, Variables.AS3);
                    Variables.ListenerConnectionCount = n;
                    //creating normal connections to AS3 (n -> n+1 )
                    bgpSpeaker[n].Connect(Variables.As3_IP_Prefix + (k + 1), Variables.ListenerPortNumber, k, k + 1);
                    Variables.ConnectionAndListener.TryAdd(n, Variables.As3_IP_Prefix + (k + 1));
                    Variables.ConnectionAndSpeaker.TryAdd(n, Variables.As3_IP_Prefix + k);
                    SendOpenMessageToListener(n);

                }
                else if (k == 9) //creating connection to the first router of AS1
                {
                    Variables.SpeakerConnectionAndAS.TryAdd((ushort)n, Variables.AS3);
                    bgpSpeaker[n].Connect(Variables.As1_IP_Prefix + (k - 9), Variables.ListenerPortNumber, k, k - 9);
                    Variables.ConnectionAndListener.TryAdd(n, Variables.As1_IP_Prefix + (k - 9));
                    Variables.ConnectionAndSpeaker.TryAdd(n, Variables.As3_IP_Prefix + k);
                    SendOpenMessageToListener(n);
                    if ((k - (as2Last + 1)) > 1)//more connections are created to AS3 depending on the size of AS3
                    {
                        n++;
                        Variables.SpeakerConnectionAndAS.TryAdd((ushort)n, Variables.AS3);
                        bgpSpeaker[n].Connect(Variables.As3_IP_Prefix + (k - 2), Variables.ListenerPortNumber, k, k - 2);
                        Variables.ConnectionAndListener.TryAdd(n, Variables.As3_IP_Prefix + (k - 2));
                        Variables.ConnectionAndSpeaker.TryAdd(n, Variables.As3_IP_Prefix + k);
                        SendOpenMessageToListener(n);
                    }
                }


                n++;

            }


        }
        public void SendOpenMessageToListener(int k)
        {
            if (Variables.True)
            {

                Variables.SpeakerConnectionCount = k;

                //Console.WriteLine("*********** SPEAKER NUMBER**************** : " + k);

                //OpenMessage(ushort type, ushort version,ushort myAS, ushort holdTime, string bgpIdentifier, ushort optimalParLength)
                OpenMessage openPacket = new OpenMessage(Variables.BGPVersion, Variables.SpeakerConnectionAndAS[(ushort)k], Variables.HoldTime,
                    Variables.ConnectionAndSpeaker[k], Variables.OptimalParameterLength);

                bgpSpeaker[k].Send(openPacket.BGPmessage);

            }
        }
    }
}

