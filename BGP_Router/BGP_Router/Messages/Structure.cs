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
        public ulong marker;
        public Structure(ulong marker, int length)
        {
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
        // Here begins the OpenMessage implementation
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
        public void writeOptimalLength(ushort value, int offset)
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
        // UpdateMessage implementation
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
        // Notification message here
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
        public Structure(byte [] packet)
        {
            mBuffer = packet;
        }
        // Message is stored in BGPmessage buffer
        public byte[] BGPmessage { get { return mBuffer; } }
    }
}
