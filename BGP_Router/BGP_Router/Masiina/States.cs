using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace BGP_Router.Masiina
{

    public class States
    {
        // Supportive class for FSM
        // Getters and Setters for events used in FSM

        public int ConnectRetryCounter;
        public Timer mConnectRetryTimer;
        public DateTime ConnectRetryTime;
        public Timer holdTimer;
        public DateTime holdTime;
        public Timer mKeepAliveTimer;
        public DateTime KeepAliveTime;

        public event EventHandler OnAutomaticStartEvent;
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
