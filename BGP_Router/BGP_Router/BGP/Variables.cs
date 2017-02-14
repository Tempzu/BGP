using System;
using System.Collections.Generic;
//using System.Net;
using System.Net.Sockets;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Data;
using System.Collections.Concurrent;

namespace BGP_Router.BGP
{
    public static class Variables
    {
        public static string ListenerConnectionStatus;
        public static string SpeakerConnectionStatus;
        public static int ListenerPortNumber = 179;
        public static int SpeakerPortNumber = 176;
        public static ushort AS1 = 1;
        public static ushort AS2 = 2;
        public static ushort AS3 = 3;
        public static string prefixAS1 = "127.1";
        public static string prefixAS2 = "127.2";
        public static string prefixAS3 = "127.3";
        public static string as1_IP_Prefix = "127.1.0.";
        public static string as2_IP_Prefix = "127.2.0.";
        public static string as3_IP_Prefix = "127.3.0.";
        //public static ushort autonomousSystemSpeaker;
        //public static ushort autonomousSystemListner;
        public static ushort packetAS;
        public static int ListenerConnectionCount;
        public static int SpeakerConnectionCount;
        public static int CurrentSpeakerCount;
        public static int AllConnectionCount; //ConnectionCount
        public static int SendMsgCount;
        public static int ReceivedMsgCount;
        public static int KeepAliveMsgSendCount;
        public static int KeepAliveExpiredCount;
        public static int CurrentConnectionCount;
        public static string ConnectionStatus;
        public static bool True;
        public static bool KeepAlive;
        public static int SpeakerNumber;
        public static int ListenerNumber;
        public static ushort BGPVersion = 4;
        public static ushort HoldTime = 4;
        public static ushort OptimalParameterLength = 0;
        //implement ip
        public static string SpeakerIpAddress;
        public static string ListenerIpAddress;

        //connectCount and Listner
        //ConcurrentDictionary is used for the thread safety
        public static ConcurrentDictionary<string, ushort> speaker_AS = new ConcurrentDictionary<string, ushort>();
        public static ConcurrentDictionary<string, ushort> listner_AS = new ConcurrentDictionary<string, ushort>();
        public static ConcurrentDictionary<int, string> conAnd_Listner = new ConcurrentDictionary<int, string>();
        public static ConcurrentDictionary<int, string> conAnd_Speaker = new ConcurrentDictionary<int, string>();

        public static ConcurrentDictionary<ushort, ushort> speakerConAnd_AS = new ConcurrentDictionary<ushort, ushort>();
        public static ConcurrentDictionary<ushort, ushort> listnerConAnd_AS = new ConcurrentDictionary<ushort, ushort>();
        public static ConcurrentDictionary<int, Socket> listnerSocket_Dictionary = new ConcurrentDictionary<int, Socket>();
        public static ConcurrentDictionary<int, Socket> SpeakerSocket_Dictionary = new ConcurrentDictionary<int, Socket>();
        public static bool[] successfulConnection = new bool[14];

        //UPDATE Variables
        public static Dictionary<int, Tuple<string, ushort, string, ushort>> conSpeakerAs_ListnerAs = new Dictionary<int, Tuple<string, ushort, string, ushort>>();
        public static DataTable data = new DataTable();
        public static Dictionary<int, Tuple<int, string, int, string, int, int, string, Tuple<string>>> Adj_RIB_Out = new Dictionary<int, Tuple<int, string, int, string, int, int, string, Tuple<string>>>();
        public static Dictionary<int, Tuple<int, string>> NLRI = new Dictionary<int, Tuple<int, string>>();
        public static Dictionary<int, Tuple<int, string, int, ushort>> pathAttribute = new Dictionary<int, Tuple<int, string, int, ushort>>();
        public static string withdrawl_IP_Address;
        public static int withdrawl_Length;
        public static Dictionary<int, Tuple<string, int>> withdrawnRoutes = new Dictionary<int, Tuple<string, int>>();
        public static Dictionary<int, Tuple<int, string>> pathSegment = new Dictionary<int, Tuple<int, string>>();
        public static Dictionary<int, string> interASConIP = new Dictionary<int, string>();
        //Notification Message Variables
        public static ushort errorCode = 6;
        public static ushort errorSubCode = 8;

    }
}
