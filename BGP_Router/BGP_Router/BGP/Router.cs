using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

/* Includes the methods to create and bind the router's speakers and listeners*/

namespace BGP_Router.BGP
{
    public class Router
    {
        public Socket mSocketListener;
        public Socket mSocketSpeaker;
        public byte[] mBuffer = new byte[1024];

        private static AutoResetEvent SpeakerOn = new AutoResetEvent(true);
        private static AutoResetEvent ListenerOn = new AutoResetEvent(true);

        public void SocketListener()
        {
            //Create a TCP socket for incoming traffic
            mSocketListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        public void SocketSpeaker()
        {
            //Create a TCP socket for outgoing traffic
            mSocketSpeaker = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        public void BindSpeaker(string ipAddr, int port, int i)
        {
            SocketSpeaker();
            //Bind socket to an IP end point with the port
            mSocketSpeaker.Bind(new IPEndPoint(IPAddress.Parse(ipAddr), port));
            SpeakerOn.WaitOne();
            Console.WriteLine("Router Speaker: " + i + " IP-Address:" + IPAddress.Parse(((IPEndPoint)mSocketSpeaker.LocalEndPoint).Address.ToString())
               + " Speaker started. It is in: " + Variables.SpeakerConnectionStatus + "state.");
            //Start the speaker event
            SpeakerOn.Set();

        }
        public void BindListener(string ipAddr, int port, int i)
        {
            SocketListener();
            // Bind socket to an IP end point with the port
            mSocketListener.Bind(new IPEndPoint(IPAddress.Parse(ipAddr), port));
            ListenerOn.WaitOne();
            Console.WriteLine("Router Listener: " + i + " IP-Address:" + IPAddress.Parse(((IPEndPoint)mSocketListener.LocalEndPoint).Address.ToString())
                + " Listener started. It is in: " + Variables.ListenerConnectionStatus + "state.");
            //Start the listener event
            ListenerOn.Set();

        }

    }
}
