using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGP_Router.Messages
{
    public abstract class Structure
    {
        private byte[] mBuffer;
        public long marker;
        public Structure(long marker, int length)
        {
            mBuffer = new byte[marker];
            for (int i = 0; i < 16; i++)
            {
                writeMarker(marker, i * 2);
            }
            writeLength(length, 32);
        }
        public void writeMarker(long value, int offset)
        {
            byte[] tempBuffer = new byte[32];
            
        }
    }
}
