using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using BGP_Router.Messages;
using BGP_Router.BGP;
using BGP_Router.Masiina;
using System.Net;


namespace BGP_Router.Masiina
{
    // The BGP must maintain a separate FSM for each configured peer.
    public class FSM : States
    {
        bool AutoStartEvent;
        bool ConnectRetryExpire;
        bool HoldTimeExpire;
        bool KeepAliveExpire;
        bool TCPConnectionSucceed;
        bool TCPConnectionFail;
        bool BGPHeaderMessageError;
        bool mBGPOpenMsgError;
        bool mBGPNotifyMsgError;
        bool BGPAutoStop;
        bool BGPOpenMsg;
        bool BGPNotifyMsg;
        bool BGPKeepAliveMsg;
        bool BGPUpdateMsg;
        bool mBGPUpdateMsgError;

        private static AutoResetEvent ConnectionType = new AutoResetEvent(true);


        InitializingListenersSpeakers Initialize_BGP = new InitializingListenersSpeakers();
        
        public void IdleStatus()
        {
            
            if (AutoStartEvent == true)
            {
                Variables.ListenerConnectionStatus = "Idle";
                Variables.SpeakerConnectionStatus = "Idle";
                
                //Initialize BGP resources for connections
                Initialize_BGP.StartListener();

                //Set the connect retry to 0
                ConnectRetryCounter = 0;
                ConnectionRetryTimer_Reset();
               
                // Start BGP listening and speaking 
                Initialize_BGP.StartListening();
                Initialize_BGP.StartSpeaker();
                Initialize_BGP.SpeakerConnection_Init();

                AutoStartEvent = false;
            }
        }
       
        public void ConnectStatus()
        {
            // Method for waiting for the TCP connection to complete

            if (ConnectRetryExpire == true)
            {
                Console.WriteLine("Connection retry is expired");
                // reset the timer
                ConnectionRetryTimer_Reset();
                //initiates a TCP Connection to the other BGP peer
                Variables.ListenerConnectionStatus = "Connected";
                ConnectRetryExpire = false;
            }
            //If the TCP Connection Succeeds
            if (TCPConnectionSucceed == true)
            {
                ConnectionRetryTimer_Reset();
                //stays in the Connected Status

                Variables.ListenerConnectionStatus = "Connect";
                TCPConnectionSucceed = false;

                // The value of the AS field will determine whether or not the connection is internal or external. If the field = local AS number -> internal
                ConnectionType.WaitOne();

                if (Variables.SpeakerAS[Variables.SpeakerIPAddress] == Variables.ListenerAS[Variables.ListenerIPAddress])
                {
                    Variables.ConnectionStatus = "Internal Connection";
                    Console.WriteLine("/ With :" + Variables.ConnectionStatus);
                }
                else
                {
                    Variables.ConnectionStatus = "External Connection";
                    Console.WriteLine("/ With :" + Variables.ConnectionStatus);
                }
                ConnectionType.Set();

            }
            if (TCPConnectionFail == true)
            {
                Console.WriteLine("Failure in TCP connection!");
                ConnectionRetryTimer_Reset();
                //Listening continues, change of status
                Variables.SpeakerConnectionStatus = "Active";
                TCPConnectionFail = false;
            }
           
            if (BGPHeaderMessageError == true || mBGPOpenMsgError == true)
            {
                Console.WriteLine("Error in BGPHeaderMessage or BGPOpenMessage!");
                ConnectionRetryTimer_Reset();
                // TCP connection is dropped, change of status
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                Console.WriteLine("Counting connection retries = " + ConnectRetryCounter + "Connection Status: " + Variables.ListenerConnectionStatus);
                BGPHeaderMessageError = false;
                mBGPOpenMsgError = false;
            }
            if (mBGPNotifyMsgError == true)
            {
                Console.WriteLine("Error in BGPNotifyMessage!");
                ConnectionRetryTimer_Reset();
                // TCP connection is dropped, change of status
                Variables.ListenerConnectionStatus = "Idle";
                mBGPNotifyMsgError = false;
            }
            if (BGPAutoStop == true || HoldTimeExpire == true || KeepAliveExpire == true || BGPOpenMsg == true || BGPNotifyMsg == true || BGPKeepAliveMsg == true ||
                BGPUpdateMsg == true || mBGPUpdateMsgError == true)
            {
                ConnectionRetryTimer_Reset();
                KeepAliveTimer_Reset();
                // TCP connection is dropped
                ConnectRetryCounter++;

                ConnectRetryExpire = false;
                BGPAutoStop = false;
                HoldTimeExpire = false;
                KeepAliveExpire = false;
                BGPOpenMsg = false;
                BGPNotifyMsg = false;
                BGPKeepAliveMsg = false;
                BGPUpdateMsg = false;
                mBGPUpdateMsgError = false;
            }            
        }
       
        public void ActiveStatus()
        {
            //Acquiring a TCP peer connection
            if (ConnectRetryExpire == true)
            {
                Console.WriteLine("Connection retry expired!");
                ConnectionRetryTimer_Reset();
                //Initiazing another connection
                Variables.ListenerConnectionStatus = "Connected";
                ConnectRetryExpire = false;
            }
            if (TCPConnectionSucceed == true)
            {
                ConnectionRetryTimer_Reset();
                Variables.SpeakerConnectionStatus = "Active";
                TCPConnectionSucceed = false;
            }

            if (TCPConnectionFail == true)
            {
                Console.WriteLine("Failure in TCP connection!");
                ConnectionRetryTimer_Reset();
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                Console.WriteLine("Counting connection retries = " + ConnectRetryCounter + "Connection Status: " + Variables.ListenerConnectionStatus);
                TCPConnectionFail = false;
            }

            if (mBGPOpenMessageReceived == true)
            {
                ConnectionRetryTimer_Reset();
                //send OPEN message
                Listener Listener = new Listener();
                Listener.SendingOpenMsg_Speaker();
                //send KeepAlive message
                Listener.SendingKeepAliveMsg_Speaker();

                if (HoldTimer != null)
                {
                    KeepAliveTimer_Reset();
                    HoldTimer_Reset();

                }
                else if (HoldTimer == null)
                {
                    KeepAliveTimer_Reset();
                    HoldTimer_Reset();

                }
                Variables.ListenerConnectionStatus = "Open message sent";
                Variables.SpeakerConnectionStatus = "Open message sent";
            }
            
            if (BGPHeaderMessageError == true || mBGPOpenMsgError == true)
            {
                Console.WriteLine("Error in BGPHeaderMessage or BGPOpenMessage");

                ConnectionRetryTimer_Reset();
                //TCP connection dropped, change of status
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                Console.WriteLine("Counting connection retries = " + ConnectRetryCounter + "Connection Status: " + Variables.ListenerConnectionStatus);
                BGPHeaderMessageError = false;
                mBGPOpenMsgError = false;
            }
            if (BGPAutoStop == true || HoldTimeExpire == true || KeepAliveExpire == true || BGPOpenMsg == true || BGPNotifyMsg == true || BGPKeepAliveMsg == true ||
                BGPUpdateMsg == true || mBGPUpdateMsgError == true)
            {
                //Console.WriteLine("Errors in active status!");
                ConnectionRetryTimer_Reset();
                KeepAliveTimer_Reset();
                //TCP connection is dropped
                ConnectRetryCounter++;
                ConnectRetryExpire = false;
                BGPAutoStop = false;
                HoldTimeExpire = false;
                KeepAliveExpire = false;
                BGPOpenMsg = false;
                BGPNotifyMsg = false;
                BGPKeepAliveMsg = false;
                BGPUpdateMsg = false;
                mBGPUpdateMsgError = false;
            }

        }
       
        public void OpenSent()
        {
            //This status is for waiting an OPEN message

            if (BGPAutoStop == true)
            {
                ConnectionRetryTimer_Reset();
                //TCP connection is dropped
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                BGPAutoStop = false;
            }
            if (HoldTimeExpire == true)
            {
                ConnectionRetryTimer_Reset();
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                HoldTimeExpire = false;
            }
            if (TCPConnectionFail == true)
            {
                ConnectionRetryTimer_Reset();
                Variables.ListenerConnectionStatus = "Active";
                TCPConnectionFail = false;
            }
            
            if (mBGPOpenMessageReceived == true)
            {
                ConnectionRetryTimer_Reset();
                KeepAliveTimer_Reset();
                HoldTimer_Reset();
                Variables.ListenerConnectionStatus = "Open confirmed";
                Variables.SpeakerConnectionStatus = "Open confirmed";
                mBGPOpenMessageReceived = false;
            }

            if (mBGPHeaderError == true || mBGPOpenMsgError == true)
            {
                ConnectionRetryTimer_Reset();
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                mBGPHeaderError = false;
                mBGPOpenMsgError = false;
            }
           
            if (mBGPNotifyMsgError == true)
            {
                ConnectionRetryTimer_Reset();
                Variables.ListenerConnectionStatus = "Idle";
                mBGPNotifyMsgError = false;
            }
            if (ConnectRetryExpire == true || KeepAliveExpire == true || BGPNotifyMsg == true || BGPKeepAliveMsg == true ||
               BGPUpdateMsg == true || mBGPUpdateMsgError == true)
            {
                ConnectionRetryTimer_Reset();
                KeepAliveTimer_Reset();
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                ConnectRetryExpire = false;
                KeepAliveExpire = false;
                BGPNotifyMsg = false;
                BGPKeepAliveMsg = false;
                BGPUpdateMsg = false;
                mBGPUpdateMsgError = false;
            }

        }
       
        public void OpenConfirm()
        {
            //FSM waits for KeepAlive or Notification message
            if (BGPAutoStop == true)
            {
                ConnectionRetryTimer_Reset();
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                BGPAutoStop = false;
            }
            if (HoldTimeExpire == true)
            {
                ConnectionRetryTimer_Reset();
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                HoldTimeExpire = false;
            }
            if (KeepAliveExpire == true)
            {
                Listener Listener = new Listener();
                Listener.KeepAliveExpired();
                KeepAliveTimer_Reset();
                ConnectionRetryTimer_Reset();
                Variables.ListenerConnectionStatus = "Idle";
                KeepAliveExpire = false;
                ConnectRetryExpire = false;
            }
            
            if (TCPConnectionFail == true)
            {
                ConnectionRetryTimer_Reset();
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                TCPConnectionFail = false;
            }
            if (mBGPNotifyMessageError == true)
            {
                ConnectionRetryTimer_Reset();
                Variables.ListenerConnectionStatus = "Idle";
                mBGPNotifyMessageError = false;
            }
           

            if (mBGPHeaderError == true || mBGPOpenMessageError == true)
            {
                ConnectionRetryTimer_Reset();
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                mBGPHeaderError = false;
                mBGPOpenMessageError = false;
            }
        }
        
        public void EstablishedStatus()
        {
            //The FSM can exchange Update, Notification or KeepAlive messages with the BGP peers
            if (BGPAutoStop == true)
            {
                ConnectionRetryTimer_Reset();
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                BGPAutoStop = false;
            }
            if (HoldTimeExpire == true)
            {
                ConnectionRetryTimer_Reset();
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                HoldTimeExpire = false;
            }
            if (KeepAliveExpire == true)
            {
                KeepAliveTimer_Reset();
                ConnectionRetryTimer_Reset();
                KeepAliveExpire = false;
                ConnectRetryExpire = false;
            }
            
            if (mBGPNotifyMessage == true)
            {
                ConnectionRetryTimer_Reset();
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                mBGPNotifyMessage = false;
            }
            if (BGPKeepAliveMsg == true)
            {
                HoldTimer_Reset();
                Variables.ListenerConnectionStatus = "Established";
                BGPKeepAliveMsg = false;
            }
            if (BGPUpdateMsg == true)
            {
                HoldTimer_Reset();
                Variables.ListenerConnectionStatus = "Established";
                BGPUpdateMsg = false;
            }
           
            if (mBGPUpdateMsgError == true)
            {
                ConnectionRetryTimer_Reset();
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                mBGPUpdateMsgError = false;
            }
            if (ConnectRetryExpire == true || BGPHeaderMessageError == true || mBGPOpenMsgError == true)
            {
                ConnectionRetryTimer_Reset();
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                ConnectRetryExpire = false;
                BGPHeaderMessageError = false;
                mBGPOpenMsgError = false;
            }
        }

        public void StartBGPConnectionMethod(bool start)
        {

            OnAutomaticStartEvent += new EventHandler(SM_OnAutomaticStartEvent);
            AutomaticStart = start;
            OnAutomaticStartEvent -= new EventHandler(SM_OnAutomaticStartEvent);
        }
        private void SM_OnAutomaticStartEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Starting simulation!");
            AutoStartEvent = Variables.True;
            IdleStatus();
        }
        public void StopBGPConnectionMethod(bool stop)
        {

            OnAutomaticStopEvent += new EventHandler(SM_OnAutomaticStopEvent);
            AutomaticStop = stop;
        }

        private void SM_OnAutomaticStopEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Stopping simulation!");
            BGPAutoStop = true;
            ConnectStatus();
            BGPAutoStop = true;
            ActiveStatus();
            BGPAutoStop = true;
            OpenSent();
            BGPAutoStop = true;
            OpenConfirm();
            BGPAutoStop = true;
            EstablishedStatus();
        }
        public void TCPConnectionAcknowledge(bool TCP_ack)
        {
            TCP_Acknowledged_Event += new EventHandler(SM_TCP_Acked_Event);
            TCPConnectionAcknowledged = TCP_ack;
            TCP_Acknowledged_Event += new EventHandler(SM_TCP_Acked_Event);
        }

        private void SM_TCP_Acked_Event(object sender, EventArgs e)
        {
            Console.WriteLine("TCP Connection ACK");
        }
        public void TCPConnectionConfirmed(bool TCP_con)
        {
            TCPConnectionConfirmed_Event += new EventHandler(SM_TCPConnectionConfirmed_Event);
            TCPConnectionConfirmedValue = TCP_con;
            TCPConnectionConfirmed_Event += new EventHandler(SM_TCPConnectionConfirmed_Event);
        }

        private void SM_TCPConnectionConfirmed_Event(object sender, EventArgs e)
        {
            TCPConnectionSucceed = Variables.True;
            ConnectStatus();
            TCPConnectionSucceed = Variables.True;
            ActiveStatus();
        }
        public void TCPConnectionFailed(bool TCP_fail)
        {
            TCPConnectionFails_Event += new EventHandler(SM_TCPConnectionFails_Event);
            TCPConnectionFails = TCP_fail;
            TCPConnectionFails_Event -= new EventHandler(SM_TCPConnectionFails_Event);

        }

        private void SM_TCPConnectionFails_Event(object sender, EventArgs e)
        {
            Console.WriteLine("TCP Connection Failled");
            TCPConnectionFail = true;
            ConnectStatus();
            TCPConnectionFail = true;
            ActiveStatus();
            TCPConnectionFail = true;
            OpenSent();
            TCPConnectionFail = true;
            OpenConfirm();
        }
        public void BGPHederError(bool BGPHederError)
        {
            BGPHeaderError_Event += new EventHandler(SM_BGPHeaderError_Event);
            BGPHeaderError = BGPHeaderError;
            BGPHeaderError_Event -= new EventHandler(SM_BGPHeaderError_Event);
        }

        private void SM_BGPHeaderError_Event(object sender, EventArgs e)
        {
            Console.WriteLine("BGP Header Message Error");
            BGPHeaderMessageError = true;
            ConnectStatus();
            BGPHeaderMessageError = true;
            ActiveStatus();
            BGPHeaderMessageError = true;
            OpenSent();
            BGPHeaderMessageError = true;
            OpenConfirm();
            BGPHeaderMessageError = true;
            EstablishedStatus();
        }
        public void BGPOpenMessageReceived(bool openReceived)
        {
            BGPOpenMessageReceived_Event += new EventHandler(SM_BGPOpenMessageReceived_Event);
            BGPOpenMessageReceive = openReceived;
            BGPOpenMessageReceived_Event += new EventHandler(SM_BGPOpenMessageReceived_Event);
        }

        private void SM_BGPOpenMessageReceived_Event(object sender, EventArgs e)
        {
            mBGPOpenMessageReceived = Variables.True;
            ActiveStatus();
            mBGPOpenMessageReceived = Variables.True;
            OpenSent();
        }


        public void BGPOpenMessageSent(bool openSent)
        {
            BGPOpenMessage_Event += new EventHandler(SM_BGPOpenMessageSent_Event);
            BGPOpenMessage = openSent;
            BGPOpenMessage_Event -= new EventHandler(SM_BGPOpenMessageSent_Event);
        }

        private void SM_BGPOpenMessageSent_Event(object sender, EventArgs e)
        {
            BGPOpenMsg = true;
            ConnectStatus();
            BGPOpenMsg = true;
            ActiveStatus();
        }
        public void BGPOpenMsgError(bool openMessageError)
        {
            BGPOpenMessageError_Event += new EventHandler(SM_BGPOpenMessageError_Event);
            BGPOpenMessageError = openMessageError;
            BGPOpenMessageError_Event -= new EventHandler(SM_BGPOpenMessageError_Event);
        }

        private void SM_BGPOpenMessageError_Event(object sender, EventArgs e)
        {
            Console.WriteLine("BGP Open message Error");
            mBGPOpenMessageError = true;
            ConnectStatus();
            mBGPOpenMessageError = true;
            ActiveStatus();
            mBGPOpenMessageError = true;
            OpenSent();
            mBGPOpenMessageError = true;
            OpenConfirm();
            mBGPOpenMessageError = true;
            EstablishedStatus();
        }
        public void BGPNotifyMessageSent(bool notifySent)
        {
            BGPNotifyMessage_Event += new EventHandler(SM_BGPNotifyMessage_Event);
            BGPNotifyMessage = notifySent;
            BGPNotifyMessage_Event -= new EventHandler(SM_BGPNotifyMessage_Event);
        }

        private void SM_BGPNotifyMessage_Event(object sender, EventArgs e)
        {
            Console.WriteLine("BGP Notification message sent");
            BGPNotifyMsg = true;
            ConnectStatus();
            BGPNotifyMsg = true;
            ActiveStatus();
            BGPNotifyMsg = true;
            OpenSent();
            BGPNotifyMsg = true;
            EstablishedStatus();
            //throw new NotImplementedException();
        }
        public void BGPNotifyMessageErrorSent(bool notifyErrorMessage)
        {
            BGPNotifyMessageError_Event += new EventHandler(SM_BGPNotifyMessageError_Event);
            BGPNotifyMessageError = notifyErrorMessage;
            BGPNotifyMessageError_Event -= new EventHandler(SM_BGPNotifyMessageError_Event);
        }

        private void SM_BGPNotifyMessageError_Event(object sender, EventArgs e)
        {
            Console.WriteLine("BGP Notification error message sent");
            mBGPNotifyMessageError = true;
            ConnectStatus();
            mBGPNotifyMessageError = true;
            OpenSent();
            mBGPNotifyMessageError = true;
            OpenConfirm();
        }
        public void BGPKeepAliveMessageSend(bool KeepAliveSent)
        {
            BGPKeepAliveMessage_Event += new EventHandler(SM_BGPKeepAliveMessageSend_Event);
            BGPKeepAliveMessage = KeepAliveSent;
            BGPKeepAliveMessage_Event -= new EventHandler(SM_BGPKeepAliveMessageSend_Event);
        }

        private void SM_BGPKeepAliveMessageSend_Event(object sender, EventArgs e)
        {
            BGPKeepAliveMsg = true;
            ConnectStatus();
            BGPKeepAliveMsg = true;
            ActiveStatus();
            BGPKeepAliveMsg = true;
            OpenSent();
            BGPKeepAliveMsg = true;
            EstablishedStatus();
        }
        public void BGPUpdateMessageSent(bool updateSent)
        {
            BGPUpdateMessage_Event += new EventHandler(SM_BGPUpdateMessage_Event);
            BGPUpdateMessage = updateSent;
            BGPUpdateMessage_Event -= new EventHandler(SM_BGPUpdateMessage_Event);
        }

        private void SM_BGPUpdateMessage_Event(object sender, EventArgs e)
        {
            Console.WriteLine("BGP Update message sent");
            BGPUpdateMsg = true;
            ConnectStatus();
            BGPUpdateMsg = true;
            ActiveStatus();
            BGPUpdateMsg = true;
            OpenSent();
            BGPUpdateMsg = true;
            EstablishedStatus();
        }
        public void BGPUpdateMsgError(bool updateError)
        {
            BGPUpdateMessageError_Event += new EventHandler(SM_BGPUpdateMessageError_Event);
            BGPUpdateMessageError = updateError;
            BGPUpdateMessageError_Event -= new EventHandler(SM_BGPUpdateMessageError_Event);
        }

        private void SM_BGPUpdateMessageError_Event(object sender, EventArgs e)
        {
            Console.WriteLine("BGP Update message Error");
            mBGPUpdateMessageError = true;
            ConnectStatus();
            mBGPUpdateMessageError = true;
            ActiveStatus();
            mBGPUpdateMessageError = true;
            OpenSent();
            mBGPUpdateMessageError = true;
            EstablishedStatus();
            }


        //Timers are implemented here
        bool ConnectionRetryFlag;
        bool holdTimerFlag;
        bool KeepAliveTimerFlag;


        public void Timers()
        {

            ConnectionRetryTimer_Reset();
            HoldTimer_Reset();
            KeepAliveTimer_Reset();
        }
        public void ConnectionRetryTimer_Reset()
        {

            if (ConnectionRetryFlag == true)
            {
                mConnectRetryTimer.Close();

                ConnectionRetryFlag = false;
            }
            mConnectRetryTimer = new System.Timers.Timer(420000);

            ConnectionRetryFlag = true;
            mConnectRetryTimer.Elapsed += OnConnectionRetryExpire;
            mConnectRetryTimer.AutoReset = false;

            mConnectRetryTimer.Enabled = true;
        }
        public void HoldTimer_Reset()
        {
            if (holdTimerFlag == true)
            {
                HoldTimer.Close();

                holdTimerFlag = false;
            }

            holdTimer = new System.Timers.Timer(540000);

            holdTimerFlag = true;
            holdTimer.Elapsed += OnHoldTimerExpire;

            holdTimer.AutoReset = false;

            holdTimer.Enabled = true;
        }
        public void KeepAliveTimer_Reset()
        {

            if (KeepAliveTimerFlag == true)
            {
                mKeepAliveTimer.Close();

                KeepAliveTimerFlag = false;
            }

            mKeepAliveTimer = new System.Timers.Timer(400000);

            KeepAliveTimerFlag = true;
            mKeepAliveTimer.Elapsed += OnKeepAliveTimerExpire;

            mKeepAliveTimer.AutoReset = false;

            mKeepAliveTimer.Enabled = true;
        }
        private void OnKeepAliveTimerExpire(object sender, ElapsedEventArgs e)
        {
            KeepaliveTimer_Expire += new EventHandler(SM_StopConnectionKeepAliveEvent);
            KeepAliveTime = e.SignalTime;
            KeepaliveTimer = mKeepAliveTimer;
            KeepaliveTimer_Expire -= new EventHandler(SM_StopConnectionKeepAliveEvent);
           }

        private void OnHoldTimerExpire(object sender, ElapsedEventArgs e)
        {
            HoldTimer_Expire += new EventHandler(SM_StopConnectionHoldEvent);
            holdTime = e.SignalTime;
            HoldTimer = holdTimer;
            HoldTimer_Expire -= new EventHandler(SM_StopConnectionHoldEvent);
           }

        private void OnConnectionRetryExpire(object sender, ElapsedEventArgs e)
        {
            ConnectRetryTimer_Expire += new EventHandler(SM_StopConnectionRetryEvent);
            ConnectRetryTime = e.SignalTime;
            ConnectionRetryTimer = mConnectRetryTimer;
            ConnectRetryTimer_Expire -= new EventHandler(SM_StopConnectionRetryEvent);
           }

        private void SM_StopConnectionRetryEvent(object sender, EventArgs e)
        {

            Console.WriteLine("Connection retry!");

            ConnectRetryExpire = true;
            ConnectStatus();
            ConnectRetryExpire = true;
            ActiveStatus();
            ConnectRetryExpire = true;
            OpenSent();
            ConnectRetryExpire = true;
            EstablishedStatus();
        }

        private void SM_StopConnectionHoldEvent(object sender, EventArgs e)
        {

            Console.WriteLine("Connection hold timer retry!");
            HoldTimeExpire = true;
            ConnectStatus();
            HoldTimeExpire = true;
            ActiveStatus();
            HoldTimeExpire = true;
            OpenSent();
            HoldTimeExpire = true;
            OpenConfirm();
            HoldTimeExpire = true;
            EstablishedStatus();
            }

        private void SM_StopConnectionKeepAliveEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Connection KeepAlive timer retry!");
            KeepAliveExpire = true;
            ConnectStatus();
            KeepAliveExpire = true;
            OpenSent();
            KeepAliveExpire = true;
            OpenConfirm();
            KeepAliveExpire = true;
            EstablishedStatus();
        }

    }
}
