using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using BGP_Router.Masiina;

namespace BGP_Router.BGP
{
    public class Speaker : Router
    {

        public Socket[] tempSocket = new Socket[14];
        FSM FSM_Speaker = new FSM();
        public bool mConnectionFlag;
        public int mSpeakerID;
        public int mListenerID;
        public string mMessage = "";

        private static AutoResetEvent SpeakerConnectionRequest = new AutoResetEvent(true);
        private static AutoResetEvent CompleteSpeakerConnection = new AutoResetEvent(true);
        private static AutoResetEvent BGPSpeakerState = new AutoResetEvent(true);
        private static AutoResetEvent BGPSpeakerOpenMsg = new AutoResetEvent(true);
        private static AutoResetEvent BGPSpeakerOpenMsgState = new AutoResetEvent(true);
        private static AutoResetEvent BGPListenerState = new AutoResetEvent(true);
        private static AutoResetEvent BGPListenerOpenMsgState = new AutoResetEvent(true);
        private static AutoResetEvent BGPListenerUpdateMsgState = new AutoResetEvent(true);
        private static AutoResetEvent BGPListenerUpdateMsg = new AutoResetEvent(true);

        public void Connect(string ipAddress, int port, int speaker, int listener) //Connect to a router
        {
            try
            {
                mSpeakerID = speaker;
                mListenerID = listener;
                SpeakerConnectionRequest.WaitOne();
                mSocketSpeaker.BeginConnect(new IPEndPoint(IPAddress.Parse(ipAddress), port), ConnectCallback, mSocketSpeaker);
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
                Socket SpeakerSocket = result.AsyncState as Socket;

                if (SpeakerSocket.Connected)
                {
                    mConnectionFlag = SpeakerSocket.Connected;
                    Variables.True = mConnectionFlag;
                    //End accepeting connections after a match
                    SpeakerSocket.EndConnect(result);
                    SpeakerConnectionRequest.Set();
                    //Store the speaker socket
                    Variables.SpeakerSocketDictionary.TryAdd(Variables.CurrentSpeakerCount, SpeakerSocket);
                    Variables.CurrentSpeakerCount++;
                    Variables.ListenerNumber = mListenerID;
                    Variables.SpeakerIPAddress = ((IPEndPoint)SpeakerSocket.LocalEndPoint).Address.ToString();
                    Variables.ListenerIPAddress = ((IPEndPoint)SpeakerSocket.RemoteEndPoint).Address.ToString();
                    CompleteSpeakerConnection.WaitOne();
                    Console.Write("\nConnection info:\nSpeaker " + mSpeakerID + " : " + IPAddress.Parse(((IPEndPoint)SpeakerSocket.LocalEndPoint).Address.ToString()) + " is connected to " +
                    "listener " + mListenerID + " : " + IPAddress.Parse(((IPEndPoint)SpeakerSocket.RemoteEndPoint).Address.ToString()));
                    FSM_Speaker.TCPConnectionConfirmed(Variables.True);
                    CompleteSpeakerConnection.Set();
                    BGPSpeakerState.WaitOne();
                    Console.WriteLine("\n---- Listener : {0} is in {1} state ---- ", Variables.ListenerIPAddress, Variables.ListenerConnectionStatus);
                    BGPSpeakerState.Set();

                }

                mBuffer = new byte[1024];
                SpeakerSocket.BeginReceive(mBuffer, 0, mBuffer.Length, SocketFlags.None, ReceivedCallback, SpeakerSocket);

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
                Socket speakerSocket = result.AsyncState as Socket;
                int bufferLength = speakerSocket.EndReceive(result);
                byte[] packet = new byte[bufferLength];
                Array.Copy(mBuffer, packet, packet.Length);
                BGPListenerState.Set();
                HandlePackets.Handle(packet, speakerSocket);

                if (bufferLength == 58)
                {
                    BGPListenerUpdateMsgState.WaitOne();
                    Console.WriteLine("---- Listener : {0} is in {1} state -----", IPAddress.Parse(((IPEndPoint)speakerSocket.RemoteEndPoint).Address.ToString()), Variables.ListenerConnectionStatus);
                    BGPListenerUpdateMsgState.Set();
                }
                if (bufferLength == 40)
                {

                    FSM_Speaker.BGPKeepAliveMessageSend(Variables.True);
                    BGPListenerOpenMsgState.WaitOne();
                    Console.WriteLine("---- Listener : {0} is in {1} state -----", IPAddress.Parse(((IPEndPoint)speakerSocket.RemoteEndPoint).Address.ToString()), Variables.ListenerConnectionStatus);
                    BGPListenerOpenMsgState.Set();
                }



                mBuffer = new byte[1024];
                speakerSocket.BeginReceive(mBuffer, 0, mBuffer.Length, SocketFlags.None, ReceivedCallback, speakerSocket);

            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine("Speaker Connection is Closed");
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
                mMessage = msg;
                speakerSocket.BeginSend(data, 0, data.Length, 0, SendCallback, speakerSocket);
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
                mSocketSpeaker.BeginSend(data, 0, data.Length, 0, SendCallback, mSocketSpeaker);
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
                Socket speakerSocket = result.AsyncState as Socket;
                int bytesSent = speakerSocket.EndSend(result);
                FSM_Speaker.BGPOpenMessageSent(Variables.True);
                if (mMessage == "")
                {
                    BGPSpeakerOpenMsg.WaitOne();
                    Console.WriteLine("#### BGP Speaker: " + IPAddress.Parse(((IPEndPoint)speakerSocket.LocalEndPoint).Address.ToString()) + " has sent an OPEN message ####");
                    BGPSpeakerOpenMsg.Set();
                    BGPSpeakerOpenMsgState.WaitOne();
                    Console.WriteLine("---- Speaker : {0} is in {1} state ----", IPAddress.Parse(((IPEndPoint)speakerSocket.LocalEndPoint).Address.ToString()), Variables.SpeakerConnectionStatus);
                    BGPSpeakerOpenMsgState.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
