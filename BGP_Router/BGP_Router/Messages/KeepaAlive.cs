using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGP_Router.Messages
{
    class KeepaAlive : Structure

    {
        private ushort mType;
        public KeepaAlive() : base ((ushort)(40), 19)
        {
            Type = 4;
        }
        public KeepaAlive(byte[] packet) : base(packet)
        {

        }
        public ushort Type
        {
            get { return mType; }
            set
            {
                mType = value;
                writeType(value, 38);
            }
        }
    }
}
