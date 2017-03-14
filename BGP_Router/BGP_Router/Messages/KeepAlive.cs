using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGP_Router.Messages
{
    class KeepAlive : Structure

    {
        private ushort mType;
        public KeepAlive() : base((ushort)(40), 19)
        {
            Type = 4;
        }
        public KeepAlive(byte[] packet) : base(packet)
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
