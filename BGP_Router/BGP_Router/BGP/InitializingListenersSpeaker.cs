using System;
using BGPSimulator.BGPMessage;
using System.Net.Sockets;
using BGPSimulator.FSM;
using System.Net;
using System.Threading;

namespace BGPSimulator.BGP
{
    public class InitilizeBGPListnerSpeaker 
    {
        public static BGPListner[] bgpListner = new BGPListner[10];
        public static BGPSpeaker[] bgpSpeaker = new BGPSpeaker[14];

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

        public void StartListner()
        {
            Console.WriteLine("There is a total of 10 routers which will be split into 3 AS. How many routers do you want in AS1? (1-8)");
            temp = Console.ReadLine();
            try
            {
                as1Last = int.Parse(temp) - 1; //-1 since routers start from 0 
                if(as1Last < 0)
                {
                    Console.WriteLine("The character you typed wasn't in the given bounds, so we set 1 routers in the AS1.");
                    as1Last = 0;
                }else if(as1Last > 7)
                {
                    Console.WriteLine("The character you typed wasn't in the given bounds, so we set 8 routers in the AS1.");
                    as1Last = 7;
                }
            }catch 
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
                }else
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
                
                bgpListner[i] = new BGPListner();
                if (i < as2First)
                {
                    AS = GlobalVariables.AS1;
                    GlobalVariables.listnerConAnd_AS.TryAdd((ushort)i, AS);
                    bgpListner[i].BindListner(GlobalVariables.as1_IP_peifix + i, GlobalVariables.listnerPortNumber, i);
                    GlobalVariables.listner_AS.TryAdd(GlobalVariables.as1_IP_peifix + i, AS);
                    

                }else if (i > as1Last && i < as3First)
                {
                    AS = GlobalVariables.AS2;
                    GlobalVariables.listnerConAnd_AS.TryAdd((ushort)i, AS);
                    bgpListner[i].BindListner(GlobalVariables.as2_IP_Prefix + i, GlobalVariables.listnerPortNumber, i);
                    GlobalVariables.listner_AS.TryAdd(GlobalVariables.as2_IP_Prefix + i, AS);
                    
                } else if (i> as2First && i<10)
                {
                    AS = GlobalVariables.AS3;
                    GlobalVariables.listnerConAnd_AS.TryAdd((ushort)i, AS);
                    bgpListner[i].BindListner(GlobalVariables.as3_IP_Prefix + i, GlobalVariables.listnerPortNumber, i);
                    GlobalVariables.listner_AS.TryAdd(GlobalVariables.as3_IP_Prefix + i, AS);
                    
                }

                Thread.Sleep(500);
                //recient computers can handle 500 connections
            }
           

        }
        

        public void StartListning()
        {
           

            for (int i = 0; i < 10; i++)
            {
                    bgpListner[i].Listen(10);
                    bgpListner[i].Accept();
            }

            
        }
        public void StartSpeaker()
        {
            for (int k = 0; k < 10; k++)
            {
                

                bgpSpeaker[m] = new BGPSpeaker();
                
                if (k < as2First)
                {
                    AS = GlobalVariables.AS1;
                    if( k == as1Last)
                    {
                        
                        bgpSpeaker[m].BindSpeaker(GlobalVariables.as1_IP_peifix + k, GlobalVariables.speakerPortNumber, m);
                        GlobalVariables.speaker_AS.TryAdd(GlobalVariables.as1_IP_peifix + k, AS);
                       
                        m++;
                        bgpSpeaker[m] = new BGPSpeaker();
                        bgpSpeaker[m].BindSpeaker(GlobalVariables.as1_IP_peifix + k, GlobalVariables.speakerPortNumber+1, m);
                        
                    }
                    else if(k < as1Last)
                    {
                        
                        bgpSpeaker[m].BindSpeaker(GlobalVariables.as1_IP_peifix + k, GlobalVariables.speakerPortNumber, m);
                        GlobalVariables.speaker_AS.TryAdd(GlobalVariables.as1_IP_peifix + k, AS);
                      
                    }
                    
                }
                else if (k > as1Last && k < as3First)
                {
                    AS = GlobalVariables.AS2;
                    if ( k == as2First)
                    {
                        bgpSpeaker[m].BindSpeaker(GlobalVariables.as2_IP_Prefix + k, GlobalVariables.speakerPortNumber, m);
                        GlobalVariables.speaker_AS.TryAdd(GlobalVariables.as2_IP_Prefix + k, AS);
                        m++;
                        bgpSpeaker[m] = new BGPSpeaker();
                        bgpSpeaker[m].BindSpeaker(GlobalVariables.as2_IP_Prefix + k, GlobalVariables.speakerPortNumber+1, m);
                    }
                    else if ( k == as2Last)
                    {
                        bgpSpeaker[m].BindSpeaker(GlobalVariables.as2_IP_Prefix + k, GlobalVariables.speakerPortNumber, m);
                        GlobalVariables.speaker_AS.TryAdd(GlobalVariables.as2_IP_Prefix + k, AS);
                        m++;
                        bgpSpeaker[m] = new BGPSpeaker();
                        bgpSpeaker[m].BindSpeaker(GlobalVariables.as2_IP_Prefix + k, GlobalVariables.speakerPortNumber+1, m);
                        m++;
                        bgpSpeaker[m] = new BGPSpeaker();
                        bgpSpeaker[m].BindSpeaker(GlobalVariables.as2_IP_Prefix + k, GlobalVariables.speakerPortNumber+2, m);
                    }
                    else if (as2First < k && k < as2Last)
                    {
                        bgpSpeaker[m].BindSpeaker(GlobalVariables.as2_IP_Prefix + k, GlobalVariables.speakerPortNumber, m);
                        GlobalVariables.speaker_AS.TryAdd(GlobalVariables.as2_IP_Prefix + k, AS);
                    }
                    

                }
                else if (k> as2Last && k < 9)
                {
                    AS = GlobalVariables.AS3;

                    bgpSpeaker[m].BindSpeaker(GlobalVariables.as3_IP_Prefix + k, GlobalVariables.speakerPortNumber, m);

                    GlobalVariables.speaker_AS.TryAdd(GlobalVariables.as3_IP_Prefix + k, AS);
                    

                }
                else if (k == 9)
                {
                    AS = GlobalVariables.AS3;
                    bgpSpeaker[m].BindSpeaker(GlobalVariables.as3_IP_Prefix + k, GlobalVariables.speakerPortNumber, m);
                    GlobalVariables.speaker_AS.TryAdd(GlobalVariables.as3_IP_Prefix + k, AS);
                    
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


                if (k < as2First)
                {

                    GlobalVariables.speakerConAnd_AS.TryAdd((ushort)n, GlobalVariables.AS1);
                    GlobalVariables.connCountListner = n;
                    if (k == as1Last)
                    {
                        bgpSpeaker[n].Connect(GlobalVariables.as2_IP_Prefix + (k + 1), GlobalVariables.listnerPortNumber, k, k + 1);

                        GlobalVariables.conAnd_Listner.TryAdd(n, GlobalVariables.as2_IP_Prefix + (k + 1));
                        GlobalVariables.conAnd_Speaker.TryAdd(n, GlobalVariables.as1_IP_peifix + k);
                        SendOpenMessageToListner(n);

                        if (1 < as1Last)
                        {
                            n++;
                            GlobalVariables.speakerConAnd_AS.TryAdd((ushort)n, GlobalVariables.AS1);
                            bgpSpeaker[n].Connect(GlobalVariables.as1_IP_peifix + (k - 2), GlobalVariables.listnerPortNumber, k, k - 2);
                            GlobalVariables.conAnd_Listner.TryAdd(n, GlobalVariables.as1_IP_peifix + (k - 2));
                            GlobalVariables.conAnd_Speaker.TryAdd(n, GlobalVariables.as1_IP_peifix + k);
                            SendOpenMessageToListner(n);
                        }

                    }
                    else if (0 < as1Last)
                    {
                        bgpSpeaker[n].Connect(GlobalVariables.as1_IP_peifix + (k + 1), GlobalVariables.listnerPortNumber, k, k + 1);
                        GlobalVariables.conAnd_Listner.TryAdd(n, GlobalVariables.as1_IP_peifix + (k + 1));
                        GlobalVariables.conAnd_Speaker.TryAdd(n, GlobalVariables.as1_IP_peifix + k);
                        SendOpenMessageToListner(n);

                    }



                }
                else if (k > as1Last && k < as3First)
                {

                    GlobalVariables.speakerConAnd_AS.TryAdd((ushort)n, GlobalVariables.AS2);

                    GlobalVariables.connCountListner = n;
                    if (k == as2First && (as2Last - as2First) > 1)
                    {
                        bgpSpeaker[n].Connect(GlobalVariables.as2_IP_Prefix + (k + 1), GlobalVariables.listnerPortNumber, k, k + 1);
                        GlobalVariables.conAnd_Listner.TryAdd(n, GlobalVariables.as2_IP_Prefix + (k + 1));
                        GlobalVariables.conAnd_Speaker.TryAdd(n, GlobalVariables.as2_IP_Prefix + k);
                        SendOpenMessageToListner(n);

                    }
                    if (k == as2First && (as2Last - as2First) > 2)
                    {
                        n++;
                        GlobalVariables.speakerConAnd_AS.TryAdd((ushort)n, GlobalVariables.AS2);
                        bgpSpeaker[n].Connect(GlobalVariables.as2_IP_Prefix + (k + 2), GlobalVariables.listnerPortNumber, k, k + 2);
                        GlobalVariables.conAnd_Listner.TryAdd(n, GlobalVariables.as2_IP_Prefix + (k + 2));
                        GlobalVariables.conAnd_Speaker.TryAdd(n, GlobalVariables.as2_IP_Prefix + k);
                        SendOpenMessageToListner(n);
                    }
                    else if (k == as2Last)
                    {
                        bgpSpeaker[n].Connect(GlobalVariables.as3_IP_Prefix + (k + 1), GlobalVariables.listnerPortNumber, k, k + 1);
                        GlobalVariables.conAnd_Listner.TryAdd(n, GlobalVariables.as3_IP_Prefix + (k + 1));
                        GlobalVariables.conAnd_Speaker.TryAdd(n, GlobalVariables.as2_IP_Prefix + k);
                        SendOpenMessageToListner(n);


                        if ((as2Last - as2First) > 1)
                        {
                            n++;
                            GlobalVariables.speakerConAnd_AS.TryAdd((ushort)n, GlobalVariables.AS2);
                            bgpSpeaker[n].Connect(GlobalVariables.as2_IP_Prefix + (k - 2), GlobalVariables.listnerPortNumber, k, k - 2);
                            GlobalVariables.conAnd_Listner.TryAdd(n, GlobalVariables.as2_IP_Prefix + (k - 2));
                            GlobalVariables.conAnd_Speaker.TryAdd(n, GlobalVariables.as2_IP_Prefix + k);
                            SendOpenMessageToListner(n);
                        }
                        if ((as2Last - as2First) > 2)
                        {
                            n++;
                            GlobalVariables.speakerConAnd_AS.TryAdd((ushort)n, GlobalVariables.AS2);
                            bgpSpeaker[n].Connect(GlobalVariables.as2_IP_Prefix + (k - 3), GlobalVariables.listnerPortNumber, k, k - 3);
                            GlobalVariables.conAnd_Listner.TryAdd(n, GlobalVariables.as2_IP_Prefix + (k - 3));
                            GlobalVariables.conAnd_Speaker.TryAdd(n, GlobalVariables.as2_IP_Prefix + k);
                            SendOpenMessageToListner(n);
                        }
                    }
                    else if (as2First < k && k < as2Last)
                    {
                        bgpSpeaker[n].Connect(GlobalVariables.as2_IP_Prefix + (k + 1), GlobalVariables.listnerPortNumber, k, k + 1);

                        GlobalVariables.conAnd_Listner.TryAdd(n, GlobalVariables.as2_IP_Prefix + (k + 1));
                        GlobalVariables.conAnd_Speaker.TryAdd(n, GlobalVariables.as2_IP_Prefix + k);
                        SendOpenMessageToListner(n);
                    }


                    //SendOpenMessageToListner(n);

                }
                
                else if (k > as2Last && k < 9)
                {

                    GlobalVariables.speakerConAnd_AS.TryAdd((ushort)n, GlobalVariables.AS3);
                    GlobalVariables.connCountListner = n;

                    bgpSpeaker[n].Connect(GlobalVariables.as3_IP_Prefix + (k + 1), GlobalVariables.listnerPortNumber, k, k + 1);
                    GlobalVariables.conAnd_Listner.TryAdd(n, GlobalVariables.as3_IP_Prefix + (k + 1));
                    GlobalVariables.conAnd_Speaker.TryAdd(n, GlobalVariables.as3_IP_Prefix + k);
                    SendOpenMessageToListner(n);

                }
                else if (k == 9 && (k - (as2Last + 1)) > 1)
                {
                    GlobalVariables.speakerConAnd_AS.TryAdd((ushort)n, GlobalVariables.AS3);
                    bgpSpeaker[n].Connect(GlobalVariables.as3_IP_Prefix + (k - 2), GlobalVariables.listnerPortNumber, k, k - 2);
                    GlobalVariables.conAnd_Listner.TryAdd(n, GlobalVariables.as3_IP_Prefix + (k - 2));
                    GlobalVariables.conAnd_Speaker.TryAdd(n, GlobalVariables.as3_IP_Prefix + k);
                    SendOpenMessageToListner(n);
                }


                n++;

            }
            

        }
        public void SendOpenMessageToListner(int k)
        {
            if (GlobalVariables.True)
                {
                
                    GlobalVariables.conCountSpeaker = k;

                //Console.WriteLine("*********** SPEAKER NUMBER**************** : " + k);

                    //OpenMessage(ushort type, ushort version,ushort myAS, ushort holdTime, string bgpIdentifier, ushort optimalParLength)
                    OpenMessage openPacket = new OpenMessage(GlobalVariables.bgpVerson, GlobalVariables.speakerConAnd_AS[(ushort)k], GlobalVariables.holdTime,
                        GlobalVariables.conAnd_Speaker[k], GlobalVariables.optimalParLength);

                    bgpSpeaker[k].Send(openPacket.BGPmessage);
               
            }
        }
    }
  }


