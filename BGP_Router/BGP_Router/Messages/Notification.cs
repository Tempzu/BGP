using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGP_Router.Messages
{
    class Notification : Structure
    {
        private short mErrorCode;
        private short mErrorSubCode;
        private string mData;
        private short mType;

        public Notification(short errorCode, short errorSubcode, string data) 
            : base((short)(38 + 2 + 2 + 2 + data.Length), 21)
        {
            Type = 3;
            ErrorCode = errorCode;
            ErrorSubCode = errorSubcode;
            Data = data;
        }
        public short Type
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
        public short ErrorCode
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
        public short ErrorSubCode
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
