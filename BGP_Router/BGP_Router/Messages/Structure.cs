using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGP_Router.Messages
{
    /* Each message has a different size header. The message header has a 16-octet marker field.
     * Consists of marker (16-octet) + length (2-octet) + type (1-octet). Value of length must be at least 19.
     * The value of type is set correspondingly to each message type.
     * */
    public abstract class Structure
    {
        private byte[] mBuffer;
        public ulong marker;
        public Structure(ulong marker, int length)
        {
            // Message header marker is 16-octets with 32 slots, value set to be all ones!
            // Message length is 3 octets with 6 slots

            mBuffer = new byte[marker];
            for (int i = 0; i < 16; i++)
            {
                writeMarker(marker, i * 2);
            }
            writeLength(length, 32);
        }
        public void writeMarker(ulong value, int offset)
        {
            byte[] temp = new byte[32];

            temp = BitConverter.GetBytes(1);
            /* Copy the specified items
             * public static void BlockCopy(
	                               Array src,
	                               int srcOffset,
	                               Array dst,
	                               int dstOffset,
	                               int count
                                   )
             * */
            
            Buffer.BlockCopy(temp, 0, mBuffer, offset, 2);
        }
        public void writeLength(int value, int offset)
        {
            byte[] temp = new byte[6];
            temp = BitConverter.GetBytes(value);
            Buffer.BlockCopy(temp, 0, mBuffer, offset, 2);
        }
        // assign message value
        public void writeType(ushort value, int offset)
        {
            byte[] temp = new byte[2];
            temp = BitConverter.GetBytes(value);
            Buffer.BlockCopy(temp, 0, mBuffer, offset, 2);
        }
        // End of message header format

        /* Here begins the OpenMessage implementation
        *  Consists of version (1-octet) + my AS (2-octet) + hold time (2-octet) + BGP identifier (4-octet)
        *  + optional parameters length (1-octet) + optional parameters (8-octet) 
        *  Minimum length of OPEN message is 29 octets (includes header).
        */
        public void writeVersion(ushort value, int offset)
        {
            byte[] temp = new byte[2];
            temp = BitConverter.GetBytes(value);
            Buffer.BlockCopy(temp, 0, mBuffer, offset, 2);
        }
        public void writeAS(ushort value, int offset)
        {
            byte[] temp = new byte[2];
            temp = BitConverter.GetBytes(value);
            Buffer.BlockCopy(temp, 0, mBuffer, offset, 2);
        }
        public void writeHoldTime(ushort value, int offset)
        {
            byte[] temp = new byte[2];
            temp = BitConverter.GetBytes(value);
            Buffer.BlockCopy(temp, 0, mBuffer, offset, 2);
        }
        public void writeBGPID(string value, int offset)
        {
            byte[] temp = new byte[2];
            temp = Encoding.UTF8.GetBytes(value);
            Buffer.BlockCopy(temp, 0, mBuffer, offset, value.Length);
        }
        public void writeOptionalLength(ushort value, int offset)
        {
            byte[] temp = new byte[2];
            temp = BitConverter.GetBytes(value);
            Buffer.BlockCopy(temp, 0, mBuffer, offset, 2);
        }
        public void writeString(string value, int offset)
        {
            byte[] temp = new byte[value.Length];
            temp = Encoding.UTF8.GetBytes(value);
            Buffer.BlockCopy(temp, 0, mBuffer, offset, value.Length);
        }
        // End of OPEN message

        // UpdateMessage implementation here
        /* Consists of withdrawn routes length (2-octets) + withdrawn routes(variable/IPLength+IPPrefix) + total path attribute length (2-octets) 
         * + path attributes (variable/ attribute flags + attribute type code) + network layer reachability information (variable/ length + prefix)
         * 
         * */
        public void writeWithdrawRoutesLn(UInt16 value, int offset)
        {
            byte[] temp = new byte[4];
            temp = BitConverter.GetBytes(value);
            Buffer.BlockCopy(temp, 0, mBuffer, offset, 2);
        }
        public void writeWithdrawalRoutes(string value, int offset)
        {
            byte[] temp = new byte[value.Length];
            temp = Encoding.UTF8.GetBytes(value);
            Buffer.BlockCopy(temp, 0, mBuffer, offset, value.Length);
        }
        public void writeIpPrefixLength(ushort value, int offset)
        {
            byte[] temp = new byte[2];
            temp = BitConverter.GetBytes(value);
            Buffer.BlockCopy(temp, 0, mBuffer, offset, 2); 
        }
        public void writeIpPrefix(string value, int offset)
        {
            byte[] temp = new byte[value.Length];
            temp = Encoding.UTF8.GetBytes(value);
            Buffer.BlockCopy(temp, 0, mBuffer, offset, value.Length);
        }
        public void writeTotalPath(ushort value, int offset)
        {
            byte[] temp = new byte[4];
            temp = BitConverter.GetBytes(value);
            Buffer.BlockCopy(temp, 0, mBuffer, offset, 2);
        }
        public void writeAttributeLength(UInt32 value, int offset)
        {
            byte[] temp = new byte[4];
            temp = BitConverter.GetBytes(value);
            Buffer.BlockCopy(temp, 0, mBuffer, offset, 2);
        }
        public void writeAttribute(string value, int offset)
        {
            byte[] temp = new byte[value.Length];
            temp = Encoding.UTF8.GetBytes(value);
            Buffer.BlockCopy(temp, 0, mBuffer, offset, value.Length);
        }
        public void writeAttributeFlags(UInt32 value, int offset)
        {
            byte[] temp = new byte[2];
            temp = BitConverter.GetBytes(value);
            Buffer.BlockCopy(temp, 0, mBuffer, offset, 2);
        }
        public void writeCodeType(ushort value, int offset)
        {
            byte[] temp = new byte[2];
            temp = BitConverter.GetBytes(value);
            Buffer.BlockCopy(temp, 0, mBuffer, offset, 2);
        }
        public void writePathSegmentType(ushort value, int offset)
        {
            byte[] temp = new byte[2];
            temp = BitConverter.GetBytes(value);
            Buffer.BlockCopy(temp, 0, mBuffer, offset, 2);
        }
        public void writePathSegmentLength(ushort value, int offset)
        {
            byte[] temp = new byte[2];
            temp = BitConverter.GetBytes(value);
            Buffer.BlockCopy(temp, 0, mBuffer, offset, 2);
        }
        public void writePathSegmentValue(string value, int offset)
        {
            byte[] temp = new byte[value.Length];
            temp = Encoding.UTF8.GetBytes(value);
            Buffer.BlockCopy(temp, 0, mBuffer, offset, value.Length);
        }
        public void writeNlrLength(ushort value, int offset)
        {
            byte[] temp = new byte[2];
            temp = BitConverter.GetBytes(value);
            Buffer.BlockCopy(temp, 0, mBuffer, offset, 2);
        }
        public void writeNlrPrefix(string value, int offset)
        {
            byte[] temp = new byte[value.Length];
            temp = Encoding.UTF8.GetBytes(value);
            Buffer.BlockCopy(temp, 0, mBuffer, offset, value.Length);
        }
        // End of UPDATE message

        // Notification message here
        /* Consists of error code (1-octet) + error subcode (1-octet) + Data (variable)
         * */
        public void writeErrorCode(ushort value, int offset)
        {
            byte[] temp = new byte[2];
            temp = BitConverter.GetBytes(value);
            Buffer.BlockCopy(temp, 0, mBuffer, offset, 2);
        }
        public void writeErrorSubCode(ushort value, int offset)
        {
            byte[] temp = new byte[2];
            temp = BitConverter.GetBytes(value);
            Buffer.BlockCopy(temp, 0, mBuffer, offset, 2);
        }
        public void writeData(string value, int offset)
        {
            byte[] temp = new byte[value.Length];
            temp = Encoding.UTF8.GetBytes(value);
            Buffer.BlockCopy(temp, 0, mBuffer, offset, value.Length);
        }
        //End of NOTIFICATION message

        public Structure(byte [] packet)
        {
            mBuffer = packet;
        }
        // Message is stored in BGPmessage buffer
        public byte[] BGPmessage { get { return mBuffer; } }
    }
}
a
