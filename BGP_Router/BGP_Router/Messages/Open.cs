using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGP_Router.Messages
{
    class Open : Structure
    {
        private ushort mType;
        private ushort mVersion;
        private ushort mAS;
        private ushort mHoldTime;
        private string mBgpIdentifier;
        private ushort mOptimalParLength;

        public Open(ushort version, ushort AS, ushort holdTime, string bgpIdentifier, ushort optimalParLength)
            : base ((ushort)(38 + 2 + 2 + 4 + bgpIdentifier.Length + 1 + 2), 40)
        {
            Type = 1;
            Version = version;
            AS1 = AS;
            HoldTime = holdTime;
            BgpIdentifier = bgpIdentifier;
            OptimalParLength = optimalParLength;
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
        public ushort OptimalParLength
        {
            get
            {
                return mOptimalParLength;
            }
            set
            {
                mOptimalParLength = value;
                writeOptimalLength(value, 55);
            }
        }
    }
}
