using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGP_Router.Messages
{
    class Notification : Structure
    {
        /* Consists of error code (1-octet) + error subcode (1-octet) + Data (variable)
         * The minimun length of notification message is 21 octets. 
         */

        private ushort mErrorCode;
        private ushort mErrorSubCode;
        private string mData;
        private ushort mType;

        public Notification(ushort errorCode, ushort errorSubcode, string data) 
            : base((ushort)(38 + 2 + 2 + 2 + data.Length), 21)
        {
            Type = 3;
            ErrorCode = errorCode;
            ErrorSubCode = errorSubcode;
            Data = data;
        }
        // The information fields are set to the corresponding places in the message fields.
        public ushort Type
        {
            get
            {
                return mType; 
            }
            set
            {
                mType = value;
                writeType(value, 38);
            }
        }
        public ushort ErrorCode
        {
            get
            {
                return mErrorCode;
            }
            set
            {
                mErrorCode = value;
                writeErrorCode(value, 40);
            }
        }
        public ushort ErrorSubCode
        {
            get
            {
                return mErrorSubCode;
            }
            set
            {
                mErrorSubCode = value;
                writeErrorSubCode(value, 42);
            }
        }
        public string Data
        {
            get
            {
                return mData;
            }
            set
            {
                mData = value;
                writeData(value, 44);
            }
        }
    }
}
