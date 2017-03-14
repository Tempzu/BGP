using System;
using System.Net;
using System.Net.Sockets;
using BGP_Router.Messages;
using System.Text;
using System.Threading;

namespace BGP_Router.BGP
{
    public static class BuildPacket
    {

        private static AutoResetEvent ConstructPacket = new AutoResetEvent(true);
        public static void Handle(byte[] Packet, Socket ClientSocket)
        {
            ConstructPacket.WaitOne();

            Thread.Sleep(1000);
            ushort Marker;

            Console.Write("\n" + "Router : " + IPAddress.Parse(((IPEndPoint)ClientSocket.LocalEndPoint).Address.ToString()) + " Has recived Packet !! Marker: ");
            //Console.Write("Router : " + IPAddress.Parse(((IPEndPoint)ClientSocket.LocalEndPoint).Address.ToString()) + " Has recived: ");

            for (int i = 0; i < 16; i++)
            {
                Marker = BitConverter.ToUInt16(Packet, i * 2);
                Console.Write(Marker);
            }
            //PacketMarkerDone.Set();
            ushort PacketLength = BitConverter.ToUInt16(Packet, 32);
            ushort PacketType = BitConverter.ToUInt16(Packet, 38);

            switch (PacketType)
            {
                case 1:

                    //Console.WriteLine("OPEN MESSAGE !!");
                    ushort BGPVersion = BitConverter.ToUInt16(Packet, 40);
                    ushort AutoSystem = BitConverter.ToUInt16(Packet, 42);
                    ushort HoldTime = BitConverter.ToUInt16(Packet, 44);
                    string BGPIdentifier = Encoding.UTF8.GetString(Packet, 46, 10);
                    ushort OptimalParaLength = BitConverter.ToUInt16(Packet, 56);

                    Console.Write(" Length: {0} | Type: {1} | Version: {2} | AS: {3} | HoldTime in Min: {4} | BGPIdentifier: {5} | OptimalParaLenth: {6} ",
                    PacketLength, PacketType, BGPVersion, AutoSystem, HoldTime, BGPIdentifier, OptimalParaLength);
                    //Console.Write("OPEN MESSAGE");
                    Console.WriteLine(" from Router : " + IPAddress.Parse(((IPEndPoint)ClientSocket.RemoteEndPoint).Address.ToString()) + "\n");

                    //PacketOpenDone.Set();

                    break;
                case 2:
                    //UpdateMessage(ushort type, UInt16 withdrawRouteLength, ushort ipPrefixLength, string ipPrefix, ushort totalPathAttributeLength, UInt32 attributeLength, 
                    //UInt32 attrFlags, ushort typeCode, string attribute, ushort pathSegmentType,ushort pathSegmentLength,string pathSegmentValue, ushort nlrLength, 
                    //string nlrPrefix)
                    UInt16 WithdrawlRouteLength = BitConverter.ToUInt16(Packet, 40);
                    string WithdrawlRoutes = Encoding.UTF8.GetString(Packet, 42, 9);
                    ushort IPPrefixLength = BitConverter.ToUInt16(Packet, 51);
                    string IPPrefix = Encoding.UTF8.GetString(Packet, 53, 5);
                    ushort TotalPathAttribute = BitConverter.ToUInt16(Packet, 62);
                    UInt32 AttributeLength = BitConverter.ToUInt16(Packet, 64);
                    string Attribute = Encoding.UTF8.GetString(Packet, 66, 9);
                    UInt32 AttributeFlag = BitConverter.ToUInt16(Packet, 75);
                    ushort AttributeTypeCode = BitConverter.ToUInt16(Packet, 77);

                    ushort PathSegmentType = BitConverter.ToUInt16(Packet, 79);
                    ushort PathSegmentLength = BitConverter.ToUInt16(Packet, 81);
                    string PathSegmentValue = Encoding.UTF8.GetString(Packet, 83, 2);
                    ushort NLRLength = BitConverter.ToUInt16(Packet, 85);
                    string NLRPrefix = Encoding.UTF8.GetString(Packet, 87, 5);
                    Console.Write(" Length: {0} | Type: {1} | WithDrawlRouteLength: {2} | WithdrawlRoute: {3} IP_PrifixLenght: {4} | IP_Prefix: {5} | TotalPathAttributeLength: {6} | AttributeLength: {7} | AttributeFlag: {8} | AttributeTypeCode: {9} | Attribute: {10} | pathSegmentType: {11} | pathSegmentLength: {12} | pathSegmentValue: {13} | nlrLength: {14} | nlrPrefix: {15} ",
                     PacketLength, PacketType, WithdrawlRouteLength, WithdrawlRoutes, IPPrefixLength, IPPrefix, TotalPathAttribute, AttributeLength, AttributeFlag, AttributeTypeCode, Attribute, PathSegmentType,
                     PathSegmentLength, PathSegmentValue, NLRLength, NLRPrefix);
                    //Console.Write("OPEN MESSAGE");
                    Console.WriteLine(" from Router : " + IPAddress.Parse(((IPEndPoint)ClientSocket.RemoteEndPoint).Address.ToString()) + "\n");
                    break;
                case 3:
                    ushort ErrorCode = BitConverter.ToUInt16(Packet, 40);
                    ushort ErrorSubCode = BitConverter.ToUInt16(Packet, 42);
                    string Error = Encoding.UTF8.GetString(Packet, 44, 26);
                    Console.WriteLine(" Length: {0} | Type: {1} | ErrorCode: {2} | ErrorSubCode: {3} | Error: {4}", PacketLength, PacketType, ErrorCode, ErrorSubCode, Error);
                    break;
                case 4:

                    Console.Write(" Length: {0} | Type: {1} ", PacketLength, PacketType + " Description: KEEPALIVE");
                    //Console.Write("KeepAlive MESSAGE");
                    Console.WriteLine(" from Router : " + IPAddress.Parse(((IPEndPoint)ClientSocket.RemoteEndPoint).Address.ToString()) + "\n");

                    //PacketKeepAliveDone.Set();

                    break;
            }
            ConstructPacket.Set();

        }
    }
}
