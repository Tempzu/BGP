using System;
//using System.Net;
//using System.Net.Sockets;
using System.Data;
using System.Collections.Generic;


namespace BGP_Router.BGP
{
    public class Routing_table
    {

        public static DataTable GetTable()
        {
            Variables.ConnectionSpeakerAs_ListenerAs.Clear();
            // Here we create a DataTable with five columns.
            DataTable Routing_Table = new DataTable();
            Routing_Table.Columns.Add("Connection", typeof(int));
            Routing_Table.Columns.Add("Network", typeof(string));
            Routing_Table.Columns.Add("AS_Number", typeof(int));
            Routing_Table.Columns.Add("NextHop", typeof(string));
            Routing_Table.Columns.Add("AS_NextHop", typeof(int));
            
            // Method for finding pairs to each peer in the system. Creates the routing table with the pre-set entities.
            foreach (KeyValuePair<int, string> pair in Variables.ConnectionAndListener)
            {
                try
                {
                    if (Variables.ListenerAS[pair.Value] == Variables.SpeakerAS[Variables.ConnectionAndSpeaker[pair.Key]])
                    {

                        Routing_Table.Rows.Add(pair.Key, Variables.ConnectionAndSpeaker[pair.Key], Variables.SpeakerAS[Variables.ConnectionAndSpeaker[pair.Key]],
                          pair.Value, Variables.ListenerAS[pair.Value], 0);
                        Tuple<string, ushort, string, ushort> conSpeakerAs_ListnerAs = new Tuple<string, ushort, string, ushort>(Variables.ConnectionAndSpeaker[pair.Key],
                           Variables.SpeakerAS[Variables.ConnectionAndSpeaker[pair.Key]], pair.Value, Variables.ListenerAS[pair.Value]);
                        Variables.ConnectionSpeakerAs_ListenerAs.Add(pair.Key, conSpeakerAs_ListnerAs);
                    }
                    else
                    {
                        Routing_Table.Rows.Add(pair.Key, Variables.ConnectionAndSpeaker[pair.Key], Variables.SpeakerAS[Variables.ConnectionAndSpeaker[pair.Key]],
                            pair.Value, Variables.ListenerAS[pair.Value], 1);
                        Tuple<string, ushort, string, ushort> connectionSpeakerAs_ListenerAs = new Tuple<string, ushort, string, ushort>(Variables.ConnectionAndSpeaker[pair.Key],
                           Variables.SpeakerAS[Variables.ConnectionAndSpeaker[pair.Key]], pair.Value, Variables.ListenerAS[pair.Value]);
                        Variables.ConnectionSpeakerAs_ListenerAs.Add(pair.Key, connectionSpeakerAs_ListenerAs);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

            }

            return Routing_Table;
        }

        // Simple methods for printing out the corresponding routing tables to each AS.
        public void DisplayDataAS1()
        {
        
            Console.WriteLine("BGP routing table for AS1");
            Console.WriteLine("Connection" + "   Network   " + " AS Number " + "   NextHop  " + " AS NextHop ");
            // Loop for filling out each rows with corresponding information.
            foreach (DataRow row in Variables.Data.Rows)
            {
                if (row.Field<int>(2) == 1 || row.Field<int>(4) == 1)
                {
                    Console.WriteLine("     " + row.Field<int>(0) + "       " + row.Field<string>(1) + "  " + row.Field<int>(2) + "     "
                        + row.Field<string>(3) + "    " + row.Field<int>(4) + "       " + row.Field<int>(5));
                }

            }
        }
        public void DisplayDataAS2()
        {
            Console.WriteLine("BGP routing table for AS2");
            Console.WriteLine("Connection" + "   Network   " + " A Number " + "   NextHop  " + " AS NextHop ");
            // Loop for filling out each rows with corresponding information.
            foreach (DataRow row in Variables.Data.Rows)
            {
                if (row.Field<int>(2) == 2 || row.Field<int>(4) == 2)
                {
                   
                    Console.WriteLine("     " + row.Field<int>(0) + "       " + row.Field<string>(1) + "  " + row.Field<int>(2) + "     "
                        + row.Field<string>(3) + "    " + row.Field<int>(4) + "       " + row.Field<int>(5));
                }
            }
        }
        public void DisplayDataAS3()
        {
            Console.WriteLine("BGP routing table for AS3");
            Console.WriteLine("Connection" + "   Network   " + " AS Number " + "   NextHop  " + " AS NextHop ");
            // Loop for filling out each rows with corresponding information.
            foreach (DataRow row in Variables.Data.Rows)
            {
                if (row.Field<int>(2) == 3 || row.Field<int>(4) == 3)
                {
                   
                    Console.WriteLine("     " + row.Field<int>(0) + "       " + row.Field<string>(1) + "  " + row.Field<int>(2) + "     "
                        + row.Field<string>(3) + "    " + row.Field<int>(4) + "       " + row.Field<int>(5));
                }
            }
        }

    }
}
