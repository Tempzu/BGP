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
    
    public class FSM : States
    {
        bool AutoStartEvent;
        bool ConnectRetryExpire;
        bool HoldTimeExpire;
        bool KeepAliveExpire;
        bool TCPConnectionSucceed;
        bool TCPConnectionFail;
        bool BGPHeaderMessageError;
        bool BGPOpenMessageError;
        bool BGPNotifyMessageError;
        bool BGPAutoStop;
        bool BGPOpenMessage;
        bool BGPNotifyMessage;
        bool BGPKeepAliveMessage;
        bool BGPUpdateMessage;
        bool BGPUpdateMessageError;

        private static AutoResetEvent ConnectionType = new AutoResetEvent(true);


        InitializingListenersSpeakers Initialize_BGP = new InitializingListenersSpeakers();

     
        public void IdleStatus()
        {
            
            if (AutoStartEvent == true)
            {
                Variables.ListenerConnectionStatus = "Idle";
                Variables.SpeakerConnectionStatus = "Idle";
             
                Initialize_BGP.StartListener();

                ConnectRetryCounter = 0;
           
                ConnectionRetryTimer_Reset();
                
                Initialize_BGP.StartListening();
              

                Initialize_BGP.StartSpeaker();
                Initialize_BGP.SpeakerConnection_Init();

                AutoStartEvent = false;
                


            }
        }
        
        public void ConnectStatus()
        {
            
            if (ConnectRetryExpire == true)
            {
                Console.WriteLine("Connection Retry Expired Connect Status do stuff here !!");

               
                ConnectionRetryTimer_Reset();
                
                Variables.ListenerConnectionStatus = "Connect";

                ConnectRetryExpire = false;
            }
          
            if (TCPConnectionSucceed == true)
            {
                
                ConnectionRetryTimer_Reset();
       
                Variables.ListenerConnectionStatus = "Connect";
                TCPConnectionSucceed = false;

               
                ConnectionType.WaitOne();

                if (Variables.SpeakerAS[Variables.SpeakerIPAddress] == Variables.ListenerAS[Variables.ListenerIPAddress])
                {
                    Variables.ConnectionStatus = "Internal Connection";
                    Console.WriteLine("!! With :" + Variables.ConnectionStatus);
                }
                else
                {
                    Variables.ConnectionStatus = "External Connection";
                    Console.WriteLine("!! With :" + Variables.ConnectionStatus);
                }
                ConnectionType.Set();

            }
            if (TCPConnectionFail == true)
            {
                Console.WriteLine("TCP Connection Failed Connect Status[Initialize_BGP.connCount] Stuff here !!");
               
                ConnectionRetryTimer_Reset();
               
                Variables.SpeakerConnectionStatus = "Active";
                TCPConnectionFail = false;
            }
           
            if (BGPHeaderMessageError == true || BGPOpenMessageError == true)
            {
                Console.WriteLine("BGP Header Message or Open Message has Error Connect Status do stuff here!!");
              
                ConnectionRetryTimer_Reset();
               
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                Console.WriteLine("Connection Retry Counter = " + ConnectRetryCounter + "Connection Status: " + Variables.ListenerConnectionStatus);
                BGPHeaderMessageError = false;
                BGPOpenMessageError = false;
            }
            if (BGPNotifyMessageError == true)
            {
                Console.WriteLine("BGP Notify Message Error Connect Status do stuff here!!");
                 ConnectionRetryTimer_Reset();
                Variables.ListenerConnectionStatus = "Idle";
                BGPNotifyMessageError = false;
            }
            if (BGPAutoStop == true || HoldTimeExpire == true || KeepAliveExpire == true || BGPOpenMessage == true || BGPNotifyMessage == true || BGPKeepAliveMessage == true ||
                BGPUpdateMessage == true || BGPUpdateMessageError == true)
            {
                
                ConnectionRetryTimer_Reset();
                KeepAliveTimer_Reset();
               
                ConnectRetryCounter++;
           
                ConnectRetryExpire = false;
                BGPAutoStop = false;
                HoldTimeExpire = false;
                KeepAliveExpire = false;
                BGPOpenMessage = false;
                BGPNotifyMessage = false;
                BGPKeepAliveMessage = false;
                BGPUpdateMessage = false;
                BGPUpdateMessageError = false;
            }


        }
        
        public void ActiveStatus()
        {
            
            if (ConnectRetryExpire == true)
            {
                Console.WriteLine("Connection Retry Expried in Active Status Do Stuff here!!");
               
                ConnectionRetryTimer_Reset();
                
                Variables.ListenerConnectionStatus = "Connect";
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
                Console.WriteLine("TCP Connection Fails in Active Status[Initialize_BGP.connCount] Stuff here!!");
               
                ConnectionRetryTimer_Reset();
               
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                Console.WriteLine("Connection Retry Counter = " + ConnectRetryCounter + "Connection Status: " + Variables.ListenerConnectionStatus);
                TCPConnectionFail = false;
            }
           

            if (mBGPOpenMessageReceived == true)
            {
               
                ConnectionRetryTimer_Reset();
            
                Listener Listener = new Listener();
                Listener.SendingOpenMsg_Speaker();



               


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
                Variables.ListenerConnectionStatus = "OpenSent";
                Variables.SpeakerConnectionStatus = "OpenSent";
            }
             if (BGPHeaderMessageError == true || BGPOpenMessageError == true)
            {
                Console.WriteLine("BGP heder message Error or open message Active Status Error Stuff here!!");
                
                ConnectionRetryTimer_Reset();
           
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                Console.WriteLine("Connection Retry Counter = " + ConnectRetryCounter + "Connection Status: " + Variables.ListenerConnectionStatus);
                BGPHeaderMessageError = false;
                BGPOpenMessageError = false;
            }
            if (BGPAutoStop == true || HoldTimeExpire == true || KeepAliveExpire == true || BGPOpenMessage == true || BGPNotifyMessage == true || BGPKeepAliveMessage == true ||
                BGPUpdateMessage == true || BGPUpdateMessageError == true)
            {
                
                ConnectionRetryTimer_Reset();
                KeepAliveTimer_Reset();
             
                ConnectRetryCounter++;
               
                ConnectRetryExpire = false;
                BGPAutoStop = false;
                HoldTimeExpire = false;
                KeepAliveExpire = false;
                BGPOpenMessage = false;
                BGPNotifyMessage = false;
                BGPKeepAliveMessage = false;
                BGPUpdateMessage = false;
                BGPUpdateMessageError = false;
            }
        }
        
        public void OpenSent()
        {
            

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
                Variables.ListenerConnectionStatus = "OpenConform";
                Variables.SpeakerConnectionStatus = "OpenConform";
                mBGPOpenMessageReceived = false;
            }
           
            if (mBGPHeaderError == true || BGPOpenMessageError == true)
            {
                
                ConnectionRetryTimer_Reset();
                
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                mBGPHeaderError = false;
                BGPOpenMessageError = false;
            }
           
            if (BGPNotifyMessageError == true)
            {
                ConnectionRetryTimer_Reset();
                
                Variables.ListenerConnectionStatus = "Idle";
                BGPNotifyMessageError = false;
            }
            if (ConnectRetryExpire == true || KeepAliveExpire == true || BGPNotifyMessage == true || BGPKeepAliveMessage == true ||
               BGPUpdateMessage == true || BGPUpdateMessageError == true)
            {
                

                ConnectionRetryTimer_Reset();
                KeepAliveTimer_Reset();
                
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                ConnectRetryExpire = false;
                KeepAliveExpire = false;
                BGPNotifyMessage = false;
                BGPKeepAliveMessage = false;
                BGPUpdateMessage = false;
                BGPUpdateMessageError = false;
            }
        }
        
        public void OpenConfirm()
        {
           
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
            
                Variables.ListenerConnectionStatus = "Idle";
                mBGPNotifyMessage = false;
            }
            if (mBGPKeepAliveMessage == true)
            {
                HoldTimer_Reset();
                Variables.ListenerConnectionStatus = "Established";
                mBGPKeepAliveMessage = false;
            }
            if (mBGPUpdateMessage == true)
            {
              
                HoldTimer_Reset();
                Variables.ListenerConnectionStatus = "Established";
                mBGPUpdateMessage = false;
            }
           
            if (BGPUpdateMessageError == true)
            {
                
                ConnectionRetryTimer_Reset();
              
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                mBGPUpdateMessageError = false;
            }
            if (ConnectRetryExpire == true || BGPHeaderMessageError == true || BGPOpenMessageError == true)
            {
                
                ConnectionRetryTimer_Reset();
           
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                ConnectRetryExpire = false;
                BGPHeaderMessageError = false;
                BGPOpenMessageError = false;
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
            Console.WriteLine("Automatic Start Event is Fired here");
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
            Console.WriteLine("Automatic Stop Event is Fired here");
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
            sTCPConnectionAcknowledged = TCP_ack;
            TCP_Acknowledged_Event += new EventHandler(SM_TCP_Acked_Event);
        }

        private void SM_TCP_Acked_Event(object sender, EventArgs e)
        {
            Console.WriteLine("TCP Connection ACKED");
          
        }
        public void TCPConnectionConfirmed(bool TCP_con)
        {
            TCPConnectionConfirmed_Event += new EventHandler(SM_TCPConnectionConfirmed_Event);
            sTCPConnectionConfirmedValue = TCP_con;
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
            sTCPConnectionFails = TCP_fail;
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
        public void BGPHeaderError(bool HeaderError)
        {
            BGPHeaderError_Event += new EventHandler(SM_BGPHeaderError_Event);
            sBGPHeaderError = HeaderError;
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
            sBGPOpenMessageReceive = openReceived;
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
            sBGPOpenMessage = openSent;
            BGPOpenMessage_Event -= new EventHandler(SM_BGPOpenMessageSent_Event);
        }

        private void SM_BGPOpenMessageSent_Event(object sender, EventArgs e)
        {
            BGPOpenMessage = true;
            ConnectStatus();
         
            BGPOpenMessage = true;
            ActiveStatus();
            
        }
        public void BGPOpenMsgError(bool openMessageError)
        {
            BGPOpenMessageError_Event += new EventHandler(SM_BGPOpenMessageError_Event);
            sBGPOpenMessageError = openMessageError;
            BGPOpenMessageError_Event -= new EventHandler(SM_BGPOpenMessageError_Event);
        }

        private void SM_BGPOpenMessageError_Event(object sender, EventArgs e)
        {
            Console.WriteLine("BGP Open message Error occured");
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
            sBGPNotifyMessage = notifySent;
            BGPNotifyMessage_Event -= new EventHandler(SM_BGPNotifyMessage_Event);
        }

        private void SM_BGPNotifyMessage_Event(object sender, EventArgs e)
        {
            Console.WriteLine("BGP Notification message Send");
            mBGPNotifyMessage = true;
            ConnectStatus();
           
            mBGPNotifyMessage = true;
            ActiveStatus();
            mBGPNotifyMessage = true;
            OpenSent();
            mBGPNotifyMessage = true;
            EstablishedStatus();
            
        }
        public void BGPNotifyMessageErrorSent(bool notifyErrorMessage)
        {
            BGPNotifyMessageError_Event += new EventHandler(SM_BGPNotifyMessageError_Event);
            sBGPNotifyMessageError = notifyErrorMessage;
            BGPNotifyMessageError_Event -= new EventHandler(SM_BGPNotifyMessageError_Event);
        }

        private void SM_BGPNotifyMessageError_Event(object sender, EventArgs e)
        {
            Console.WriteLine("BGP Notification Error message Send");
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
            sBGPKeepAliveMessage = KeepAliveSent;
            BGPKeepAliveMessage_Event -= new EventHandler(SM_BGPKeepAliveMessageSend_Event);
        }

        private void SM_BGPKeepAliveMessageSend_Event(object sender, EventArgs e)
        {
            
            mBGPKeepAliveMessage = true;
            ConnectStatus();
            mBGPKeepAliveMessage = true;
            ActiveStatus();
            mBGPKeepAliveMessage = true;
            OpenSent();
            mBGPKeepAliveMessage = true;
            EstablishedStatus();
            
        }
        public void BGPUpdateMessageSent(bool updateSent)
        {
            BGPUpdateMessage_Event += new EventHandler(SM_BGPUpdateMessage_Event);
            sBGPUpdateMessage = updateSent;
            BGPUpdateMessage_Event -= new EventHandler(SM_BGPUpdateMessage_Event);
        }

        private void SM_BGPUpdateMessage_Event(object sender, EventArgs e)
        {
            Console.WriteLine("BGP Update message Send");
            mBGPUpdateMessage = true;
            ConnectStatus();
            
            mBGPUpdateMessage = true;
            ActiveStatus();
            mBGPUpdateMessage = true;
            OpenSent();
            mBGPUpdateMessage = true;
            EstablishedStatus();
          
        }
        public void BGPUpdateMsgError(bool updateError)
        {
            BGPUpdateMessageError_Event += new EventHandler(SM_BGPUpdateMessageError_Event);
            sBGPUpdateMessageError = updateError;
            BGPUpdateMessageError_Event -= new EventHandler(SM_BGPUpdateMessageError_Event);
        }

        private void SM_BGPUpdateMessageError_Event(object sender, EventArgs e)
        {
            Console.WriteLine("BGP Update message Error Occured");
            mBGPUpdateMessageError = true;
            ConnectStatus();
            
            mBGPUpdateMessageError = true;
            ActiveStatus();
            mBGPUpdateMessageError = true;
            OpenSent();
            mBGPUpdateMessageError = true;
            EstablishedStatus();
          
        }

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

            Console.WriteLine("Connection Retry Event is Fired here");

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

            Console.WriteLine("Connection Hold Timer is expired Event is Fired here");
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
            Console.WriteLine("Connection KeepAlive timer is expired Event is Fired here");
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
