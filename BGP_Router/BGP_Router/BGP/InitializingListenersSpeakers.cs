using System;
using BGP_Router.Messages;
using System.Threading;


/* In this class routers are created to the autonomous systems according to the user's allocation wishes.*/

namespace BGP_Router.BGP
{
    public class InitializingListenersSpeakers
    {
        public static Listener[] BGPListener = new Listener[10];
        public static Speaker[] BGPSpeaker = new Speaker[15];

        private static AutoResetEvent speakerConnectionRequest = new AutoResetEvent(true);

        //counters for listeners and speakers
        int m = 0;
        int n = 0;
        
        public ushort AS;
        string temp;

        public void StartListener() // user can configure the autonomous systems
        {
            Console.WriteLine("There is a total of 10 routers which will be split into 3 AS. How many routers do you want in AS1? (1-8)");
            temp = Console.ReadLine();
            try
            {
                as1Last = int.Parse(temp) - 1; //-1 since routers start from 0 
                if (as1Last < 0)
                {
                    Console.WriteLine("The character you typed wasn't in the given bounds, so we set 1 router in AS1.");
                    as1Last = 0;
                }
                else if (as1Last > 7)
                {
                    Console.WriteLine("The character you typed wasn't in the given bounds, so we set 8 routers in AS1.");
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
                    Console.WriteLine("How many routers do you want in AS2? (1-" + (8 - as1Last) + "). Rest will be placed in AS3.");
                }
                else
                {
                    Console.WriteLine("How many routers do you want in AS2? (1-8). Rest will be placed in AS3.");
                }
                temp = Console.ReadLine();
                try
                {
                    as2Last = as1Last + int.Parse(temp);
                    if (as2Last <= as1Last)
                    {
                        Console.WriteLine("The character you typed wasn't in the given bounds, so we set 1 router in AS2.");
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
                    Console.WriteLine("The character you typed wasn't a number, so we set " + (8 - Variables.as1Last) + " routers to AS2.");
                    Variables.as2Last = Variables.as1Last + (8 - Variables.as1Last);
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
            Console.WriteLine("\nConnections will be now created. Below you can see available commands.\n");
            Console.WriteLine("Type 'update' to enforce local policy. This must be done in order to update routing tables.");
            Console.WriteLine("Type 'as1', 'as2' or 'as3' to see routing tables.");
            Console.WriteLine("Type 'remove' to close a router.");
            Console.WriteLine("Type 'help' for commands.");
            Thread.Sleep(3000);

            // Now that the user has configured the autonomous systems, we can create the listener routers to the according ASs.
            Console.Write("\n----Setting up routers to the autonomous systems----\n");
            for (int i = 0; i < 10; i++)
            {

                BGPListener[i] = new Listener();
                if (i < as2First)
                {
                    AS = Variables.AS1;
                    Variables.ListenerConnectionAndAS.TryAdd((ushort)i, AS);
                    BGPListener[i].BindListener(Variables.As1_IP_Prefix + i, Variables.ListenerPortNumber, i);
                    Variables.ListenerAS.TryAdd(Variables.As1_IP_Prefix + i, AS);


                }
                else if (i > as1Last && i < as3First)
                {
                    AS = Variables.AS2;
                    Variables.ListenerConnectionAndAS.TryAdd((ushort)i, AS);
                    BGPListener[i].BindListener(Variables.As2_IP_Prefix + i, Variables.ListenerPortNumber, i);
                    Variables.ListenerAS.TryAdd(Variables.As2_IP_Prefix + i, AS);

                }
                else if (i > as2First && i < 10)
                {
                    AS = Variables.AS3;
                    Variables.ListenerConnectionAndAS.TryAdd((ushort)i, AS);
                    BGPListener[i].BindListener(Variables.As3_IP_Prefix + i, Variables.ListenerPortNumber, i);
                    Variables.ListenerAS.TryAdd(Variables.As3_IP_Prefix + i, AS);

                }

                Thread.Sleep(500);
            }


        }


        public void StartListening() //Set the created listener sockets/routers to start listening for traffic
        {


            for (int i = 0; i < 10; i++)
            {
                BGPListener[i].Listen(10);
                BGPListener[i].Accept();
            }


        }
        public void StartSpeaker() //Create the speaker sockets for the routers
        {
            for (int k = 0; k < 10; k++)
            {


                BGPSpeaker[m] = new Speaker();

                if (k < as2First)
                {
                    AS = Variables.AS1;
                    if (k == as1Last)
                    {

                        BGPSpeaker[m].BindSpeaker(Variables.As1_IP_Prefix + k, Variables.SpeakerPortNumber, m);
                        Variables.SpeakerAS.TryAdd(Variables.As1_IP_Prefix + k, AS);

                        m++;
                        BGPSpeaker[m] = new Speaker();
                        BGPSpeaker[m].BindSpeaker(Variables.As1_IP_Prefix + k, Variables.SpeakerPortNumber + 1, m);

                    }
                    else if (k < as1Last)
                    {

                        BGPSpeaker[m].BindSpeaker(Variables.As1_IP_Prefix + k, Variables.SpeakerPortNumber, m);
                        Variables.SpeakerAS.TryAdd(Variables.As1_IP_Prefix + k, AS);

                    }

                }
                else if (k > as1Last && k < as3First)
                {
                    AS = Variables.AS2;
                    if (k == as2First)
                    {
                        BGPSpeaker[m].BindSpeaker(Variables.As2_IP_Prefix + k, Variables.SpeakerPortNumber, m);
                        Variables.SpeakerAS.TryAdd(Variables.As2_IP_Prefix + k, AS);
                        m++;
                        BGPSpeaker[m] = new Speaker();
                        BGPSpeaker[m].BindSpeaker(Variables.As2_IP_Prefix + k, Variables.SpeakerPortNumber + 1, m);
                    }
                    else if (k == as2Last)
                    {
                        BGPSpeaker[m].BindSpeaker(Variables.As2_IP_Prefix + k, Variables.SpeakerPortNumber, m);
                        Variables.SpeakerAS.TryAdd(Variables.As2_IP_Prefix + k, AS);
                        m++;
                        BGPSpeaker[m] = new Speaker();
                        BGPSpeaker[m].BindSpeaker(Variables.As2_IP_Prefix + k, Variables.SpeakerPortNumber + 1, m);
                        m++;
                        BGPSpeaker[m] = new Speaker();
                        BGPSpeaker[m].BindSpeaker(Variables.As2_IP_Prefix + k, Variables.SpeakerPortNumber + 2, m);
                    }
                    else if (as2First < k && k < as2Last)
                    {
                        BGPSpeaker[m].BindSpeaker(Variables.As2_IP_Prefix + k, Variables.SpeakerPortNumber, m);
                        Variables.SpeakerAS.TryAdd(Variables.As2_IP_Prefix + k, AS);
                    }


                }
                else if (k > as2Last && k < 9)
                {
                    AS = Variables.AS3;

                    BGPSpeaker[m].BindSpeaker(Variables.As3_IP_Prefix + k, Variables.SpeakerPortNumber, m);

                    Variables.SpeakerAS.TryAdd(Variables.As3_IP_Prefix + k, AS);


                }
                else if (k == 9)
                {
                    AS = Variables.AS3;
                    BGPSpeaker[m].BindSpeaker(Variables.As3_IP_Prefix + k, Variables.SpeakerPortNumber, m);
                    Variables.SpeakerAS.TryAdd(Variables.As3_IP_Prefix + k, AS);
                    m++;
                    BGPSpeaker[m] = new Speaker();
                    BGPSpeaker[m].BindSpeaker(Variables.As3_IP_Prefix + k, Variables.SpeakerPortNumber + 1, m);

                }
                m++;

                Thread.Sleep(500);

            }

        }
        public void SpeakerConnection_Init() //Initialize the speakers
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
                        BGPSpeaker[n].Connect(Variables.As2_IP_Prefix + (k + 1), Variables.ListenerPortNumber, k, k + 1);

                        Variables.ConnectionAndListener.TryAdd(n, Variables.As2_IP_Prefix + (k + 1));
                        Variables.ConnectionAndSpeaker.TryAdd(n, Variables.As1_IP_Prefix + k);
                        SendOpenMessageToListener(n);

                        if (1 < as1Last) //more connections are created to AS1 depending on the size of AS1
                        {
                            n++;
                            Variables.SpeakerConnectionAndAS.TryAdd((ushort)n, Variables.AS1);
                            BGPSpeaker[n].Connect(Variables.As1_IP_Prefix + (k - 2), Variables.ListenerPortNumber, k, k - 2);
                            Variables.ConnectionAndListener.TryAdd(n, Variables.As1_IP_Prefix + (k - 2));
                            Variables.ConnectionAndSpeaker.TryAdd(n, Variables.As1_IP_Prefix + k);
                            SendOpenMessageToListener(n);
                        }

                    }
                    else if (0 < as1Last) //creating normal connections to AS1 (n -> n+1 )
                    {
                        BGPSpeaker[n].Connect(Variables.As1_IP_Prefix + (k + 1), Variables.ListenerPortNumber, k, k + 1);
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
                        BGPSpeaker[n].Connect(Variables.As2_IP_Prefix + (k + 1), Variables.ListenerPortNumber, k, k + 1);
                        Variables.ConnectionAndListener.TryAdd(n, Variables.As2_IP_Prefix + (k + 1));
                        Variables.ConnectionAndSpeaker.TryAdd(n, Variables.As2_IP_Prefix + k);
                        SendOpenMessageToListener(n);
                        if (k == as2First && (as2Last - as2First) > 1)
                        {
                            n++;
                            Variables.SpeakerConnectionAndAS.TryAdd((ushort)n, Variables.AS2);
                            BGPSpeaker[n].Connect(Variables.As2_IP_Prefix + (k + 2), Variables.ListenerPortNumber, k, k + 2);
                            Variables.ConnectionAndListener.TryAdd(n, Variables.As2_IP_Prefix + (k + 2));
                            Variables.ConnectionAndSpeaker.TryAdd(n, Variables.As2_IP_Prefix + k);
                            SendOpenMessageToListener(n);
                        }
                    }

                    else if (k == as2Last) //creating connection which connects to the first router of AS3
                    {
                        BGPSpeaker[n].Connect(Variables.As3_IP_Prefix + (k + 1), Variables.ListenerPortNumber, k, k + 1);
                        Variables.ConnectionAndListener.TryAdd(n, Variables.As3_IP_Prefix + (k + 1));
                        Variables.ConnectionAndSpeaker.TryAdd(n, Variables.As2_IP_Prefix + k);
                        SendOpenMessageToListener(n);


                        if ((as2Last - as2First) > 1) //more connections are created to AS2 depending on the size of AS2
                        {
                            n++;
                            Variables.SpeakerConnectionAndAS.TryAdd((ushort)n, Variables.AS2);
                            BGPSpeaker[n].Connect(Variables.As2_IP_Prefix + (k - 2), Variables.ListenerPortNumber, k, k - 2);
                            Variables.ConnectionAndListener.TryAdd(n, Variables.As2_IP_Prefix + (k - 2));
                            Variables.ConnectionAndSpeaker.TryAdd(n, Variables.As2_IP_Prefix + k);
                            SendOpenMessageToListener(n);
                            if ((as2Last - as2First) > 2)
                            {
                                n++;
                                Variables.SpeakerConnectionAndAS.TryAdd((ushort)n, Variables.AS2);
                                BGPSpeaker[n].Connect(Variables.As2_IP_Prefix + (k - 3), Variables.ListenerPortNumber, k, k - 3);
                                Variables.ConnectionAndListener.TryAdd(n, Variables.As2_IP_Prefix + (k - 3));
                                Variables.ConnectionAndSpeaker.TryAdd(n, Variables.As2_IP_Prefix + k);
                                SendOpenMessageToListener(n);
                            }
                        }

                    }
                    else if (as2First < k && k < as2Last) //creating normal connections to AS2 (n -> n+1 )
                    {
                        BGPSpeaker[n].Connect(Variables.As2_IP_Prefix + (k + 1), Variables.ListenerPortNumber, k, k + 1);

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
                    BGPSpeaker[n].Connect(Variables.As3_IP_Prefix + (k + 1), Variables.ListenerPortNumber, k, k + 1);
                    Variables.ConnectionAndListener.TryAdd(n, Variables.As3_IP_Prefix + (k + 1));
                    Variables.ConnectionAndSpeaker.TryAdd(n, Variables.As3_IP_Prefix + k);
                    SendOpenMessageToListener(n);

                }
                else if (k == 9) //creating connection to the first router of AS1
                {
                    Variables.SpeakerConnectionAndAS.TryAdd((ushort)n, Variables.AS3);
                    BGPSpeaker[n].Connect(Variables.As1_IP_Prefix + (k - 9), Variables.ListenerPortNumber, k, k - 9);
                    Variables.ConnectionAndListener.TryAdd(n, Variables.As1_IP_Prefix + (k - 9));
                    Variables.ConnectionAndSpeaker.TryAdd(n, Variables.As3_IP_Prefix + k);
                    SendOpenMessageToListener(n);
                    if ((k - (as2Last + 1)) > 1)//more connections are created to AS3 depending on the size of AS3
                    {
                        n++;
                        Variables.SpeakerConnectionAndAS.TryAdd((ushort)n, Variables.AS3);
                        BGPSpeaker[n].Connect(Variables.As3_IP_Prefix + (k - 2), Variables.ListenerPortNumber, k, k - 2);
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
                Open OpenPacket = new Open(Variables.BGPVersion, Variables.SpeakerConnectionAndAS[(ushort)k], Variables.HoldTime,
                    Variables.ConnectionAndSpeaker[k], Variables.OptimalParameterLength);

                BGPSpeaker[k].Send(OpenPacket.BGPmessage);

            }
        }
    }
}

