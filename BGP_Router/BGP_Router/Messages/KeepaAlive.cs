using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGP_Router.Messages
{
    class KeepaAlive : Structure

    {
        private short mType;
        public KeepaAlive() : base ((short)(40), 19)
        {
            Type = 4;
        }
        public KeepaAlive(byte[] packet) : base(packet)
        {

        }
        public short Type
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
