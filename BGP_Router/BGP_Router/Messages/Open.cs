using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGP_Router.Messages
{
    class Open : Structure
    {
        /* Consists of version(1-octet) + my AS(2-octet) + hold time(2-octet) + BGP identifier(4-octet)
        *  + optional parameters length(1-octet) + optional parameters(8-octet)
        *  Minimum length of OPEN message is 29 octets(includes header).
        */
        //Open message type
        private ushort mType;
        //Open message version/BGP version
        private ushort mVersion;
        private ushort mAS;
        private ushort mHoldTime;
        private string mBgpIdentifier;
        private ushort mOptionalParLength;

        public Open(ushort version, ushort AS, ushort holdTime, string bgpIdentifier, ushort optionalParLength)
            : base ((ushort)(38 + 2 + 2 + 4 + bgpIdentifier.Length + 1 + 2), 40)
        {
            Type = 1;
            Version = version;
            AS1 = AS;
            HoldTime = holdTime;
            BgpIdentifier = bgpIdentifier;
            OptionalParLength = optionalParLength;
        }

        // Setting the field to corresponding places
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
        public ushort Version
        {
            get
            {
                return mVersion;
            }
            set
            {
                mVersion = value;
                writeVersion(value, 42);
            }
        }
        public ushort AS1
        {
            get
            {
                return mAS;
            }
            set
            {
                mAS = value;
                writeAS(value, 42);
            }
        }
        public ushort HoldTime
        {
            get
            {
                return mHoldTime;
            }
            set
            {
                mHoldTime = value;
                writeHoldTime(value, 44);
            }
        }
        public string BgpIdentifier
        {
            get
            {
                return mBgpIdentifier;
            }
            set
            {
                mBgpIdentifier = value;
                writeBGPID(value, 46);
            }
        }
        public ushort OptionalParLength
        {
            get
            {
                return mOptionalParLength;
            }
            set
            {
                mOptionalParLength = value;
                writeOptionalLength(value, 55);
            }
        }
    }
}
