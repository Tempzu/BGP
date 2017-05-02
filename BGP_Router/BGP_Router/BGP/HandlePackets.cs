using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

/* The Handle Packets class views the packet traffic in the network
 *  */
namespace BGP_Router.BGP
{
    public static class HandlePackets
    {

        private static AutoResetEvent HandlePacket = new AutoResetEvent(true);
        public static void Handle(byte[] Packet, Socket ClientSocket)
        {
            HandlePacket.WaitOne();

            Thread.Sleep(1000);
            ushort Marker;
            int MarkerTest = 0;
            ushort PacketLength = BitConverter.ToUInt16(Packet, 32);
            ushort PacketType = BitConverter.ToUInt16(Packet, 38);

            Console.Write("\n*************************************************************\nRouter with IP: " + IPAddress.Parse(((IPEndPoint)ClientSocket.LocalEndPoint).Address.ToString()) + " has recieved a packet. \nStarting packet marker checkup!\n");

            for (int i = 0; i < 16; i++)
            {
                /* Marker checkup. If all 16 bits are not ones -> packet desynchronization */
                Marker = BitConverter.ToUInt16(Packet, i * 2);
                MarkerTest = MarkerTest + Marker;
                if (MarkerTest == 16)
                {
                    Console.Write("Marker is OK.\n");
                }else if(Marker == 0)
                {
                    Console.Write("Marker is invalid.\n");
                }
                 
            }
            
            switch (PacketType)
            {
                case 1: // Open message info displayed 

                    ushort BGPVersion = BitConverter.ToUInt16(Packet, 40);
                    ushort AutonomousSystem = BitConverter.ToUInt16(Packet, 42);
                    ushort HoldTime = BitConverter.ToUInt16(Packet, 44);
                    string BGPIdentifier = Encoding.UTF8.GetString(Packet, 46, 10);
                    ushort OptimalParaLength = BitConverter.ToUInt16(Packet, 56);

                    Console.Write("This is an OPEN message!\nPacket info:\nLength: {0} |*| Packet type: {1} |*| BGP version: {2} |*| AS: {3} |*| HoldTime in Min: {4} |*| BGPIdentifier: {5}",
                    PacketLength, PacketType, BGPVersion, AutonomousSystem, HoldTime, BGPIdentifier);
                    Console.WriteLine("\nThe packet was sent from router: " + IPAddress.Parse(((IPEndPoint)ClientSocket.RemoteEndPoint).Address.ToString()) + "\n*************************************************************\n");
                    break;

                case 2: // Update message info dislpayed 
                  
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
                    Console.Write("This is an UPDATE message!\nPacket info:\nLength: {0} |*| Packet type: {1} |*| Route length: {2} |*| WithdrawlRoute: {3} |*| IP prefix length: {4} |*| IP prefix: {5} |*| Total path attribute length: {6} |*| Attribute length: {7} |*| Attribute flag: {8} |*| Attribute type code: {9} |*| Attribute: {10} |*| Path segment type: {11} |*| Path segment length: {12} |*| Path segment value: {13} |*| NLR length: {14} |*| NLR prefix: {15} ",
                     PacketLength, PacketType, WithdrawlRouteLength, WithdrawlRoutes, IPPrefixLength, IPPrefix, TotalPathAttribute, AttributeLength, AttributeFlag, AttributeTypeCode, Attribute, PathSegmentType,
                     PathSegmentLength, PathSegmentValue, NLRLength, NLRPrefix);
                    Console.WriteLine("\nThe packet was sent from router: " + IPAddress.Parse(((IPEndPoint)ClientSocket.RemoteEndPoint).Address.ToString()) + "\n*************************************************************\n");
                    break;

                case 3: //Notification message info displayed

                    ushort ErrorCode = BitConverter.ToUInt16(Packet, 40);
                    ushort ErrorSubCode = BitConverter.ToUInt16(Packet, 42);
                    string Error = Encoding.UTF8.GetString(Packet, 44, 26);
                    Console.WriteLine("This is a NOTIFICATION message!\nPacket info:\nLength: {0} |*| Packet type: {1} |*| ErrorCode: {2} |*| ErrorSubCode: {3} |*| Error: {4}", PacketLength, PacketType, ErrorCode, ErrorSubCode, Error);
                    Console.WriteLine("\nThe packet was sent from router: " + IPAddress.Parse(((IPEndPoint)ClientSocket.RemoteEndPoint).Address.ToString()) + "\n*************************************************************\n");
                    break;

                case 4: //Keepalive message info displayed

                    Console.Write("This is a KEEPALIVE message!\nPacket info:\nLength: {0} |*| Type: {1} ", PacketLength, PacketType);     
                    Console.WriteLine("\nThe packet was sent from router: " + IPAddress.Parse(((IPEndPoint)ClientSocket.RemoteEndPoint).Address.ToString()) + "\n*************************************************************\n");
                    break;
            }
            HandlePacket.Set();

        }
    }
}
