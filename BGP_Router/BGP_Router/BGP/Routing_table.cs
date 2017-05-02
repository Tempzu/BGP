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
            // Here we create a DataTable with four columns.
            DataTable Route_table = new DataTable();
            Route_table.Columns.Add("Connection", typeof(int));
            Route_table.Columns.Add("Network", typeof(string));
            Route_table.Columns.Add("AS_Number", typeof(int));
            Route_table.Columns.Add("NextHop", typeof(string));
            Route_table.Columns.Add("AS_NextHop", typeof(int));
            Route_table.Columns.Add("IGP/EGP", typeof(int));

            foreach (KeyValuePair<int, string> pair in Variables.ConnectionAndListener)
            {
                try
                {
                    if (Variables.ListenerAS[pair.Value] == Variables.SpeakerAS[Variables.ConnectionAndSpeaker[pair.Key]])
                    {

                        //Console.WriteLine("{0},{1}", pair.Key, pair.Value);
                        Route_table.Rows.Add(pair.Key, Variables.ConnectionAndSpeaker[pair.Key], Variables.SpeakerAS[Variables.ConnectionAndSpeaker[pair.Key]],
                          pair.Value, Variables.ListenerAS[pair.Value], 0);
                        //Storing connection speaker (ip, AS) and listner (ip, As)
                        Tuple<string, ushort, string, ushort> conSpeakerAs_ListnerAs = new Tuple<string, ushort, string, ushort>(Variables.ConnectionAndSpeaker[pair.Key],
                           Variables.SpeakerAS[Variables.ConnectionAndSpeaker[pair.Key]], pair.Value, Variables.ListenerAS[pair.Value]);
                        Variables.ConnectionSpeakerAs_ListenerAs.Add(pair.Key, conSpeakerAs_ListnerAs);
                    }
                    else
                    {
                        Route_table.Rows.Add(pair.Key, Variables.ConnectionAndSpeaker[pair.Key], Variables.SpeakerAS[Variables.ConnectionAndSpeaker[pair.Key]],
                            pair.Value, Variables.ListenerAS[pair.Value], 1);
                        //Storing connection speaker (ip, AS) and listner (ip, As)
                        Tuple<string, ushort, string, ushort> conSpeakerAs_ListnerAs = new Tuple<string, ushort, string, ushort>(Variables.ConnectionAndSpeaker[pair.Key],
                           Variables.SpeakerAS[Variables.ConnectionAndSpeaker[pair.Key]], pair.Value, Variables.ListenerAS[pair.Value]);
                        Variables.ConnectionSpeakerAs_ListenerAs.Add(pair.Key, conSpeakerAs_ListnerAs);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

            }

            return Route_table;
        }

        public void DisplayDataAS1()
        {
            // This uses the GetTable method (please paste it in).
            //GlobalVariables.data = GetTable();

            Console.WriteLine("BGP ROUTING TABLE OF AS 1");
            Console.WriteLine("Connection" + "   Network   " + " AS_Number " + "   NextHop  " + " AS_NextHop ");
            // ... Loop over all rows.
            foreach (DataRow row in Variables.Data.Rows)
            {
                if (row.Field<int>(2) == 1 || row.Field<int>(4) == 1)
                {
                    // ... Write value of first field as integer.

                    Console.WriteLine("     " + row.Field<int>(0) + "       " + row.Field<string>(1) + "  " + row.Field<int>(2) + "     "
                        + row.Field<string>(3) + "    " + row.Field<int>(4) + "       " + row.Field<int>(5));
                }

            }
        }
        public void DisplayDataAS2()
        {
            // This uses the GetTable method (please paste it in).

            Console.WriteLine("BGP ROUTING TABLE OF AS2");
            Console.WriteLine("Connection" + "   Network   " + " AS_Number " + "   NextHop  " + " AS_NextHop ");
            // ... Loop over all rows.
            foreach (DataRow row in Variables.Data.Rows)
            {
                if (row.Field<int>(2) == 2 || row.Field<int>(4) == 2)
                {
                    // ... Write value of first field as integer.

                    Console.WriteLine("     " + row.Field<int>(0) + "       " + row.Field<string>(1) + "  " + row.Field<int>(2) + "     "
                        + row.Field<string>(3) + "    " + row.Field<int>(4) + "       " + row.Field<int>(5));
                }
            }
        }
        public void DisplayDataAS3()
        {
            // This uses the GetTable method (please paste it in).

            Console.WriteLine("BGP ROUTING TABLE OF AS3");
            Console.WriteLine("Connection" + "   Network   " + " AS_Number " + "   NextHop  " + " AS_NextHop ");
            // ... Loop over all rows.
            foreach (DataRow row in Variables.Data.Rows)
            {
                if (row.Field<int>(2) == 3 || row.Field<int>(4) == 3)
                {
                    // ... Write value of first field as integer.

                    Console.WriteLine("     " + row.Field<int>(0) + "       " + row.Field<string>(1) + "  " + row.Field<int>(2) + "     "
                        + row.Field<string>(3) + "    " + row.Field<int>(4) + "       " + row.Field<int>(5));
                }
            }
        }

    }
}
