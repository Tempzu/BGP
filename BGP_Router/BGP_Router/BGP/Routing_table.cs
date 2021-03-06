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
            // Here we create a DataTable with six columns.
            DataTable Routes_Table = new DataTable();
            Routes_Table.Columns.Add("Connection", typeof(int));
            Routes_Table.Columns.Add("Network", typeof(string));
            Routes_Table.Columns.Add("AS_Number", typeof(int));
            Routes_Table.Columns.Add("NextHop", typeof(string));
            Routes_Table.Columns.Add("AS_NextHop", typeof(int));
            Routes_Table.Columns.Add("IGP/EGP", typeof(int));
            // Method for finding pairs to each peer in the system. Creates the routing table with the pre-set entities.
            foreach (KeyValuePair<int, string> pair in Variables.ConnectionAndListener)
            {
                try
                {
                    if (Variables.ListenerAS[pair.Value] == Variables.SpeakerAS[Variables.ConnectionAndSpeaker[pair.Key]])
                    {

                        Routes_Table.Rows.Add(pair.Key, Variables.ConnectionAndSpeaker[pair.Key], Variables.SpeakerAS[Variables.ConnectionAndSpeaker[pair.Key]],
                          pair.Value, Variables.ListenerAS[pair.Value], 0);
                        Tuple<string, ushort, string, ushort> conSpeakerAs_ListnerAs = new Tuple<string, ushort, string, ushort>(Variables.ConnectionAndSpeaker[pair.Key],
                           Variables.SpeakerAS[Variables.ConnectionAndSpeaker[pair.Key]], pair.Value, Variables.ListenerAS[pair.Value]);
                        Variables.ConnectionSpeakerAs_ListenerAs.Add(pair.Key, conSpeakerAs_ListnerAs);
                    }
                    else
                    {
                        Routes_Table.Rows.Add(pair.Key, Variables.ConnectionAndSpeaker[pair.Key], Variables.SpeakerAS[Variables.ConnectionAndSpeaker[pair.Key]],
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

            return Routes_Table;
        }
        string temp; //For printing internal/external
        // Simple methods for printing out the corresponding routing tables to each AS.
        public void DisplayDataAS1()
        {
        
            Console.WriteLine("BGP routing table for AS1");
            Console.WriteLine("Connection" + "     Router  " + "  AS  " + "   NextHop  " + "NextHop AS " + " I/E");
            // Loop for filling out each rows with corresponding information.
            foreach (DataRow row in Variables.Data.Rows)
            {
                if (row.Field<int>(2) == 1 || row.Field<int>(4) == 1)
                {
                    if (row.Field<int>(5) == 1)
                    {
                        temp = "E";
                    }else
                    {
                        temp = "I";
                    }
                    if (row.Field<int>(0) < 10)
                    {
                        Console.WriteLine("     " + row.Field<int>(0) + "        " + row.Field<string>(1) + "  " + row.Field<int>(2) + "     "
                        + row.Field<string>(3) + "    " + row.Field<int>(4) + "         " + temp);
                    }
                    else
                        Console.WriteLine("     " + row.Field<int>(0) + "       " + row.Field<string>(1) + "  " + row.Field<int>(2) + "     "
                            + row.Field<string>(3) + "    " + row.Field<int>(4) + "         " + temp);
                }

            }
        }
        public void DisplayDataAS2()
        {
            Console.WriteLine("BGP routing table for AS2");
            Console.WriteLine("Connection" + "     Router  " + "  AS  " + "   NextHop  " + "NextHop AS " + " I/E");
            // Loop for filling out each rows with corresponding information.
            foreach (DataRow row in Variables.Data.Rows)
            {
                if (row.Field<int>(2) == 2 || row.Field<int>(4) == 2)
                {
                   
                    if (row.Field<int>(5) == 1)
                    {
                        temp = "E";
                    }else
                    {
                        temp = "I";
                    }
                    if (row.Field<int>(0) < 10)
                    {
                        Console.WriteLine("     " + row.Field<int>(0) + "        " + row.Field<string>(1) + "  " + row.Field<int>(2) + "     "
                        + row.Field<string>(3) + "    " + row.Field<int>(4) + "         " + temp);
                    }
                    else
                        Console.WriteLine("     " + row.Field<int>(0) + "       " + row.Field<string>(1) + "  " + row.Field<int>(2) + "     "
                            + row.Field<string>(3) + "    " + row.Field<int>(4) + "         " + temp);
                }
            }
        }
        public void DisplayDataAS3()
        {
            Console.WriteLine("BGP routing table for AS3");
            Console.WriteLine("Connection" + "     Router  " + "  AS  " + "   NextHop  " + "NextHop AS " + " I/E");
            // Loop for filling out each rows with corresponding information.
            foreach (DataRow row in Variables.Data.Rows)
            {
                if (row.Field<int>(2) == 3 || row.Field<int>(4) == 3)
                {
                   
                     if (row.Field<int>(5) == 1)
                    {
                        temp = "E";
                    }else
                    {
                        temp = "I";
                    }
                    if (row.Field<int>(0) < 10)
                    {
                        Console.WriteLine("     " + row.Field<int>(0) + "        " + row.Field<string>(1) + "  " + row.Field<int>(2) + "     "
                        + row.Field<string>(3) + "    " + row.Field<int>(4) + "         " + temp);
                    }
                    else
                        Console.WriteLine("     " + row.Field<int>(0) + "       " + row.Field<string>(1) + "  " + row.Field<int>(2) + "     "
                            + row.Field<string>(3) + "    " + row.Field<int>(4) + "         " + temp);
                }
            }
        }

    }
}
