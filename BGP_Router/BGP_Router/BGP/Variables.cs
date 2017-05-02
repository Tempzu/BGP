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
        public static string PrefixAS1 = "127.1";
        public static string PrefixAS2 = "127.2";
        public static string PrefixAS3 = "127.3";
        public static string As1_IP_Prefix = "127.1.0.";
        public static string As2_IP_Prefix = "127.2.0.";
        public static string As3_IP_Prefix = "127.3.0.";
        public static int as1Last;
        public static int as2First;
        public static int as2Last;
        public static int as3First;
        //public static ushort autonomousSystemSpeaker;
        //public static ushort autonomousSystemListner;
        public static ushort PacketAS;
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
        //implement IP
        public static string SpeakerIPAddress;
        public static string ListenerIPAddress;

        //connectCount and Listner
        //ConcurrentDictionary is used for the thread safety
        public static ConcurrentDictionary<string, ushort> SpeakerAS = new ConcurrentDictionary<string, ushort>();
        public static ConcurrentDictionary<string, ushort> ListenerAS = new ConcurrentDictionary<string, ushort>();
        public static ConcurrentDictionary<int, string> ConnectionAndListener = new ConcurrentDictionary<int, string>();
        public static ConcurrentDictionary<int, string> ConnectionAndSpeaker = new ConcurrentDictionary<int, string>();

        public static ConcurrentDictionary<ushort, ushort> SpeakerConnectionAndAS = new ConcurrentDictionary<ushort, ushort>();
        public static ConcurrentDictionary<ushort, ushort> ListenerConnectionAndAS = new ConcurrentDictionary<ushort, ushort>();
        public static ConcurrentDictionary<int, Socket> ListenerSocketDictionary = new ConcurrentDictionary<int, Socket>();
        public static ConcurrentDictionary<int, Socket> SpeakerSocketDictionary = new ConcurrentDictionary<int, Socket>();
        public static bool[] SuccessfulConnection = new bool[14];

        //UPDATE Variables
        public static Dictionary<int, Tuple<string, ushort, string, ushort>>ConnectionSpeakerAs_ListenerAs = new Dictionary<int, Tuple<string, ushort, string, ushort>>();
        public static DataTable Data = new DataTable();
        public static Dictionary<int, Tuple<int, string, int, string, int, int, string, Tuple<string>>> Adj_RIB_Out = new Dictionary<int, Tuple<int, string, int, string, int, int, string, Tuple<string>>>();
        public static Dictionary<int, Tuple<int, string>> NLRI = new Dictionary<int, Tuple<int, string>>();
        public static Dictionary<int, Tuple<int, string, int, ushort>> PathAttribute = new Dictionary<int, Tuple<int, string, int, ushort>>();
        public static string Withdrawl_IP_Address;
        public static int Withdrawl_Length;
        public static Dictionary<int, Tuple<string, int>> WithdrawnRoutes = new Dictionary<int, Tuple<string, int>>();
        public static Dictionary<int, Tuple<int, string>> PathSegment = new Dictionary<int, Tuple<int, string>>();
        public static Dictionary<int, string> InterAS_IP = new Dictionary<int, string>();
        //Notification Message Variables
        public static ushort ErrorCode = 6;
        public static ushort ErrorSubCode = 8;

    }
}
