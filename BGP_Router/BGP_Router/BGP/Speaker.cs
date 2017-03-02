uusing System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using BGP_Router.Masiina;
using BGP_Router.Messages;

namespace BGP_Router.BGP
{
    public class Speaker : Router
    {

        public Socket[] tempSocket = new Socket[14];
        FinateStateMachine FSM_Speaker = new FinateStateMachine();
        public bool conectionFlag;
        public int SpeakerID;
        public int ListenerID;
        public string message = "";


        private static AutoResetEvent speakerConnectionRequest = new AutoResetEvent(true);
        private static AutoResetEvent completeSpeakerConnection = new AutoResetEvent(true);
        private static AutoResetEvent BGPSpeakerState = new AutoResetEvent(true);
        private static AutoResetEvent BGPSpeakerOpenMsg = new AutoResetEvent(true);
        private static AutoResetEvent BGPSpeakerOpenMsgState = new AutoResetEvent(true);

        private static AutoResetEvent BGPListenerState = new AutoResetEvent(true);
        private static AutoResetEvent BGPListenerOpenMsgState = new AutoResetEvent(true);
        private static AutoResetEvent BGPListenerUpdateMsgState = new AutoResetEvent(true);
        private static AutoResetEvent BGPListenerUpdateMsg = new AutoResetEvent(true);


        //public int sendTo;


        //since client doesnot need to listen to the connection but it only does connect to server
        public void Connect(string ipAddress, int port, int speaker, int Listener)
        {
            // Connect to a remote device.
            try
            {

                SpeakerID = speaker;
                ListenerID = Listener;

                speakerConnectionRequest.WaitOne();

                //Console.WriteLine("IP ADDRESS: port : speaker ID : Listener ID :"+ipAddress +"  " + port + "  " + speaker + "  " + Listener);

                speakerSocket.BeginConnect(new IPEndPoint(IPAddress.Parse(ipAddress), port), ConnectCallback, speakerSocket);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private void ConnectCallback(IAsyncResult result)
        {
            try
            {
                // we catch that connection we send and since AsyncState is a object so we set it as Socket to get connection
                Socket speakerSocket = result.AsyncState as Socket;

                if (speakerSocket.Connected)
                {
                    conectionFlag = speakerSocket.Connected;

                    //Variables.sucessfulConnection[Variables.i] = conectionFlag;
                    //Variables.i++;
                    Variables.True = conectionFlag;

                    // when one client connection is accepted then it stops accepting other clients by EndAccept
                    speakerSocket.EndConnect(result);

                    speakerConnectionRequest.Set();

                    //Store the speaker socket
                    Variables.SpeakerSocketDictionary.TryAdd(Variables.CurrentSpeakerCount, speakerSocket);
                    Variables.CurrentSpeakerCount++;

                    Variables.ListenerNumber = ListenerID;
                    Variables.SpeakerIpAddress = ((IPEndPoint)speakerSocket.LocalEndPoint).Address.ToString();
                    Variables.ListenerIpAddress = ((IPEndPoint)speakerSocket.RemoteEndPoint).Address.ToString();



                    completeSpeakerConnection.WaitOne();

                    Console.Write("BGP Speaker " + SpeakerID + " : " + IPAddress.Parse(((IPEndPoint)speakerSocket.LocalEndPoint).Address.ToString()) + " Connected to ---->" +
                    "BGP Listener " + ListenerID + " : " + IPAddress.Parse(((IPEndPoint)speakerSocket.RemoteEndPoint).Address.ToString()));

                    FSM_Speaker.TcpConnectionConformed(Variables.True);

                    completeSpeakerConnection.Set();

                    BGPSpeakerState.WaitOne();
                    //connectDone.Set();
                    Console.WriteLine("BGP Listener : {0}| is in state : {1}", Variables.ListenerIpAddress, Variables.ListenerConnectionStatus);
                    BGPSpeakerState.Set();

                }

                buffer = new byte[1024];
                speakerSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceivedCallback, speakerSocket);

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

                BGPListenerState.WaitOne();
                // Read data from the remote device.
                //int bytesRead = client.EndReceive(result);
                // we catch that connection we send and since AsyncState is a object so we set it as Socket to get connection
                Socket speakerSocket = result.AsyncState as Socket;
                int bufferLength = speakerSocket.EndReceive(result);

                byte[] packet = new byte[bufferLength];
                Array.Copy(buffer, packet, packet.Length);
                BGPListenerState.Set();
                // Signal that all bytes have been received.
                // Signal that all bytes have been received.
                //Console.Write("\n"+"BGP Speaker:" + IPAddress.Parse(((IPEndPoint)speakerSocket.LocalEndPoint).Address.ToString()) + " has RECIVED ");

                //Handle packet here
                PacketHandler.Handle(packet, speakerSocket);
                //receiveDone.Set();
                //FSM_Speaker.BGPOpenMsgRecivedSpeaker(Variables.True);


                if (bufferLength == 58)
                {
                    BGPListenerUpdateMsgState.WaitOne();
                    Console.WriteLine("BGP Listener : {0}| is in state : {1}", IPAddress.Parse(((IPEndPoint)speakerSocket.RemoteEndPoint).Address.ToString()), Variables.ListenerConnectionState);
                    BGPListenerUpdateMsgState.Set();
                }
                if (bufferLength == 40)
                {

                    FSM_Speaker.BGPKeepAliveMsgSend(Variables.True);
                    BGPListenerOpenMsgState.WaitOne();
                    Console.WriteLine("BGP Listener : {0}| is in state : {1}", IPAddress.Parse(((IPEndPoint)speakerSocket.RemoteEndPoint).Address.ToString()), Variables.ListenerConnectionState);
                    BGPListenerOpenMsgState.Set();
                }



                buffer = new byte[1024];
                speakerSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceivedCallback, speakerSocket);

            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine("Speaker Connection is Closed");
                // Don't care
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }
        public void SendListener(byte[] data, Socket speaker, string msg)
        {
            try
            {
                Socket speakerSocket = speaker;
                message = msg;
                // Begin sending the data to the remote device.
                speakerSocket.BeginSend(data, 0, data.Length, 0, SendCallback, speakerSocket);
                //BGPListenerUpdateMsg.WaitOne();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());

            }

        }

        public void Send(byte[] data)
        {
            try
            {

                // Begin sending the data to the remote device.
                speakerSocket.BeginSend(data, 0, data.Length, 0, SendCallback, speakerSocket);
                // Console.WriteLine("*********************** Speaker" + IPAddress.Parse(((IPEndPoint)_speakerSocket.LocalEndPoint).Address.ToString())
                //   +"*********************** Listener" + IPAddress.Parse(((IPEndPoint)_speakerSocket.RemoteEndPoint).Address.ToString()));
                //Thread.Sleep(1000);
                //sendDone.WaitOne();


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

                // Retrieve the socket from the state object.
                Socket speakerSocket = result.AsyncState as Socket;


                // Complete sending the data to the remote device.
                int bytesSent = speakerSocket.EndSend(result);



                //sendDone.Set();
                FSM_Speaker.BGPOpenMsgSent(Variables.True);
                if (message == "")
                {
                    BGPSpeakerOpenMsg.WaitOne();

                    Console.WriteLine("BGP Speaker: " + IPAddress.Parse(((IPEndPoint)speakerSocket.LocalEndPoint).Address.ToString()) + " has SEND  OPEN MESSAGE !!");

                    BGPSpeakerOpenMsg.Set();

                    BGPSpeakerOpenMsgState.WaitOne();

                    Console.WriteLine("BGP Speaker : {0}| is in state : {1}", IPAddress.Parse(((IPEndPoint)speakerSocket.LocalEndPoint).Address.ToString()), Variables.speakerConnectionState);

                    BGPSpeakerOpenMsgState.Set();
                }
                if (message == "Update")
                {
                    //BGPListenerUpdateMsg.Set();
                    //Console.WriteLine("BGP Speaker: " + IPAddress.Parse(((IPEndPoint)speakerSocket.LocalEndPoint).Address.ToString()) + " has SEND  UPDATE MESSAGE !!");
                    //Console.WriteLine("BGP Speaker : {0}| is in state : {1}", IPAddress.Parse(((IPEndPoint)speakerSocket.LocalEndPoint).Address.ToString()), Variables.speakerConnectionState);
                }
                if (message == "Notify")
                {

                }


            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
