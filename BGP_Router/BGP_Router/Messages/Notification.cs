using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGP_Router.Messages
{
    class Notification : Structure
    {
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
