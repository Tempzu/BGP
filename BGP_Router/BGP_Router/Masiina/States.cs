using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
/** 
        Events for the BGP FSM
        8.1.1 Optional Events Linked to Optional Session Attributes (The Inputs to the BGP FSM are events.  Events can either be mandatory or optional)
        The linkage between FSM functionality, events, and the optional session attributes are described below.
            Group 1: Automatic Administrative Events (Start/Stop)
                Optional Session Attributes: AllowAutomaticStart, AllowAutomaticStop, DampPeerOscillations(all three bool value),IdleHoldTime, IdleHoldTimer(both value is time in sec)
            Group 2: Unconfigured Peers (not implementing right now complicated with security reasions)
                Optional Session Attributes: AcceptConnectionsUnconfiguredPeers (bool value)
            Group 3: TCP processing
                Optional Session Attributes: PassiveTCPEstablishment,TrackTCPState (both bool values)
            Group 4:  BGP Message Processing
                Optional Session Attributes: DelayOpen (bool), DelayOpenTime(time in sec), DelayOpenTimer(time in sec), SendNOTIFICATIONwithoutOPEN(bool), CollisionDetectEstablishedState(bool)
    **/
namespace BGP_Router.Masiina
{

    public class States
    {
        /**
        1) Description of Events for the State[init_BGP.connCount] machine(Section 8.1)
    Session attributes required(mandatory) for each Connection are:
        1) State[init_BGP.connCount]    2) ConnectRetryCounter  3) ConnectRetryTimer    4) ConnectRetryTime     5) HoldTimer    6) HoldTime     7) KeepaliveTimer   8) KeepaliveTime
    The state session attribute indicates the current state of the BGP FSM.
    Each timer has a "timer" and a "time" (the initial value).
    
    Two groups of the attributes which relate to timers are: (Which I am not going to implement right now.)
      group 1: DelayOpen, DelayOpenTime, DelayOpenTimer
      group 2: DampPeerOscillations, IdleHoldTime, IdleHoldTimer
    **/
        //public String State[init_BGP.connCount];
        public int ConnectRetryCounter;
        public Timer mConnectRetryTimer;
        public DateTime ConnectRetryTime;
        public Timer holdTimer;
        public DateTime holdTime;
        public Timer mKeepAliveTimer;
        public DateTime KeepAliveTime;

        /**
          
    8.1.2.  Administrative Events
    An administrative event is an event in which the operator interface and BGP Policy engine signal the BGP-finite state machine to start or
    stop the BGP state machine.
        Event 1: ManualStart
        Event 2: ManualStop
        Event 3: AutomaticStart (Among all of the above method I am just implementing this part)
            Definition: Local system automatically starts the BGP Connection.
            Status:     Optional, depending on local system
                Optional Attribute Status:     
                     1) The AllowAutomaticStart attribute SHOULD be set to TRUE if this event occurs.
                     2) If the PassiveTCPEstablishment optional session attribute is supported, it SHOULD be set to FALSE.
                     3) If the DampPeerOscillations is supported, it SHOULD be set to FALSE when this event occurs.
        Event 5: AutomaticStart_with_PassiveTCPEstablishment
         Definition: Local system automatically starts the BGP Connection with the PassiveTCPEstablishment enabled.  The PassiveTCPEstablishment optional
                     attribute indicates that the peer will listen prior to establishing a Connection.
         Status:     Optional, depending on local system
         Optional Attribute Status:     
                    1) The AllowAutomaticStart attribute SHOULD be set to TRUE.
                     2) The PassiveTCPEstablishment attribute SHOULD be set to TRUE.
                     3) If the DampPeerOscillations attribute is supported, the DampPeerOscillations SHOULD be set to FALSE.
         Event 6: AutomaticStart_with_DampPeerOscillations (most likely I will not implement this part also)
         Definition: Local system automatically starts the BGP peer Connection with peer oscillation damping enabled.The exact method of damping persistent peer
                     oscillations is determined by the implementation and is outside the scope of this document.
         Status:     Optional, depending on local system.
         Optional Attribute Status:     
                     1) The AllowAutomaticStart attribute SHOULD be set to TRUE.
                     2) The DampPeerOscillations attribute SHOULD be set to TRUE.
                     3) The PassiveTCPEstablishment attribute SHOULD be set to FALSE.
         Event 8: AutomaticStop
         Definition: Local system automatically stops the BGP Connection. An example of an automatic stop event is exceeding the number of prefixes for a given peer and 
                     the local system automatically disConnecting the peer.
         Status:     Optional, depending on local system
         Optional Attribute Status:    1) The AllowAutomaticStop attribute SHOULD be TRUE.
        **/
        public event EventHandler OnAutomaticStartEvent;
        //public event EventHandler AutomaticStart_with_PassiveTCPEstablishment;
        public event EventHandler OnAutomaticStopEvent;

        bool automaticStart;
        public bool AutomaticStart
        {
            get { return automaticStart; }
            set
            {
                automaticStart = value;
                OnAutomaticStartEvent(this, new EventArgs());
            }
        }
        bool automaticStop;
        public bool AutomaticStop
        {
            get { return automaticStop; }
            set
            {
                automaticStop = value;
                OnAutomaticStopEvent(this, new EventArgs());

            }

        }
        /**
        string state;
        public string State[init_BGP.connCount]
        {
            get { return state; }
            set
            {
                state = value;
            }
        }
            **/
        /**
         8.1.3.  Timer Events
      Event 9: ConnectRetryTimer_Expires
         Definition: An event generated when the ConnectRetryTimer expires.
         Status:     Mandatory
      Event 10: HoldTimer_Expires
         Definition: An event generated when the HoldTimer expires.
         Status:     Mandatory
      Event 11: KeepaliveTimer_Expires
         Definition: An event generated when the KeepaliveTimer expires.
         Status:     Mandatory
        **/
        public event EventHandler ConnectRetryTimer_Expire;
        public event EventHandler HoldTimer_Expire;
        public event EventHandler KeepaliveTimer_Expire;

        public Timer ConnectionRetryTimer
        {
            get { return ConnectionRetryTimer; }
            set
            {
                ConnectionRetryTimer = value;
                ConnectRetryTimer_Expire(this, new EventArgs());
            }
        }

        public Timer HoldTimer
        {
            get { return holdTimer; }
            set
            {
                holdTimer = value;
                HoldTimer_Expire(this, new EventArgs());

            }
        }
        public Timer KeepaliveTimer
        {
            get { return mKeepAliveTimer; }
            set
            {
                mKeepAliveTimer = value;
                KeepaliveTimer_Expire(this, new EventArgs());

            }

        }
        /**
         8.1.4.  TCP Connection-Based Events
      Event 14: TCPConnection_Valid (Optional)
      Event 15: TCP_CR_Invalid (Optional)
      Event 16: TCP_CR_Acked
         Definition: Event indicating the local system's request to establish a TCP Connection to the remote peer. The local system's TCP Connection sent a TCP SYN,
                     received a TCP SYN/ACK message, and sent a TCP ACK.
         Status:     Mandatory
      Event 17: TCPConnectionConfirmed
         Definition: Event indicating that the local system has received a confirmation that the TCP Connection has been established by the remote site.
                     The remote peer's TCP engine sent a TCP SYN.  The local peer's TCP engine sent a SYN, ACK message and now has received a final ACK.
         Status:     Mandatory
      Event 18: TCPConnectionFails
         Definition: Event indicating that the local system has received a TCP Connection failure notice. The remote BGP peer's TCP machine could have sent a FIN.  The local peer would respond with a FIN-ACK.
                     Another possibility is that the local peer indicated a timeout in the TCP Connection and downed the Connection.
         Status:     Mandatory
        **/
        public event EventHandler TCP_Acknowledged_Event;
        public event EventHandler TCPConnectionConfirmed_Event;
        public event EventHandler TCPConnectionFails_Event;

        public bool mTCPConnectionAcknowledged;
        public bool TCPConnectionAcknowledged
        {
            get { return mTCPConnectionAcknowledged; }
            set
            {
                mTCPConnectionAcknowledged = value;
                TCP_Acknowledged_Event(this, new EventArgs());
            }
        }

        public bool mTCPConnectionConfirmedValue;
        public bool TCPConnectionConfirmedValue
        {
            get { return mTCPConnectionConfirmedValue; }
            set
            {
                mTCPConnectionConfirmedValue = value;
                TCPConnectionConfirmed_Event(this, new EventArgs());
            }
        }

        public bool mTCPConnectionFails;
        public bool TCPConnectionFails
        {
            get { return mTCPConnectionFails; }
            set
            {
                mTCPConnectionFails = value;
                TCPConnectionFails_Event(this, new EventArgs());
            }
        }
        /**
        8.1.5.  BGP Message-Based Events
       Event 19: BGPOpen
         Definition: An event is generated when a valid OPEN message has been received.
         Status:     Mandatory
         Optional Attribute Status:     
                     1) The DelayOpen optional attribute SHOULD be set to FALSE.
                     2) The DelayOpenTimer SHOULD not be running.
       Event 21: BGPHeaderErr
         Definition: An event is generated when a received BGP message header is not valid.
         Status:     Mandatory
      Event 22: BGPOpenMessageErr
         Definition: An event is generated when an OPEN message has been received with errors.
         Status:     Mandatory 
      Event 24: NotifMessageVerErr
         Definition: An event is generated when a NOTIFICATION message with "version error" is received.
         Status:     Mandatory
      Event 25: NotifMessage
         Definition: An event is generated when a NOTIFICATION message is received and the error code is anything but "version error".
         Status:     Mandatory
      Event 26: KeepAliveMessage
         Definition: An event is generated when a KEEPALIVE message is received.
         Status:     Mandatory 
      Event 27: UpdateMessage
         Definition: An event is generated when a valid UPDATE message is received.
         Status:     Mandatory
      Event 28: UpdateMessageErr
         Definition: An event is generated when an invalid UPDATE message is received.
         Status:     Mandatory
        **/
        public event EventHandler BGPOpenMessage_Event;
        public event EventHandler BGPOpenMessageReceived_Event;
        public event EventHandler BGPHeaderError_Event;
        public event EventHandler BGPOpenMessageError_Event;
        public event EventHandler BGPNotifyMessageError_Event;
        public event EventHandler BGPNotifyMessage_Event;
        public event EventHandler BGPKeepAliveMessage_Event;
        public event EventHandler BGPUpdateMessage_Event;
        public event EventHandler BGPUpdateMessageError_Event;

        public bool mBGPOpenMessage;
        public bool BGPOpenMessage
        {
            get { return mBGPOpenMessage; }
            set
            {
                mBGPOpenMessage = value;
                BGPOpenMessage_Event(this, new EventArgs());
            }
        }
        public bool mBGPOpenMessageReceived;
        public bool BGPOpenMessageReceive
        {
            get { return mBGPOpenMessageReceived; }
            set
            {
                mBGPOpenMessageReceived = value;
                BGPOpenMessageReceived_Event(this, new EventArgs());
            }
        }
        public bool mBGPHeaderError;
        public bool BGPHeaderError
        {
            get { return mBGPHeaderError; }
            set
            {
                mBGPHeaderError = value;
                BGPHeaderError_Event(this, new EventArgs());
            }
        }
        public bool mBGPOpenMessageError;
        public bool BGPOpenMessageError
        {
            get { return mBGPOpenMessageError; }
            set
            {
                mBGPOpenMessageError = value;
                BGPOpenMessageError_Event(this, new EventArgs());
            }
        }

        public bool mBGPNotifyMessageError;
        public bool BGPNotifyMessageError
        {
            get { return mBGPNotifyMessageError; }
            set
            {
                mBGPNotifyMessageError = value;
                BGPNotifyMessageError_Event(this, new EventArgs());
            }
        }
        public bool mBGPNotifyMessage;
        public bool BGPNotifyMessage
        {
            get { return mBGPNotifyMessage; }
            set
            {
                mBGPNotifyMessage = value;
                BGPNotifyMessage_Event(this, new EventArgs());
            }
        }
        public bool mBGPKeepAliveMessage;
        public bool BGPKeepAliveMessage
        {
            get { return mBGPKeepAliveMessage; }
            set
            {
                mBGPKeepAliveMessage = value;
                BGPKeepAliveMessage_Event(this, new EventArgs());
            }
        }
        public bool mBGPUpdateMessage;
        public bool BGPUpdateMessage
        {
            get { return mBGPUpdateMessage; }
            set
            {
                mBGPUpdateMessage = value;
                BGPUpdateMessage_Event(this, new EventArgs());
            }
        }
        public bool mBGPUpdateMessageError;
        public bool BGPUpdateMessageError
        {
            get { return mBGPUpdateMessageError; }
            set
            {
                mBGPUpdateMessageError = value;
                BGPUpdateMessageError_Event(this, new EventArgs());
            }
        }
    }
}
