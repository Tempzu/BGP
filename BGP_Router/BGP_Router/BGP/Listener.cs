using System;
using System.Net;
using System.Net.Sockets;
using BGP_Router.Masiina;
using BGP_Router.Messages;
using System.Threading;

namespace BGP_Router.BGP
{
    public class Listener : Router
    {
        FSM FSM_Listener = new FSM();


        public Socket[] TempSocket = new Socket[14];
        public string mMessageType;

        private static AutoResetEvent AcceptedConnection = new AutoResetEvent(true);
        private static AutoResetEvent ReceivedMessage = new AutoResetEvent(true);
        private static AutoResetEvent SendKeepAlive = new AutoResetEvent(true);
        private static AutoResetEvent SendUpdate = new AutoResetEvent(true);

        public void Listen(int backlog)
        {
        /*Enables the current socket to listen for incoming connection attempts; backlog is 
        the maximum number of incoming connections that can be queued for acceptance. */
            
        mSocketListener.Listen(backlog);
        }
        //Try to accept connection
        public void Accept()
        {
            try
            {
                mSocketListener.BeginAccept(AcceptedCallback, mSocketListener); 
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        //This Asynccallback is called when the connection is sucessful
        private void AcceptedCallback(IAsyncResult result)
        {
            try
            {
                AcceptedConnection.WaitOne();

                //Get the socket that handles the client request.
                Socket ListenerSocket = result.AsyncState as Socket;
                ListenerSocket = ListenerSocket.EndAccept(result);

                //Check that the amount of total connections is not less than speaker connection amount
                if (Variables.AllConnectionCount < Variables.ConnectionAndSpeaker.Count)
                {
                    Variables.ListenerSocketDictionary.TryAdd(Variables.AllConnectionCount, ListenerSocket);

                    Variables.AllConnectionCount++;

                }

                AcceptedConnection.Set();
                //Clear buffer
                mBuffer = new byte[1024];
                //Start to receive data with the set buffer parameters
                //ReceivedCallback gets the ACK is executed from the receiver
                ListenerSocket.BeginReceive(mBuffer, 0, mBuffer.Length, SocketFlags.None, ReceivedCallback, ListenerSocket);
                //Listen to more connections with Accept();
                Accept();
                ReceivedMessage.WaitOne();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ReceivedCallback(IAsyncResult result)
        {
            try
            {

                // we catch that connection we send and since AsyncState is a object so we set it as Socket to get connection
                Socket ListenerSocket = result.AsyncState as Socket;
                //EndReceive tells the received data amount
                int mBufferSize = ListenerSocket.EndReceive(result);
                //Set the packet size to be the same as the buffer size
                byte[] mPacket = new byte[mBufferSize];
                //Copy the data to the packet from the buffer
                Array.Copy(mBuffer, mPacket, mPacket.Length);
                //Call the packet handling method
                HandlePackets.Handle(mPacket, ListenerSocket);
                ReceivedMessage.Set();
                //Set the listener to "received open message" status
                FSM_Listener.BGPOpenMessageReceived(Variables.True);
                //clear buffer
                mBuffer = new byte[1024];
                ListenerSocket.BeginReceive(mBuffer, 0, mBuffer.Length, SocketFlags.None, ReceivedCallback, ListenerSocket);

            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine("Listener socket is closed");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void SendingOpenMsg_Speaker()
        {
            if (Variables.SendMsgCount < Variables.ListenerSocketDictionary.Count)
            {
                Socket tempSock = Variables.ListenerSocketDictionary[Variables.SendMsgCount];
                Open openPacket = new Open(Variables.BGPVersion, Variables.SpeakerConnectionAndAS[(ushort)Variables.SendMsgCount], Variables.HoldTime,
                   "" + IPAddress.Parse(((IPEndPoint)tempSock.LocalEndPoint).Address.ToString()), Variables.OptimalParameterLength);
                mMessageType = "OPEN";
                Console.WriteLine("#### Listener:" + IPAddress.Parse(((IPEndPoint)tempSock.LocalEndPoint).Address.ToString()) + " has sent an OPEN message ####");
                SendSpeaker(openPacket.BGPmessage, tempSock, mMessageType);

                Variables.SendMsgCount++;
            }

        }
        public void SendingKeepAliveMsg_Speaker()
        {
            if (Variables.KeepAliveMsgSendCount < Variables.ListenerSocketDictionary.Count)
            {
                Socket tempSock = Variables.ListenerSocketDictionary[Variables.KeepAliveMsgSendCount];
                KeepAlive keepAlivePacket = new KeepAlive();
                Console.WriteLine("#### Listener:" + IPAddress.Parse(((IPEndPoint)tempSock.LocalEndPoint).Address.ToString()) + " has sent a KEEPALIVE message ####");
                SendSpeaker(keepAlivePacket.BGPmessage, tempSock, mMessageType);

                Variables.KeepAliveMsgSendCount++;


            }

        }
        public void KeepAliveExpired()
        {
            if (Variables.KeepAliveExpiredCount < Variables.ListenerSocketDictionary.Count)
            {
                if (Variables.ListenerSocketDictionary.ContainsKey(Variables.KeepAliveExpiredCount))
                {
                    Socket tempSock = Variables.ListenerSocketDictionary[Variables.KeepAliveExpiredCount];
                    KeepAlive keepAlivePacket = new KeepAlive();
                    mMessageType = "KeepAlive";
                    SendKeepAlive.WaitOne();
                    Console.WriteLine("#### Listener:" + IPAddress.Parse(((IPEndPoint)tempSock.LocalEndPoint).Address.ToString()) + " has sent a KEEPALIVE expiration message ####");
                    SendKeepAlive.Set();
                    SendSpeaker(keepAlivePacket.BGPmessage, tempSock, mMessageType);
                    Variables.KeepAliveExpiredCount++;
                }
                else
                {
                    Variables.KeepAliveExpiredCount++;
                }

            }
            else
            {
                Variables.KeepAliveExpiredCount = 0;
            }
        }

        public void SendSpeaker(byte[] data, Socket sendSock, string msg)
        {
            try
            {
                Socket ListenerSocket = sendSock;
                if (msg == "OPEN")
                {
                    mMessageType = msg;
                    ListenerSocket.BeginSend(data, 0, data.Length, 0, SendCallback, sendSock);
                }
                else if (msg == "KeepAlive")
                {
                    mMessageType = msg;
                    ListenerSocket.BeginSend(data, 0, data.Length, 0, SendCallback, sendSock);
                }
                else if (msg == "Update")
                {
                    mMessageType = msg;
                    ListenerSocket.BeginSend(data, 0, data.Length, 0, SendCallback, sendSock);

                    SendUpdate.WaitOne();
                }
                else if (msg == "Notify")
                {
                    mMessageType = msg;
                    ListenerSocket.BeginSend(data, 0, data.Length, 0, SendCallback, sendSock);
                }



            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }
        private void SendCallback(IAsyncResult result)
        {
            try
            {

                Socket ListenerSocket = result.AsyncState as Socket;

                //Finish the data transmission
                int bytesSent = ListenerSocket.EndSend(result);
                SendUpdate.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }
}
