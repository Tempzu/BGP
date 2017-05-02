using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGP_Router.Messages
{
    /* A KEEPALIVE message consists of only the message header and has a
   length of 19 bytes. The send interval is defaultly 60s. 

    */

    class KeepAlive : Structure
    
        // Basic message structure for KEEPALIVE message
        // Message fields represented by the numbers
    {
        private ushort mType;

        public KeepAlive() : base ((ushort)(38 + 2), 19)
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
