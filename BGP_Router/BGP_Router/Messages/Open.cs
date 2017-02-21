using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGP_Router.Messages
{
    class Open : Structure
    {
        private short mType;
        private short mVersion;
        private short mAS;
        private short mHoldTime;
        private string mBgpIdentifier;
        private short mOptimalParLength;

        public Open(short version, short AS, short holdTime, string bgpIdentifier, short optimalParLength)
            : base ((short)(38 + 2 + 2 + 4 + bgpIdentifier.Length + 1 + 2), 40)
        {
            Type = 1;
            Version = version;
            AS1 = AS;
            HoldTime = holdTime;
            BgpIdentifier = bgpIdentifier;
            OptimalParLength = optimalParLength;
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
        public short Version
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
        public short AS1
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
        public short HoldTime
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
        public short OptimalParLength
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
