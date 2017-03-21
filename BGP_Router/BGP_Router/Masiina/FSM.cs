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
/**
    8.2.  DescrIPtion of FSM
        8.2.1.  FSM Definition 
            BGP MUST maintain a separate FSM for each configured peer.  Each BGP peer paired in a potential Connection will attempt to Connect to the
        other, unless configured to remain in the idle Status, or configured to remain passive.  For the purpose of this discussion, the active or
        Connecting side of the TCP Connection (the side of a TCP Connection sending the first TCP SYN packet) is called outgoing.  The passive or
        listening side (the sender of the first SYN/ACK) is called an incoming Connection.  (See Section 8.2.1.1 for information on the terms active and passive used below.)
            A BGP implementation MUST Connect to and listen on TCP port 179 for incoming Connections in addition to trying to Connect to peers.  For
        each incoming Connection, a Status machine MUST be instantiated. There exists a period in which the identity of the peer on the other
        end of an incoming Connection is known, but the BGP identifier is not known.  During this time, both an incoming and outgoing Connection
        may exist for the same configured peering.  This is refErrored to as a Connection collision (see Section 6.8).
            A BGP implementation will have, at most, one FSM for each configured peering, plus one FSM for each incoming TCP Connection for which the
        peer has not yet been identified.  Each FSM corresponds to exactly one TCP Connection.
             There may be more than one Connection between a pair of peers if the Connections are configured to use a different pair of IP addresses.
        This is refErrored to as multIPle "configured peerings" to the same peer.
    8.2.1.1.  Terms "active" and "passive"
             The words active and passive have slightly different meanings when applied to a TCP Connection or a peer.  There is only one active side and one
        passive side to any one TCP Connection, per the definition above and the Status machine below.  When a BGP speaker is configured as active,
        it may end up on either the active or passive side of the Connection that eventually gets established.  Once the TCP Connection is
        completed, it doesn't matter which end was active and which was passive.  The only difference is in which side of the TCP Connection has port number 179.
   8.2.1.2.  FSM and Collision Detection
            There is one FSM per BGP Connection.  When the Connection collision occurs prior to determining what peer a Connection is associated
        with, there may be two Connections for one peer.  After the Connection collision is resolved (see Section 6.8), the FSM for the
        Connection that is closed SHOULD be disposed.
  8.2.1.3.  FSM and Optional Session Attributes
            Optional Session Attributes specify either attributes that act as flags (TRUE or FALSE) or optional timers.  For optional attributes
        that act as flags, if the optional session attribute can be set to TRUE on the system, the corresponding BGP FSM actions must be
        supported.  For example, if the following options can be set in a BGP implementation: AutoStart and PassiveTCPEstablishment, then Events 3,
        4 and 5 must be supported.  If an Optional Session attribute cannot be set to TRUE, the events supporting that set of options do not have to be supported.
     Each of the optional timers (DelayOpenTimer and IdleHoldTimer) has a group of attributes that are:
            - flag indicating support, - Time set in Timer - Timer.
     The two optional timers show this format:
            DelayOpenTimer: DelayOpen, DelayOpenTime, DelayOpenTimer
            IdleHoldTimer:  DampPeerOscillations, IdleHoldTime, IdleHoldTimer
   8.2.1.4.  FSM Event Numbers
            The Event numbers (1-28) utilized in this Status machine descrIPtion aid in specifying the behavior of the BGP Status machine.
        Implementations MAY use these numbers to provide network management information.  The exact form of an FSM or the FSM events are specific to each implementation.
   8.2.1.5.  FSM Actions that are Implementation Dependent
        At certain points, the BGP FSM specifies that BGP initialization will occur or that BGP resources will be deleted.  The initialization of
    the BGP FSM and the associated resources depend on the policy portion of the BGP implementation.  The details of these actions are outside the scope of the FSM document.
 ********* Actual implementation Part************
    Idle Status[Initialize_BGP.connCount]:
    Connect Status[Initialize_BGP.connCount]:
  
   Active Status[Initialize_BGP.connCount]:
            
            If the DelayOpen attribute is set to FALSE, the local system:
                    - sets the ConnectRetryTimer to zero,
                    - completes the BGP initialization,
                    - sends the OPEN message to its peer,
                    - sets its HoldTimer to a large value, and
                    - changes its Status to OpenSent.
            
            
    OpenSent:
            
      OpenConfirm Status[Initialize_BGP.connCount]:
            In this Status, BGP waits for a KeepAlive or NOTIFICATION message.
            Any start event (Events 1, 3-7) is ignored in the OpenConfirm Status.
           
            If a TCP Connection is attempted with an invalid port (Event 15), the local system will ignore the second Connection attempt.
            If the local system receives a valid OPEN message (BGPOpen (Event 19)), the collision detect function is processed per Section 6.8.
            If this Connection is to be dropped due to Connection collision, the local system:
                    - sends a NOTIFICATION with a Cease,
                    - sets the ConnectRetryTimer to zero,
                    - releases all BGP resources,
                    - drops the TCP Connection (send TCP FIN),
                    - increments the ConnectRetryCounter by 1,
                    - (optionally) performs peer oscillation damping if the DampPeerOscillations attribute is set to TRUE, and
                    - changes its Status to Idle.
           
            
    Established Status[Initialize_BGP.connCount]:
           
            Each time the local system sends a KeepAlive or UPDATE message, it restarts its KeepAliveTimer, unless the negotiated HoldTime value is zero.
            A TCPConnection_Valid (Event 14), received for a valid port, will cause the second Connection to be tracked.
            In response to an indication that the TCP Connection is successfully established (Event 16 or Event 17), the second Connection SHALL be tracked 
            until it sends an OPEN message.
            
**/

namespace BGP_Router.Masiina
{
    /**
     8.2.2.  Finite Status[Initialize_BGP.connCount] Machine
        Idle Status:
            Initially, the BGP peer FSM is in the Idle Status.Hereafter, the BGP peer FSM will be shortened to BGP FSM.In this Status, BGP FSM refuses all incoming BGP Connections for
        this peer.No resources are allocated to the peer.
      
        
        In response to AutomaticStart_with_PassiveTCPEstablishment event (Event 5), the local system:
                    - initializes all BGP resources,
                    - sets the ConnectRetryCounter to zero,
                    - starts the ConnectRetryTimer with the initial value,
                    - listens for a Connection that may be initiated by the remote peer, and
                    - changes its Status to Active.
       
            
       Connect Status[Initialize_BGP.connCount]:
            
            If the DelayOpen attribute is set to FALSE, the local system:
                    - stops the ConnectRetryTimer (if running) and sets the ConnectRetryTimer to zero,
                    - completes BGP initialization
                    - sends an OPEN message to its peer,
                    - sets the HoldTimer to a large value, and
                    - changes its Status to OpenSent.
           
    **/
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
        //public bool BGPOpenMessageReceived;
        bool BGPNotifyMsg;
        bool BGPKeepAliveMsg;
        bool BGPUpdateMsg;
        bool mBGPUpdateMsgError;

        private static AutoResetEvent ConnectionType = new AutoResetEvent(true);


        InitilizingListenerSpeaker Initialize_BGP = new InitilizingListenerSpeaker();

        //public string Status[Initialize_BGP.connCount];
        //StatusMachine SM = new StatusMachine();
        //BGPTimers BGPTimers = new BGPTimers();
        /**
        Idle Status[Initialize_BGP.connCount]
           In response an AutomaticStart event (Event 3), the local system:
                   - initializes all BGP resources for the peer Connection,
                   - sets ConnectRetryCounter to zero,
                   - starts the ConnectRetryTimer with the initial value,
                   - initiates a TCP Connection to the other BGP peer,
                   - listens for a Connection that may be initiated by the remote BGP peer, and
                   - changes its Status to Connect.
            AutomaticStop (Event 8) event are ignored in the Idle Status.
             The exact value of the ConnectRetryTimer is a local matter, but it SHOULD be sufficiently large to allow TCP initialization.
             Any other event (Events 9-12, 15-28) received in the Idle Status does not cause change in the Status of the local system.
      
           **/
        public void IdleStatus()
        {
            //Initially, the BGP peer FSM is in the Idle Status.Hereafter, the BGP peer FSM will be shortened to BGP FSM.In this Status, BGP FSM refuses all incoming BGP 
            //Connections for this peer.No resources are allocated to the peer.

            //Status[Initialize_BGP.connCount] = "Idle";
            //Status[Initialize_BGP.connCount](Status[Initialize_BGP.connCount]);
            if (AutoStartEvent == true)
            {
                Variables.ListenerConnectionStatus = "Idle";
                Variables.SpeakerConnectionStatus = "Idle";
                //Status[Initialize_BGP.connCount] = "Idle";
                //Console.WriteLine("Auto Start Event fired Idle Status[Initialize_BGP.connCount] do Stuff Here!!");
                //initalize all BGP resources for the peer Connection
                Initialize_BGP.StartListener();

                ConnectRetryCounter = 0;
                //sets ConnectRetryCounter to zero
                //SM.ConnectRetryTimer = new System.Timers.Timer(120000);
                ConnectionRetryTimer_Reset();
                //listens for a Connection that may be initiad by the remote  BGP peer and changes its Status to Connect
                Initialize_BGP.StartListening();
                //initiates a TCP Connection to the other BGP peer

                Initialize_BGP.StartSpeaker();
                Initialize_BGP.SpeakerConnection_Init();

                //Initialize_BGP.SendMessage();

                //RouterStatus(routerStatus);
                AutoStartEvent = false;
                //Console.WriteLine("Connection Retry Counter = " + SM.ConnectRetryCounter + "Connection Status[Initialize_BGP.connCount]: " + SM.Status[Initialize_BGP.connCount]);


            }
        }
        /**
        Connect Status:
            In this Status, BGP FSM is waiting for the TCP Connection to be completed.
            The start events (Events 1, 3-7) are ignored in the Connect Status.
            In response to the ConnectRetryTimer_Expire event (Event 9), the local system:
                    - drops the TCP Connection,
                    - restarts the ConnectRetryTimer,
                    - stops the DelayOpenTimer and resets the timer to zero,
                    - initiates a TCP Connection to the other BGP peer,
                    - continues to listen for a Connection that may be initiated by the remote BGP peer, and
                    - stays in the Connect Status.
            If the TCP Connection Succeed (Event 16 or Event 17), the local system checks the DelayOpen attribute prior to processing.  If the
            DelayOpen attribute is set to TRUE, the local system:
                    - stops the ConnectRetryTimer (if running) and sets the ConnectRetryTimer to zero,
                    - sets the DelayOpenTimer to the initial value, and
                    - stays in the Connect Status.
            A HoldTimer value of 4 minutes is suggested.
            If the TCP Connection fails (Event 18), the local system checks the DelayOpenTimer.  If the DelayOpenTimer is running, the local system:
                    - restarts the ConnectRetryTimer with the initial value,
                    - stops the DelayOpenTimer and resets its value to zero,
                    - continues to listen for a Connection that may be initiated by the remote BGP peer, and
                    - changes its Status to Active.
        **/
        public void ConnectStatus()
        {
            //In this Status, BGP FSM is waiting for the TCP Connection to be completed.
            //If the BGP FSM receives a TCPConnection_Valid event (Event 14),the TCP Connection is processed, and the Connection remains in theConnect Status.

            if (ConnectRetryExpire == true)
            {
                Console.WriteLine("Connection Retry Expired Connect Status do stuff here !!");

                //drops the TCP Connection
                //******ConnectRetryTimer is auto reseted in the implementation*******
                // Create a timer with a two 120000 interval.
                ConnectionRetryTimer_Reset();
                //initiates a TCP Connection to the other BGP peer

                //continues to listen for a Connection that may be initiated by the remote BGP peer, and
                //stays in the Connect Status.
                Variables.ListenerConnectionStatus = "Connect";

                ConnectRetryExpire = false;
            }
            //If the TCP Connection Succeed
            if (TCPConnectionSucceed == true)
            {
                //Console.WriteLine("TCP Connection Established Connect Status[Initialize_BGP.connCount] do stuff here !!");
                //stops the ConnectRetryTimer (if running) and sets the ConnectRetryTimer to zero,
                // Create a timer with a two 120000 interval.
                //SM.ConnectRetryTimer = new System.Timers.Timer(120000);
                ConnectionRetryTimer_Reset();
                //stays in the Connect Status.
                Variables.ListenerConnectionStatus = "Connect";
                TCPConnectionSucceed = false;

                // If the value of the autonomous system field is the same as the local Autonomous System number, set the Connection status to an
                //internal Connection; otherwise it will be "external".

                //Console.WriteLine("Speaker AS "+Variables.SpeakerAS[Variables.speakerIPAddress]);
                //Console.WriteLine("AS check Listener  " + Variables.ListenerAS[Variables.ListenerIPAddress]);

                ConnectionType.WaitOne();

                if (Variables.SpeakerAS[Variables.SpeakerIpAddress] == Variables.ListenerAS[Variables.ListenerIpAddress])
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
                // Create a timer with a two 120000 interval.
                //SM.ConnectRetryTimer = new System.Timers.Timer(120000);
                ConnectionRetryTimer_Reset();
                //continues to listen for a Connection that may be initiated by the remote BGP peer, and
                //Changes its Status to Active
                Variables.SpeakerConnectionStatus = "Active";
                TCPConnectionFail = false;
            }
            /**
            If BGP message header checking(Event 21) or OPEN message checking detects an Error(Event 22) (see Section 6.2), the local system:
                    - (optionally) If the SendNOTIFICATIONwithoutOPEN attribute is set to TRUE, then the local system first sends a NOTIFICATION message with the appropriate Error code, and then
                    - stops the ConnectRetryTimer(if running) and sets the ConnectRetryTimer to zero,
                    - releases all BGP resources,
                    - drops the TCP Connection,
                    - increments the ConnectRetryCounter by 1,
                    - (optionally) performs peer oscillation damping if the DampPeerOscillations attribute is set to TRUE, and
                    - changes its Status to Idle.
            If a NOTIFICATION message is received with a version Error(Event 24), the local system checks the DelayOpenTimer.If the DelayOpenTimer is running, the local system:
                    - stops the ConnectRetryTimer (if running) and sets the ConnectRetryTimer to zero,
                    - stops and resets the DelayOpenTimer(sets to zero),
                    - releases all BGP resources,
                    - drops the TCP Connection, and
                    - changes its Status to Idle.
            In response to any other events(Events 8, 10-11, 13, 19, 23, 25-28), the local system:
                    - if the ConnectRetryTimer is running, stops and resets the ConnectRetryTimer(sets to zero),
                    - if the DelayOpenTimer is running, stops and resets the DelayOpenTimer(sets to zero),
                    - releases all BGP resources,
                    - drops the TCP Connection,
                    - increments the ConnectRetryCounter by 1,
                    - performs peer oscillation damping if the DampPeerOscillations attribute is set to True, and
                    - changes its Status to Idle.
            **/
            if (BGPHeaderMessageError == true || mBGPOpenMessageError == true)
            {
                Console.WriteLine("BGP Header Message or Open Message has Error Connect Status do stuff here!!");
                //stops the ConnectRetryTimer(if running) and sets the ConnectRetryTimer to zero,
                //Create a timer with a two 120000 interval.
                //SM.ConnectRetryTimer = new System.Timers.Timer(120000);
                ConnectionRetryTimer_Reset();
                //releases all BGP resources,
                //drops the TCP Connection
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                Console.WriteLine("Connection Retry Counter = " + ConnectRetryCounter + "Connection Status: " + Variables.ListenerConnectionStatus);
                BGPHeaderMessageError = false;
                mBGPOpenMessageError = false;
            }
            if (mBGPNotifyMessageError == true)
            {
                Console.WriteLine("BGP Notify Message Error Connect Status do stuff here!!");
                //stops the ConnectRetryTimer(if running) and sets the ConnectRetryTimer to zero,
                // Create a timer with 120000 interval.
                //SM.ConnectRetryTimer = new System.Timers.Timer(120000);
                ConnectionRetryTimer_Reset();
                //releses all BGP resources
                //drops the TCP Connection
                Variables.ListenerConnectionStatus = "Idle";
                mBGPNotifyMessageError = false;
            }
            if (BGPAutoStop == true || HoldTimeExpire == true || KeepAliveExpire == true || BGPOpenMessage == true || BGPNotifyMessage == true || BGPKeepAliveMessage == true ||
                BGPUpdateMessage == true || mBGPUpdateMessageError == true)
            {
                //Console.WriteLine("BGP Events in Connect Status[Initialize_BGP.connCount] 8, 10-11, 19 and 25-28 do stuff here!!");
                // Create a timer with a two 120000 interval.
                //SM.ConnectRetryTimer = new System.Timers.Timer(120000);
                ConnectionRetryTimer_Reset();
                KeepAliveTimer_Reset();
                //relese all BGP resources
                //drops TCP Connection
                ConnectRetryCounter++;
                //Variables.ListenerConnectionStatus = "Idle";
                ConnectRetryExpire = false;
                BGPAutoStop = false;
                HoldTimeExpire = false;
                KeepAliveExpire = false;
                BGPOpenMessage = false;
                BGPNotifyMessage = false;
                BGPKeepAliveMessage = false;
                BGPUpdateMessage = false;
                mBGPUpdateMessageError = false;
            }


        }
        /**
        Active Status[Initialize_BGP.connCount]:
        In this Status, BGP FSM is trying to acquire a peer by listening for, and accepting, a TCP Connection.
            The start events (Events 1, 3-7) are ignored in the Active Status.
            In response to a ConnectRetryTimer_Expire event (Event 9), the local system:
                    - restarts the ConnectRetryTimer (with initial value),
                    - initiates a TCP Connection to the other BGP peer,
                    - continues to listen for a TCP Connection that may be initiated by a remote BGP peer, and
                    - changes its Status to Connect.
            A HoldTimer value of 4 minutes is also suggested for this Status transition.
            If the local system receives a TCPConnection_Valid event (Event 14), the local system processes the TCP Connection flags and stays in the Active Status.
            If the local system receives a TCP_CR_Invalid event (Event 15), the local system rejects the TCP Connection and stays in the Active Status[Initialize_BGP.connCount].
            In response to the success of a TCP Connection (Event 16 or Event 17), the local system checks the DelayOpen optional attribute prior to processing.
            If the DelayOpen attribute is set to TRUE, the local system:
                    - stops the ConnectRetryTimer and sets the ConnectRetryTimer to zero,
                    - sets the DelayOpenTimer to the initial value (DelayOpenTime), and
                    - stays in the Active Status.
            A HoldTimer value of 4 minutes is suggested as a "large value" for the HoldTimer.
            If the local system receives a TCPConnectionFails event (Event 18), the local system:
                    - restarts the ConnectRetryTimer (with the initial value),
                    - stops and clears the DelayOpenTimer (sets the value to zero),
                    - releases all BGP resource,
                    - increments the ConnectRetryCounter by 1,
                    - optionally performs peer oscillation damping if the DampPeerOscillations attribute is set to TRUE, and
                    - changes its Status to Idle.
           
        **/
        public void ActiveStatus()
        {
            //In this Status, BGP FSM is trying to acquire a peer by listening for, and accepting, a TCP Connection.
            if (ConnectRetryExpire == true)
            {
                Console.WriteLine("Connection Retry Expried in Active Status Do Stuff here!!");
                //SM.ConnectRetryTimer = new System.Timers.Timer(120000);
                ConnectionRetryTimer_Reset();
                //initiates a TCP Connection to the other BGP peer
                //continues to listen for a TCP Connection that may be initiated by a remote BGP peer, and
                Variables.ListenerConnectionStatus = "Connect";
                ConnectRetryExpire = false;
            }
            if (TCPConnectionSucceed == true)
            {
                //Console.WriteLine("TCP Connection Succeed in Active Status Stuff here!!");
                //stops the ConnectRetryTimer and sets the ConnectRetryTimer to zero,
                //**** must implement reset ConnectRetryTimer here in final Solution******
                //SM.ConnectRetryTimer = new System.Timers.Timer(120000);
                ConnectionRetryTimer_Reset();
                Variables.SpeakerConnectionStatus = "Active";
                TCPConnectionSucceed = false;
            }

            if (TCPConnectionFail == true)
            {
                Console.WriteLine("TCP Connection Fails in Active Status[Initialize_BGP.connCount] Stuff here!!");
                //**** must implement reset ConnectRetryTimer here in final Solution******
                //SM.ConnectRetryTimer = new System.Timers.Timer(120000);
                ConnectionRetryTimer_Reset();
                //relese all BGP resource
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                Console.WriteLine("Connection Retry Counter = " + ConnectRetryCounter + "Connection Status: " + Variables.ListenerConnectionStatus);
                TCPConnectionFail = false;
            }
            /**
            If an OPEN message is received and the DelayOpenTimer is running (Event 20), the local system:
                    - stops the ConnectRetryTimer (if running) and sets the ConnectRetryTimer to zero,
                    - stops and clears the DelayOpenTimer (sets to zero),
                    - completes the BGP initialization,
                    - sends an OPEN message,
                    - sends a KeepAlive message,
                    - if the HoldTimer value is non-zero,
                    - starts the KeepAliveTimer to initial value,
                    - resets the HoldTimer to the negotiated value,
            else if the HoldTimer is zero
                    - resets the KeepAliveTimer (set to zero),
                    - resets the HoldTimer to zero, and
                    - changes its Status to OpenConfirm.
            **/
            // BGPOpenMessageReceived is not implemented yet

            if (mBGPOpenMessageReceived == true)
            {
                //Console.WriteLine("BGP Open Message Received in Active Stuff here!!");
                //SM.ConnectRetryTimer = new System.Timers.Timer(120000);
                ConnectionRetryTimer_Reset();
                //completes the BGP initialization
                //sends an OPEN message
                Listener Listener = new Listener();
                Listener.SendingOpenMsg_Speaker();



                //sends a KeepAlive message


                Listener.SendingKeepAliveMsg_Speaker();

                if (HoldTimer != null)
                {
                    //SM.KeepAliveTimer = new System.Timers.Timer(80000);
                    KeepAliveTimer_Reset();
                    //SM.HoldTimer = new System.Timers.Timer(240000);
                    HoldTimer_Reset();

                }
                else if (HoldTimer == null)
                {
                    //SM.KeepAliveTimer = new System.Timers.Timer(80000);
                    KeepAliveTimer_Reset();
                    //SM.HoldTimer = new System.Timers.Timer(240000);
                    HoldTimer_Reset();

                }
                Variables.ListenerConnectionStatus = "OpenSent";
                Variables.SpeakerConnectionStatus = "OpenSent";
            }
            /**
            If the value of the autonomous system field is the same as the local Autonomous System number, set the Connection status to an internal Connection; otherwise it will be external.
            If BGP message header checking (Event 21) or OPEN message checking detects an Error (Event 22) (see Section 6.2), the local system:
                    - (optionally) sends a NOTIFICATION message with the appropriate Error code if the SendNOTIFICATIONwithoutOPEN attribute is set to TRUE,
                    - sets the ConnectRetryTimer to zero,
                    - releases all BGP resources,
                    - drops the TCP Connection,
                    - increments the ConnectRetryCounter by 1,
                    - (optionally) performs peer oscillation damping if the DampPeerOscillations attribute is set to TRUE, and
                    - changes its Status to Idle.
            If a NOTIFICATION message is received with a version Error (Event 24), the local system checks the DelayOpenTimer.  If the DelayOpenTimer is running, the local system:
                    - stops the ConnectRetryTimer (if running) and sets the ConnectRetryTimer to zero,
                    - stops and resets the DelayOpenTimer (sets to zero),
                    - releases all BGP resources,
                    - drops the TCP Connection, and
                    - changes its Status to Idle.
           In response to any other event (Events 8, 10-11, 13, 19, 23, 25-28), the local system:
                    - sets the ConnectRetryTimer to zero,
                    - releases all BGP resources,
                    - drops the TCP Connection,
                    - increments the ConnectRetryCounter by one,
                    - (optionally) performs peer oscillation damping if the DampPeerOscillations attribute is set to TRUE, and
                    - changes its Status to Idle.
            **/
            //***This part is left for further implementation After implemention Autonomous System*****
            //If the value of the autonomous system field is the same as the local Autonomous System number, set the Connection status to an internal Connection; otherwise it will be external.
            if (BGPHeaderMessageError == true || mBGPOpenMessageError == true)
            {
                Console.WriteLine("BGP heder message Error or open message Active Status Error Stuff here!!");
                //**** must implement reset ConnectRetryTimer here in final Solution******
                //SM.ConnectRetryTimer = new System.Timers.Timer(120000);
                ConnectionRetryTimer_Reset();
                //relese all BGP resources
                //drops the TCP Connection
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                Console.WriteLine("Connection Retry Counter = " + ConnectRetryCounter + "Connection Status: " + Variables.ListenerConnectionStatus);
                BGPHeaderMessageError = false;
                mBGPOpenMessageError = false;
            }
            if (BGPAutoStop == true || HoldTimeExpire == true || KeepAliveExpire == true || BGPOpenMessage == true || BGPNotifyMessage == true || BGPKeepAliveMessage == true ||
                BGPUpdateMessage == true || mBGPUpdateMessageError == true)
            {
                //Console.WriteLine("BGP Events in Active Status[Initialize_BGP.connCount] 8, 10-11, 19 and 25-28 do stuff here!!");
                // Create a timer with a two 120000 interval.
                //SM.ConnectRetryTimer = new System.Timers.Timer(120000);
                ConnectionRetryTimer_Reset();
                KeepAliveTimer_Reset();
                //relese all BGP resources
                //drops TCP Connection
                ConnectRetryCounter++;
                //Variables.ListenerConnectionStatus = "Idle";
                ConnectRetryExpire = false;
                BGPAutoStop = false;
                HoldTimeExpire = false;
                KeepAliveExpire = false;
                BGPOpenMessage = false;
                BGPNotifyMessage = false;
                BGPKeepAliveMessage = false;
                BGPUpdateMessage = false;
                mBGPUpdateMessageError = false;
            }
        }
        /**
         OpenSent:
            In this Status, BGP FSM waits for an OPEN message from its peer.
            The start events (Events 1, 3-7) are ignored in the OpenSent Status.
            If an AutomaticStop event (Event 8) is issued in the OpenSent Status, the local system:
                    - sends the NOTIFICATION with a Cease,
                    - sets the ConnectRetryTimer to zero,
                    - releases all the BGP resources,
                    - drops the TCP Connection,
                    - increments the ConnectRetryCounter by 1,
                    - (optionally) performs peer oscillation damping if the DampPeerOscillations attribute is set to TRUE, and
                    - changes its Status to Idle.
            If the HoldTimer_Expire (Event 10), the local system:
                    - sends a NOTIFICATION message with the Error code Hold Timer Expired,
                    - sets the ConnectRetryTimer to zero,
                    - releases all BGP resources,
                    - drops the TCP Connection,
                    - increments the ConnectRetryCounter,
                    - (optionally) performs peer oscillation damping if the DampPeerOscillations attribute is set to TRUE, and
                    - changes its Status to Idle.
            If a TCPConnectionFails event (Event 18) is received, the local system:
                    - closes the BGP Connection,
                    - restarts the ConnectRetryTimer,
                    - continues to listen for a Connection that may be initiated by the remote BGP peer, and
                    - changes its Status to Active.
            
        **/
        public void OpenSent()
        {
            //In this Status, BGP FSM waits for an OPEN message from its peer.

            if (BGPAutoStop == true)
            {
                //sends the Notification with Cease
                ConnectionRetryTimer_Reset();
                //relese all the BGP resources,
                //drops the TCP Connection
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                BGPAutoStop = false;
            }
            if (HoldTimeExpire == true)
            {
                //sends a NOTIFICATION message with the Error code Hold Timer Expired,
                ConnectionRetryTimer_Reset();
                //relese all BGP resources,
                //drops the TCP Connection,
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                HoldTimeExpire = false;
            }
            if (TCPConnectionFail == true)
            {
                //closes the BGP Connection,
                ConnectionRetryTimer_Reset();
                //continues to listen for a Connection that may be initiated by the remote BGP peer, and
                Variables.ListenerConnectionStatus = "Active";
                TCPConnectionFail = false;
            }
            /**
            When an OPEN message is received, all fields are checked for correctness.  If there are no Errors in the OPEN message (Event 19), the local system:
                    - resets the DelayOpenTimer to zero,
                    - sets the BGP ConnectRetryTimer to zero,
                    - sends a KeepAlive message, and
                    - sets a KeepAliveTimer (via the text below)
                    - sets the HoldTimer according to the negotiated value (see Section 4.2),
                    - changes its Status to OpenConfirm.
            If the negotiated hold time value is zero, then the HoldTimer and KeepAliveTimer are not started.  
            If the value of the Autonomous System field is the same as the local Autonomous System number,then the Connection is an "internal" 
    Connection; otherwise, it is an "external" Connection.  (This will impact UPDATE processing as described below.)
            If the BGP message header checking (Event 21) or OPEN message checking detects an Error (Event 22)(see Section 6.2), the local system:
                    - sends a NOTIFICATION message with the appropriate Error code,
                    - sets the ConnectRetryTimer to zero,
                    - releases all BGP resources,
                    - drops the TCP Connection,
                    - increments the ConnectRetryCounter by 1,
                    - (optionally) performs peer oscillation damping if the DampPeerOscillations attribute is TRUE, and
                    - changes its Status to Idle.
            
            **/
            //this part is not implemented yet
            if (mBGPOpenMessageReceived == true)
            {
                ConnectionRetryTimer_Reset();
                //sends a KeepAlive message, and

                KeepAliveTimer_Reset();
                HoldTimer_Reset();
                Variables.ListenerConnectionStatus = "OpenConform";
                Variables.SpeakerConnectionStatus = "OpenConform";
                mBGPOpenMessageReceived = false;
            }
            //******** After implementing AS************
            //If the value of the Autonomous System field is the same as the local Autonomous System number,then the Connection is an "internal" 
            //Connection; otherwise, it is an "external" Connection.
            if (mBGPHeaderError == true || mBGPOpenMessageError == true)
            {
                //sends a NOTIFICATION message with the appropriate Error code,
                ConnectionRetryTimer_Reset();
                //releses all BGP resources
                //drops the TCP Connection
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                mBGPHeaderError = false;
                mBGPOpenMessageError = false;
            }
            /**
            If a NOTIFICATION message is received with a version Error (Event 24), the local system:
                    - sets the ConnectRetryTimer to zero,
                    - releases all BGP resources,
                    - drops the TCP Connection, and
                    - changes its Status to Idle.
           In response to any other event (Events 9, 11-13, 20, 25-28), the local system:
                    - sends the NOTIFICATION with the Error Code Finite Status[Initialize_BGP.connCount] Machine Error,
                    - sets the ConnectRetryTimer to zero,
                    - releases all BGP resources,
                    - drops the TCP Connection,
                    - increments the ConnectRetryCounter by 1,
                    - (optionally) performs peer oscillation damping if the DampPeerOscillations attribute is set to TRUE, and
                    - changes its Status to Idle.
            **/
            if (mBGPNotifyMessageError == true)
            {
                ConnectionRetryTimer_Reset();
                //releases all BGP resources,
                //drops the TCP Connection, and
                Variables.ListenerConnectionStatus = "Idle";
                mBGPNotifyMessageError = false;
            }
            if (ConnectRetryExpire == true || KeepAliveExpire == true || mBGPNotifyMessage == true || mBGPKeepAliveMessage == true ||
               mBGPUpdateMessage == true || mBGPUpdateMessageError == true)
            {
                //Console.WriteLine("BGP Events in OpenSent (Events 9, 11-13, 20, 25-28), do stuff here!!");
                //sends the NOTIFICATION with the Error Code Finite Status[Initialize_BGP.connCount] Machine Error,

                ConnectionRetryTimer_Reset();
                KeepAliveTimer_Reset();
                //relese all BGP resources
                //drops TCP Connection
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                ConnectRetryExpire = false;
                KeepAliveExpire = false;
                mBGPNotifyMessage = false;
                mBGPKeepAliveMessage = false;
                mBGPUpdateMessage = false;
                mBGPUpdateMessageError = false;
            }
        }
        /**
        OpenConfirm Status[Initialize_BGP.connCount]:
            In this Status, BGP waits for a KeepAlive or NOTIFICATION message.
             In response to the AutomaticStop event initiated by the system (Event 8), the local system:
                    - sends the NOTIFICATION message with a Cease,
                    - sets the ConnectRetryTimer to zero,
                    - releases all BGP resources,
                    - drops the TCP Connection,
                    - increments the ConnectRetryCounter by 1,
                    - (optionally) performs peer oscillation damping if the DampPeerOscillations attribute is set to TRUE, and
                    - changes its Status to Idle.
            If the HoldTimer_Expire event (Event 10) occurs before a KeepAlive message is received, the local system:
                    - sends the NOTIFICATION message with the Error Code Hold Timer Expired,
                    - sets the ConnectRetryTimer to zero,
                    - releases all BGP resources,
                    - drops the TCP Connection,
                    - increments the ConnectRetryCounter by 1,
                    - (optionally) performs peer oscillation damping if the DampPeerOscillations attribute is set to TRUE, and
                    - changes its Status to Idle.
            If the local system receives a KeepAliveTimer_Expire event (Event 11), the local system:
                    - sends a KeepAlive message,
                    - restarts the KeepAliveTimer, and
                    - remains in the OpenConfirmed Status.
            In the event of a TCPConnection_Valid event (Event 14), or the success of a TCP Connection (Event 16 or Event 17) while in OpenConfirm, 
            the local system needs to track the second Connection.
            
        **/
        public void OpenConfirm()
        {
            //In this Status, BGP waits for a KeepAlive or NOTIFICATION message.
            if (BGPAutoStop == true)
            {
                // sends the Notification message with a Cease,
                ConnectionRetryTimer_Reset();
                //releases all the BGP resources,
                //drops the TCP Connection,
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                BGPAutoStop = false;
            }
            if (HoldTimeExpire == true)
            {
                //sends the NOTIFICATION message with the Error Code Hold Timer Expired,
                ConnectionRetryTimer_Reset();
                //releases all BGP resources,
                //drops the TCP Connection,
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                HoldTimeExpire = false;
            }
            if (KeepAliveExpire == true)
            {
                //sends a KeepAlive message,
                Listener Listener = new Listener();
                Listener.KeepAliveExpired();
                //Listener.SendingKeepAliveMessage_Speaker();
                KeepAliveTimer_Reset();
                ConnectionRetryTimer_Reset();
                Variables.ListenerConnectionStatus = "Idle";
                KeepAliveExpire = false;
                ConnectRetryExpire = false;
            }
            /**
            If the local system receives a TCPConnectionFails event (Event 18) from the underlying TCP or a NOTIFICATION message (Event 25), the local system:
                    - sets the ConnectRetryTimer to zero,
                    - releases all BGP resources,
                    - drops the TCP Connection,
                    - increments the ConnectRetryCounter by 1,
                    - (optionally) performs peer oscillation damping if the DampPeerOscillations attribute is set to TRUE, and
                    - changes its Status to Idle.
            If the local system receives a NOTIFICATION message with a version Error (NotifMessageVerError (Event 24)), the local system:
                    - sets the ConnectRetryTimer to zero,
                    - releases all BGP resources,
                    - drops the TCP Connection, and
                    - changes its Status to Idle.
            
            **/
            if (TCPConnectionFail == true)
            {
                ConnectionRetryTimer_Reset();
                //releases all BGP resources,
                //drops the TCP Connection
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                TCPConnectionFail = false;
            }
            if (mBGPNotifyMessageError == true)
            {
                ConnectionRetryTimer_Reset();
                //releases all BGP resources,
                //drops the TCP Connection
                Variables.ListenerConnectionStatus = "Idle";
                mBGPNotifyMessageError = false;
            }
            /**
           
            If an OPEN message is received, all fields are checked for correctness.  If the BGP message header checking (BGPHeaderError
        (Event 21)) or OPEN message checking detects an Error (see Section 6.2) (BGPOpenMessageError (Event 22)), the local system:
                    - sends a NOTIFICATION message with the appropriate Error code,
                    - sets the ConnectRetryTimer to zero,
                    - releases all BGP resources,
                    - drops the TCP Connection,
                    - increments the ConnectRetryCounter by 1,
                    - (optionally) performs peer oscillation damping if the DampPeerOscillations attribute is set to TRUE, and
                    - changes its Status to Idle.
            **/

            if (mBGPHeaderError == true || mBGPOpenMessageError == true)
            {
                //sends a NOTIFICATION message with the appropriate Error code,
                ConnectionRetryTimer_Reset();
                //releases all BGP resources,
                //drops the TCP Connection,
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                mBGPHeaderError = false;
                mBGPOpenMessageError = false;
            }
        }
        /**
    Established Status[Initialize_BGP.connCount]:
            In the Established Status, the BGP FSM can exchange UPDATE, NOTIFICATION, and KeepAlive messages with its peer.
            Any Start event (Events 1, 3-7) is ignored in the Established Status.
            In response to an AutomaticStop event (Event 8), the local system:
                    - sends a NOTIFICATION with a Cease,
                    - sets the ConnectRetryTimer to zero
                    - deletes all routes associated with this Connection,
                    - releases all BGP resources,
                    - drops the TCP Connection,
                    - increments the ConnectRetryCounter by 1,
                    - (optionally) performs peer oscillation damping if the DampPeerOscillations attribute is set to TRUE, and
                    - changes its Status to Idle.
            One reason for an AutomaticStop event is: A BGP receives an UPDATE messages with a number of prefixes for a given peer such that the
        total prefixes received exceeds the maximum number of prefixes configured.  The local system automatically disConnects the peer.
            If the HoldTimer_Expire event occurs (Event 10), the local system:
                    - sends a NOTIFICATION message with the Error Code Hold Timer Expired,
                    - sets the ConnectRetryTimer to zero,
                    - releases all BGP resources,
                    - drops the TCP Connection,
                    - increments the ConnectRetryCounter by 1,
                    - (optionally) performs peer oscillation damping if the DampPeerOscillations attribute is set to TRUE, and
                    - changes its Status to Idle.
            If the KeepAliveTimer_Expire event occurs (Event 11), the local system:
                    - sends a KeepAlive message, and
                    - restarts its KeepAliveTimer, unless the negotiated HoldTime value is zero.
        **/
        public void EstablishedStatus()
        {
            //In the Established Status, the BGP FSM can exchange UPDATE, NOTIFICATION, and KeepAlive messages with its peer.
            if (BGPAutoStop == true)
            {
                //sends a NOTIFICATION with a Cease,
                ConnectionRetryTimer_Reset();
                //deletes all routes associated with this Connection,
                //releases all BGP resources,
                //drops the TCP Connection,
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                BGPAutoStop = false;
            }
            if (HoldTimeExpire == true)
            {
                //sends a NOTIFICATION message with the Error Code Hold Timer Expired,
                ConnectionRetryTimer_Reset();
                //releases all BGP resources,
                //drops the TCP Connection,
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                HoldTimeExpire = false;
            }
            if (KeepAliveExpire == true)
            {
                //sends a KeepAlive message,
                //restarts its KeepAliveTimer, unless the negotiated HoldTime value is zero.
                KeepAliveTimer_Reset();
                ConnectionRetryTimer_Reset();
                KeepAliveExpire = false;
                ConnectRetryExpire = false;
            }
            /**
            If the local system receives a NOTIFICATION message (Event 24 or Event 25) or a TCPConnectionFails (Event 18) from the underlying TCP, the local system:
                    - sets the ConnectRetryTimer to zero,
                    - deletes all routes associated with this Connection,
                    - releases all the BGP resources,
                    - drops the TCP Connection,
                    - increments the ConnectRetryCounter by 1,
                    - changes its Status to Idle.
            If the local system receives a KeepAlive message (Event 26), the local system:
                    - restarts its HoldTimer, if the negotiated HoldTime value is non-zero, and
                    - remains in the Established Status.
            If the local system receives an UPDATE message (Event 27), the local system:
                    - processes the message,
                    - restarts its HoldTimer, if the negotiated HoldTime value is non-zero, and
                    - remains in the Established Status.
            **/
            if (mBGPNotifyMessage == true)
            {
                ConnectionRetryTimer_Reset();
                //deletes all routes associated with this Connection,
                //releases all the BGP resources,
                //drops the TCP Connection,
                ConnectRetryCounter++;
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
                //processes the message,
                HoldTimer_Reset();
                Variables.ListenerConnectionStatus = "Established";
                mBGPUpdateMessage = false;
            }
            /**
            If the local system receives an UPDATE message, and the UPDATE message Error handling procedure (see Section 6.3) detects an Error (Event 28), the local system:
                    - sends a NOTIFICATION message with an Update Error,
                    - sets the ConnectRetryTimer to zero,
                    - deletes all routes associated with this Connection,
                    - releases all BGP resources,
                    - drops the TCP Connection,
                    - increments the ConnectRetryCounter by 1,
                    - (optionally) performs peer oscillation damping if the DampPeerOscillations attribute is set to TRUE, and
                    - changes its Status to Idle.
            In response to any other event (Events 9, 12-13, 20-22), the local system:
                    - sends a NOTIFICATION message with the Error Code Finite Status[Initialize_BGP.connCount] Machine Error,
                    - deletes all routes associated with this Connection,
                    - sets the ConnectRetryTimer to zero,
                    - releases all BGP resources,
                    - drops the TCP Connection,
                    - increments the ConnectRetryCounter by 1,
                    - (optionally) performs peer oscillation damping if the DampPeerOscillations attribute is set to TRUE, and
                    - changes its Status to Idle.
            **/
            if (mBGPUpdateMessageError == true)
            {
                //sends a NOTIFICATION message with an Update Error,
                ConnectionRetryTimer_Reset();
                //deletes all routes associated with this Connection,
                //releases all BGP resources,
                //drops the TCP Connection,
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                mBGPUpdateMessageError = false;
            }
            if (ConnectRetryExpire == true || BGPHeaderMessageError == true || mBGPOpenMessageError == true)
            {
                //sends a NOTIFICATION message with the Error Code Finite Status[Initialize_BGP.connCount] Machine Error,
                //deletes all routes associated with this Connection,
                ConnectionRetryTimer_Reset();
                //releases all BGP resources
                //drops the TCP Connection
                ConnectRetryCounter++;
                Variables.ListenerConnectionStatus = "Idle";
                ConnectRetryExpire = false;
                BGPHeaderMessageError = false;
                mBGPOpenMessageError = false;
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
            //flag value for another method
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
            Console.WriteLine("TCP Connection ACKED");
            //throw new NotImplementedException();
        }
        public void TCPConnectionConfirmed(bool TCP_con)
        {
            TCPConnectionConfirmed_Event += new EventHandler(SM_TCPConnectionConfirmed_Event);
            TCPConnectionConfirmedValue = TCP_con;
            TCPConnectionConfirmed_Event += new EventHandler(SM_TCPConnectionConfirmed_Event);
        }

        private void SM_TCPConnectionConfirmed_Event(object sender, EventArgs e)
        {
            //Console.WriteLine("TCP Connection Confirmed");
            TCPConnectionSucceed = Variables.True;
            ConnectStatus();
            //flag value for another method
            TCPConnectionSucceed = Variables.True;
            ActiveStatus();
            //throw new NotImplementedException();
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
            //flag value for another method
            TCPConnectionFail = true;
            ActiveStatus();
            TCPConnectionFail = true;
            OpenSent();
            TCPConnectionFail = true;
            OpenConfirm();
            //throw new NotImplementedException();
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
            //flag value for another method
            BGPHeaderMessageError = true;
            ActiveStatus();
            BGPHeaderMessageError = true;
            OpenSent();
            BGPHeaderMessageError = true;
            OpenConfirm();
            BGPHeaderMessageError = true;
            EstablishedStatus();
            //throw new NotImplementedException();
        }
        public void BGPOpenMessageReceived(bool openReceived)
        {
            BGPOpenMessageReceived_Event += new EventHandler(SM_BGPOpenMessageReceived_Event);
            BGPOpenMessageReceive = openReceived;
            BGPOpenMessageReceived_Event += new EventHandler(SM_BGPOpenMessageReceived_Event);
        }

        private void SM_BGPOpenMessageReceived_Event(object sender, EventArgs e)
        {
            //Console.WriteLine("BGP Open Message Received by Listener");
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
            //Console.WriteLine("BGP Open Message Sent");
            BGPOpenMessage = true;
            ConnectStatus();
            //flag value for another method
            BGPOpenMessage = true;
            ActiveStatus();
            //throw new NotImplementedException();
        }
        public void BGPOpenMsgError(bool openMessageError)
        {
            BGPOpenMessageError_Event += new EventHandler(SM_BGPOpenMessageError_Event);
            BGPOpenMessageError = openMessageError;
            BGPOpenMessageError_Event -= new EventHandler(SM_BGPOpenMessageError_Event);
        }

        private void SM_BGPOpenMessageError_Event(object sender, EventArgs e)
        {
            Console.WriteLine("BGP Open message Error occured");
            mBGPOpenMessageError = true;
            ConnectStatus();
            //flag value for another method
            mBGPOpenMessageError = true;
            ActiveStatus();
            mBGPOpenMessageError = true;
            OpenSent();
            mBGPOpenMessageError = true;
            OpenConfirm();
            mBGPOpenMessageError = true;
            EstablishedStatus();
            //throw new NotImplementedException();
        }
        public void BGPNotifyMessageSent(bool notifySent)
        {
            BGPNotifyMessage_Event += new EventHandler(SM_BGPNotifyMessage_Event);
            BGPNotifyMessage = notifySent;
            BGPNotifyMessage_Event -= new EventHandler(SM_BGPNotifyMessage_Event);
        }

        private void SM_BGPNotifyMessage_Event(object sender, EventArgs e)
        {
            Console.WriteLine("BGP Notification message Send");
            mBGPNotifyMessage = true;
            ConnectStatus();
            //flag value for another method
            mBGPNotifyMessage = true;
            ActiveStatus();
            mBGPNotifyMessage = true;
            OpenSent();
            mBGPNotifyMessage = true;
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
            Console.WriteLine("BGP Notification Error message Send");
            mBGPNotifyMessageError = true;
            ConnectStatus();
            mBGPNotifyMessageError = true;
            OpenSent();
            mBGPNotifyMessageError = true;
            OpenConfirm();
            //throw new NotImplementedException();
        }
        public void BGPKeepAliveMessageSend(bool KeepAliveSent)
        {
            BGPKeepAliveMessage_Event += new EventHandler(SM_BGPKeepAliveMessageSend_Event);
            BGPKeepAliveMessage = KeepAliveSent;
            BGPKeepAliveMessage_Event -= new EventHandler(SM_BGPKeepAliveMessageSend_Event);
        }

        private void SM_BGPKeepAliveMessageSend_Event(object sender, EventArgs e)
        {
            //Console.WriteLine("BGP Keep Alive  message Send");
            mBGPKeepAliveMessage = true;
            ConnectStatus();
            //flag value for another method
            mBGPKeepAliveMessage = true;
            ActiveStatus();
            mBGPKeepAliveMessage = true;
            OpenSent();
            mBGPKeepAliveMessage = true;
            EstablishedStatus();
            //throw new NotImplementedException();
        }
        public void BGPUpdateMessageSent(bool updateSent)
        {
            BGPUpdateMessage_Event += new EventHandler(SM_BGPUpdateMessage_Event);
            BGPUpdateMessage = updateSent;
            BGPUpdateMessage_Event -= new EventHandler(SM_BGPUpdateMessage_Event);
        }

        private void SM_BGPUpdateMessage_Event(object sender, EventArgs e)
        {
            Console.WriteLine("BGP Update message Send");
            mBGPUpdateMessage = true;
            ConnectStatus();
            //flag value for another method
            mBGPUpdateMessage = true;
            ActiveStatus();
            mBGPUpdateMessage = true;
            OpenSent();
            mBGPUpdateMessage = true;
            EstablishedStatus();
            //throw new NotImplementedException();
        }
        public void BGPUpdateMsgError(bool updateError)
        {
            BGPUpdateMessageError_Event += new EventHandler(SM_BGPUpdateMessageError_Event);
            BGPUpdateMessageError = updateError;
            BGPUpdateMessageError_Event -= new EventHandler(SM_BGPUpdateMessageError_Event);
        }

        private void SM_BGPUpdateMessageError_Event(object sender, EventArgs e)
        {
            Console.WriteLine("BGP Update message Error Occured");
            mBGPUpdateMessageError = true;
            ConnectStatus();
            //flag value for another method
            mBGPUpdateMessageError = true;
            ActiveStatus();
            mBGPUpdateMessageError = true;
            OpenSent();
            mBGPUpdateMessageError = true;
            EstablishedStatus();
            //throw new NotImplementedException();
        }


        //********** Timer section code *************

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
            // Create a timer with a two 120000 interval.

            //ConnectRetryTimer = new System.Timers.Timer(120000);

            //this time is for only test purpose
            mConnectRetryTimer = new System.Timers.Timer(420000);


            ConnectionRetryFlag = true;
            // Hook up the Elapsed event for the timer. 
            mConnectRetryTimer.Elapsed += OnConnectionRetryExpire;

            // Have the timer fire repeated events (true is the default)
            mConnectRetryTimer.AutoReset = false;

            // Start the timer
            mConnectRetryTimer.Enabled = true;
            //SM.ConnectRetryTimer.Start();
        }
        public void HoldTimer_Reset()
        {
            if (holdTimerFlag == true)
            {
                HoldTimer.Close();

                holdTimerFlag = false;
            }

            //holdTimer = new System.Timers.Timer(240000);

            //this time is for only test purpose
            holdTimer = new System.Timers.Timer(540000);

            holdTimerFlag = true;
            holdTimer.Elapsed += OnHoldTimerExpire;

            // Have the timer fire repeated events (true is the default)
            holdTimer.AutoReset = false;

            // Start the timer
            holdTimer.Enabled = true;
        }
        public void KeepAliveTimer_Reset()
        {

            if (KeepAliveTimerFlag == true)
            {
                mKeepAliveTimer.Close();

                KeepAliveTimerFlag = false;
            }

            //KeepAliveTimer = new System.Timers.Timer(80000);

            //this time is for only test purpose
            mKeepAliveTimer = new System.Timers.Timer(400000);

            KeepAliveTimerFlag = true;
            mKeepAliveTimer.Elapsed += OnKeepAliveTimerExpire;

            // Have the timer fire repeated events (true is the default)
            mKeepAliveTimer.AutoReset = false;

            // Start the timer
            mKeepAliveTimer.Enabled = true;
        }
        private void OnKeepAliveTimerExpire(object sender, ElapsedEventArgs e)
        {
            KeepaliveTimer_Expire += new EventHandler(SM_StopConnectionKeepAliveEvent);
            KeepAliveTime = e.SignalTime;
            KeepaliveTimer = mKeepAliveTimer;
            KeepaliveTimer_Expire -= new EventHandler(SM_StopConnectionKeepAliveEvent);
            //throw new NotImplementedException();
        }

        private void OnHoldTimerExpire(object sender, ElapsedEventArgs e)
        {
            HoldTimer_Expire += new EventHandler(SM_StopConnectionHoldEvent);
            holdTime = e.SignalTime;
            HoldTimer = holdTimer;
            HoldTimer_Expire -= new EventHandler(SM_StopConnectionHoldEvent);
            //throw new NotImplementedException();
        }

        private void OnConnectionRetryExpire(object sender, ElapsedEventArgs e)
        {
            ConnectRetryTimer_Expire += new EventHandler(SM_StopConnectionRetryEvent);
            ConnectRetryTime = e.SignalTime;
            ConnectionRetryTimer = mConnectRetryTimer;
            ConnectRetryTimer_Expire -= new EventHandler(SM_StopConnectionRetryEvent);
            //throw new NotImplementedException();
        }

        private void SM_StopConnectionRetryEvent(object sender, EventArgs e)
        {

            Console.WriteLine("Connection Retry Event is Fired here");

            ConnectRetryExpire = true;
            ConnectStatus();
            //flag value for another method
            ConnectRetryExpire = true;
            ActiveStatus();
            ConnectRetryExpire = true;
            OpenSent();
            ConnectRetryExpire = true;
            EstablishedStatus();
            //throw new NotImplementedException();
        }

        private void SM_StopConnectionHoldEvent(object sender, EventArgs e)
        {

            Console.WriteLine("Connection Hold Timer is expired Event is Fired here");
            HoldTimeExpire = true;
            ConnectStatus();
            //flag value for another method
            HoldTimeExpire = true;
            ActiveStatus();
            HoldTimeExpire = true;
            OpenSent();
            HoldTimeExpire = true;
            OpenConfirm();
            HoldTimeExpire = true;
            EstablishedStatus();
            //throw new NotImplementedException();
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
            //throw new NotImplementedException();
        }

    }
}
