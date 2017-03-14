using System;
using System.Net;
using System.Net.Sockets;
using BGP_Router.Masiina;
using BGP_Router.Messages;
using System.Threading;
using System.Text;
using System.Collections.Generic;

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
        //private object mListenerSocket; //??

        public void Listen(int backlog)
        {
            // listens for 500 tcp backup connection request
            mSocketListener.Listen(backlog); //??
        }
        //it is implemented to accept to listen data
        public void Accept()
        {

            try
            {

                // Start an asynchronous socket to listen for connections.
                //Console.WriteLine("Waiting for a connection...");

                // Set the event to nonsignaled state.

                // Begin to accept the client connection
                // and it asks for two parameter with AsyncCallback and object (AcceptedCallback and null) null is set for object reference parameter

                mSocketListener.BeginAccept(AcceptedCallback, mSocketListener); // ??
                //acceptDone.Set();
                //acceptDone.WaitOne();
                // Wait until a connection is made before continuing.


            }


            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        // This Asynccallback is called when the connection is sucessful
        private void AcceptedCallback(IAsyncResult result)
        {
            try
            {
                AcceptedConnection.WaitOne();

                // Get the socket that handles the client request.
                Socket ListenerSocket = result.AsyncState as Socket;
                mSocketListener = ListenerSocket.EndAccept(result);

                //acceptDone.Set();
                // Create the state object.
                // StateObject state = new StateObject();
                // state.workSocket = handler;
                //allDone.Set();

                if (Variables.AllConnectionCount < Variables.ConnectionAndSpeaker.Count)
                {
                    Variables.ListenerSocketDictionary.TryAdd(Variables.AllConnectionCount, ListenerSocket);

                    Variables.AllConnectionCount++;

                }

                AcceptedConnection.Set();

                // it is the place where we store data and this step is done to clear previous data from memory
                mBuffer = new byte[1024];

                // this is done to be ready to recive data 
                // the four paramaters are buffer, the place in packet where 0 is the begining of packet, the amount of data where we set full capacity of buffer
                // SocketFlag is none, when the data is received the next parameter defines where to send data, and last one is the connection which should be passed 
                // to callback method
                ListenerSocket.BeginReceive(mBuffer, 0, mBuffer.Length, SocketFlags.None, ReceivedCallback, ListenerSocket);
                //reciveDone.WaitOne();

                //accept method is called to listen to more connection
                Accept();
                //acceptDone.WaitOne();
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


                // Retrieve the state object and the handler socket
                // from the asynchronous state object.
                //StateObject state = (StateObject)reasult.AsyncState;
                //Socket handler = state.workSocket;

                //EndReceive is used to count the amount of data received
                int mBufferSize = ListenerSocket.EndReceive(result);
                //reciveDone.Set();
                //if (bufferSize == 58 || bufferSize == 40)
                //{

                //it is done to store to store the data in buffer to packet
                byte[] mPacket = new byte[mBufferSize];
                //reciveDone.Set();
                // Console.WriteLine("*********************** Listener" + IPAddress.Parse(((IPEndPoint)ListenerSocket.LocalEndPoint).Address.ToString())
                //   + "*********************** Speaker" + IPAddress.Parse(((IPEndPoint)ListenerSocket.RemoteEndPoint).Address.ToString()));

                // it is done to create a shadow clone of buffer before anyone uses it
                // this method stores the data in buffer to packet
                Array.Copy(mBuffer, mPacket, mPacket.Length);



                //Handle the packet
                BuildPacket.Handle(mPacket, mSocketListener);

                ReceivedMessage.Set();

                FSM_Listener.BGPOpenMessageReceived(Variables.True);

                //}
                // else
                //{

                mBuffer = new byte[1024];
                ListenerSocket.BeginReceive(mBuffer, 0, mBuffer.Length, SocketFlags.None, ReceivedCallback, ListenerSocket);
                // }

            }
            catch (ObjectDisposedException ex)
            {
                // Don't care
                Console.WriteLine("Listener socket is closed");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void SendingOpenMsg_Speaker()
        {
            //tracking the proper connection number and the speaker socket was the difficult task

            if (Variables.SendMsgCount < Variables.ListenerSocketDictionary.Count)
            {
                //tempSocket[Variables.openMsgSendCount] = Variables.ListenerSocket_Dictionary[Variables.openMsgSendCount];
                Socket tempSock = Variables.ListenerSocketDictionary[Variables.SendMsgCount];
                Open openPacket = new Open(Variables.BGPVersion, Variables.SpeakerConnectionAndAS[(ushort)Variables.SendMsgCount], Variables.HoldTime,
                   "" + IPAddress.Parse(((IPEndPoint)tempSock.LocalEndPoint).Address.ToString()), Variables.OptimalParameterLength);


                //Console.WriteLine("Sending open packet" + "verson : " + Variables.bgpVerson + "AS : " + Variables.speakerConAnd_AS[(ushort)Variables.openMsgSendCount] + "Hold Time : " + Variables.holdTime + "IP : "
                // + IPAddress.Parse(((IPEndPoint)tempSocket[Variables.openMsgSendCount].RemoteEndPoint).Address.ToString()) + "param : " + Variables.optimalParLength);
                mMessageType = "OPEN";
                //Socket temSoc = tempSocket[Variables.openMsgSendCount];
                Console.WriteLine("BGP Listener:" + IPAddress.Parse(((IPEndPoint)tempSock.LocalEndPoint).Address.ToString()) + " has send open Message !!");
                SendSpeaker(openPacket.BGPmessage, tempSock, mMessageType);

                //sendOpenDone.WaitOne();

                Variables.SendMsgCount++;
            }

        }
        public void SendingKeepAliveMsg_Speaker()
        {
            //tracking the proper connection number and the speaker socket was the difficult task

            if (Variables.KeepAliveMsgSendCount < Variables.ListenerSocketDictionary.Count)
            {



                //tempSocket[Variables.keepAliveMsgSendCount] = Variables.ListenerSocket_Dictionary[Variables.keepAliveMsgSendCount];
                Socket tempSock = Variables.ListenerSocketDictionary[Variables.KeepAliveMsgSendCount];
                KeepAlive keepAlivePacket = new KeepAlive();
                mMessageType = "KeepAlive";

                //sendKeepAliveMessage.WaitOne();

                //Socket temSoc = tempSocket[Variables.keepAliveMsgSendCount];
                Console.WriteLine("BGP Listener:" + IPAddress.Parse(((IPEndPoint)tempSock.LocalEndPoint).Address.ToString()) + " has send keepAlive Message !!");



                SendSpeaker(keepAlivePacket.BGPmessage, tempSock, mMessageType);
                //sendKeepAliveMessage.Set();

                //sendKeepAliveDone.WaitOne();

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

                    Console.WriteLine("BGP Listener:" + IPAddress.Parse(((IPEndPoint)tempSock.LocalEndPoint).Address.ToString()) + " has send keepAlive Message !!");

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
                    //FSM_Listener.BGPKeepAliveMsgSend(Variables.True);
                    ListenerSocket.BeginSend(data, 0, data.Length, 0, SendCallback, sendSock);
                    //sendOpenDone.Set();

                }
                else if (msg == "KeepAlive")
                {

                    mMessageType = msg;
                    ListenerSocket.BeginSend(data, 0, data.Length, 0, SendCallback, sendSock);
                    //sendKeepAliveDone.Set();
                    //FSM_Listener.BGPKeepAliveMsgSend(Variables.True);

                }
                else if (msg == "Update")
                {
                    mMessageType = msg;
                    ListenerSocket.BeginSend(data, 0, data.Length, 0, SendCallback, sendSock);

                    SendUpdate.WaitOne();
                    //Console.WriteLine("Listener Send update message to speeker");
                }
                else if (msg == "Notify")
                {
                    mMessageType = msg;
                    ListenerSocket.BeginSend(data, 0, data.Length, 0, SendCallback, sendSock);
                    //Console.WriteLine("Listener Send update message to speeker");
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

                // Complete sending the data to the remote device.
                int bytesSent = ListenerSocket.EndSend(result);
                SendUpdate.Set();




                //sendOpenDone.Set();
                //sendKeepAliveDone.Set();


                //sendDone.Set();
                //FSM_Listener.BGPOpenMsgSent(Variables.True);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }
}
