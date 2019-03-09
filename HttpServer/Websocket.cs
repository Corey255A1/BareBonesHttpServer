// Corey Wunderlich - 2019
// www.wundervisionenvisionthefuture.com
//
// A class to decode and encode WebSocket Frames.

 // From the RFC6455
 //     0                   1                   2                   3
 //     0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
 //    +-+-+-+-+-------+-+-------------+-------------------------------+
 //    |F|R|R|R| opcode|M| Payload len |    Extended payload length    |
 //    |I|S|S|S|  (4)  |A|     (7)     |             (16/64)           |
 //    |N|V|V|V|       |S|             |   (if payload len==126/127)   |
 //    | |1|2|3|       |K|             |                               |
 //    +-+-+-+-+-------+-+-------------+ - - - - - - - - - - - - - - - +
 //    |     Extended payload length continued, if payload len == 127  |
 //    + - - - - - - - - - - - - - - - +-------------------------------+
 //    |                               |Masking-key, if MASK set to 1  |
 //    +-------------------------------+-------------------------------+
 //    | Masking-key(continued)       |          Payload Data         |
 //    +-------------------------------- - - - - - - - - - - - - - - - +
 //    :                     Payload Data continued...                :
 //    + - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - +
 //    |                     Payload Data continued...                |
 //    +---------------------------------------------------------------+

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
            this.OpCode = 1; //Text
            this.FIN = 1;
            this.Masked = false;
            this.PayloadLength = message.Length;
            this.Payload = Encoding.UTF8.GetBytes(message);
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
            this.Payload = new byte[this.PayloadLength];
            //Probably will cause issues using PayloadLength rather than just the remaining byte count
            Array.Copy(bytes, nextbyte, this.Payload, 0, this.PayloadLength);
            if (this.Masked)
            {
                //Decode the message;
                for (int m = 0; m < this.PayloadLength; m++)
                {
                    this.Payload[m] = (byte)(this.Payload[m] ^ this.MaskingKey[m % 4]);
                }
            }
        }

        public byte[] GetBytes()
        {
            Int64 totalpacketlength = this.PayloadLength;            
            totalpacketlength += 2; //FIN/OpCode Byte + PayloadLen Byte

            byte payloadlen = 0;
            if (this.PayloadLength<=125)
            {
                payloadlen = (byte)this.PayloadLength;
            }
            else
            {
                if(this.PayloadLength>(UInt16.MaxValue))
                {
                    payloadlen = 127;
                    totalpacketlength += 8; //8 extra Bytes for a 64 Bit PayloadLength Field
                }
                else
                {
                    payloadlen = 126;
                    totalpacketlength += 2; //2 extra Bytes for a 16 Bit PayloadLength Field
                }
            }
            if(Masked)
            {
                totalpacketlength += 4; //4 extra Bytes for the Masking Key Field;
            }

            byte[] packet = new byte[totalpacketlength];

            //Time for the packing process ... and for something completely different
            packet[0] = (byte)(((this.FIN & 0x01) << 7) | (this.OpCode & 0x0F)); //[Fin 0][r1 1][r2 2][r3 3][opcode 4-7]
            packet[1] = (byte)(this.Masked ? 0x80 : 0x00 | (payloadlen & 0x7F)); //[mask 0][payloadlen 1-7]
            int nextbyte = 2;
            if (payloadlen == 126)
            {
                Array.Copy(BitConverter.GetBytes((Int16)this.PayloadLength), 0, packet, nextbyte, 2);
                nextbyte += 2;
            }
            else if (payloadlen == 127)
            {
                Array.Copy(BitConverter.GetBytes(this.PayloadLength), 0, packet, nextbyte, 8);
                nextbyte += 8;
            }
            if (this.Masked)
            {
                Array.Copy(this.MaskingKey, 0, packet, nextbyte, 4);
                nextbyte += 4;
            }

            //Do some shenanigans with the mask ... From reading though, server side doesn't typically mask..
            //To-do: Shenanigans
            Array.Copy(this.Payload, 0, packet, nextbyte, this.PayloadLength);

            return packet;


        }


    }
}
