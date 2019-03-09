using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleHttpServer
{
    public class WebSocketFrame
    {
        public int FIN;
        public int OpCode;
        public bool Masked;
        public byte[] MaskingKey;
        public Int64 PayloadLength;
        public byte[] Payload;
        public WebSocketFrame(string message)
        {
            OpCode = 1; //Text
            FIN = 1;
            Masked = false;
            PayloadLength = message.Length;
            Payload = Encoding.UTF8.GetBytes(message);
        }

        public WebSocketFrame(byte[] bytes)
        {
            this.FIN = (bytes[0] & 0x80) >> 7;
            this.OpCode = bytes[0] & 0x0F;
            this.Masked = (bytes[1] & 0x80) == 0x80;
            this.PayloadLength = bytes[1] & 0x7F;
            int nextbyte = 2;
            if (this.PayloadLength == 126)
            {
                this.PayloadLength = 0;
                this.PayloadLength = ((Int64)bytes[nextbyte]) & ((Int64)bytes[nextbyte + 1]) << 8;
                nextbyte += 2;
            }
            else if (this.PayloadLength == 127)
            {
                this.PayloadLength = 0;
                for (int i = 0; i < 8; i++)
                {
                    this.PayloadLength = this.PayloadLength & ((Int64)bytes[nextbyte++]) << (8 * i);
                }
            }
            if (this.Masked)
            {
                this.MaskingKey = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    this.MaskingKey[i] = bytes[nextbyte++];
                }
            }
            Payload = new byte[this.PayloadLength];
            //Probably will cause issues using PayloadLength rather than just the remaining byte count
            Array.Copy(bytes, nextbyte, Payload, 0, this.PayloadLength);
            if (this.Masked)
            {
                //Decode the message;
                for (int m = 0; m < this.PayloadLength; m++)
                {
                    Payload[m] = (byte)(Payload[m] ^ MaskingKey[m % 4]);
                }
            }
        }

        public byte[] GetBytes()
        {

        }


    }
}
