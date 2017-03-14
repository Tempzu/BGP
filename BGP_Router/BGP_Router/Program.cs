using System;
using BGPSimulator.FSM;
using BGPSimulator.BGP;
using System.Threading;

namespace BGPSimulator
{
    public static class Program
    {
       
        public static void Main(string[] args)
        {
            
            Console.WriteLine("Run the BGP simulator");
            Thread.Sleep(1000);
            FinateStateMachine FSM_Server = new FinateStateMachine();
            FSM_Server.Timers();
            GlobalVariables.True = true;
            FSM_Server.StartBGPConnectionMethod(GlobalVariables.True);
            Routes bgpRoutes = new Routes();
           
            UpdateMessageHandling createUpdate = new UpdateMessageHandling();
            CloseRouter close = new CloseRouter();

            int rnumber; //variables that help in the removing of the router
            int AS;
            string temp;
            string address;

            

            while (true)
            {
                Console.WriteLine("Type 'update' to enforce local policy. This must be done in order to see routing tables.");
                Console.WriteLine("Type 'as1', 'as2' or 'as3' to see routing tables");
                Console.WriteLine("Type 'remove' to close a router");
                Console.WriteLine("Type 'help' for commands");
                string line = Console.ReadLine(); // User can update, look routing tables and remove routers with commands

                switch (line)
                {
                    case "help":
                        Console.WriteLine("Type 'update' to enforce local policy. This must be done in order to see routing tables.");
                        Console.WriteLine("Type 'as1', 'as2' or 'as3' to see routing tables");
                        Console.WriteLine("Type 'remove' to close a router");
                        break;
                    case "as1":
                        bgpRoutes.DisplayDataAS1();
                        break;
                    case "as2":
                        bgpRoutes.DisplayDataAS2();
                        break;
                    case "as3":
                        bgpRoutes.DisplayDataAS3();
                        break;
                    case "update":
                        GlobalVariables.data = Routes.GetTable();
                        Console.WriteLine("Local Policies are updated for AS1, AS2 and AS3");
                        createUpdate.adj_RIB_Out();
                        createUpdate.pathAttribute();
                        createUpdate.networkLayerReachibility();
                        createUpdate.pathSegment();
                        break;
                    case "remove":
                        Console.WriteLine("Type the AS and the number of the router you want to remove (for example, to remove 127.1.0.0 type '1' and '0')");
                        Console.WriteLine("Type the AS");
                        temp = Console.ReadLine();
                        try
                        {
                            AS = int.Parse(temp);
                        }
                        catch
                        {
                            Console.WriteLine("The character you typed wasn't a number, try again by typing remove");
                            break;
                        }
                        Console.WriteLine("Type the number");
                        temp = Console.ReadLine();
                        try
                        {
                            rnumber = int.Parse(temp);
                        }
                        catch
                        {
                            Console.WriteLine("The character you typed wasn't a number, try again by typing remove");
                            break;
                        }
                        address = "127." + AS + ".0." + rnumber;
                        Console.WriteLine("You are trying to remove: "+ address);
                        close.CloseSpeakerListner(address, rnumber, AS);
                        break;
                    default:
                        Console.WriteLine("Your input didn't match any commands, type help for commands");
                        break;
                }

            }

        }
   
    }
}
