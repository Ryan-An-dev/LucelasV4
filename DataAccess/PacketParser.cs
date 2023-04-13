using DataAccess.NetWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public enum PacketStepEnum { Header, Body }

    public class PacketHeader
    {
        public PacketHeader()
        {
            IC = new byte[4];
            IC = Encoding.Default.GetBytes("FERP");
        }
        public byte[] IC { get; set; }
        public ushort CMD { get; set; }
        public ushort Response { get; set; }
        public ushort ScrapIndex { get; set; }
        public int Length { get; set; }

        public bool CheckIC(byte[] data, int index)
        {
            bool rslt = true;
            for (int i = 0; i < 4; i++)
            {
                rslt &= this.IC[i] == data[index + i];
            }

            return rslt;
        }
        public byte[] GetBytes(byte[] body, int len, COMMAND cmd)
        {
            this.CMD = (ushort)cmd;
            this.Length = len;

            byte[] sendData = new byte[PacketParser.HEADER_LENGTH + len];
            PacketSerializer serializer = new PacketSerializer();
            serializer.Serialize(this.IC);
            serializer.Serialize(this.CMD);
            serializer.Serialize(this.Response);
            serializer.Serialize(this.ScrapIndex);
            serializer.Serialize(this.Length);
            Array.Copy(serializer.GetBytes(), 0, sendData, 0, PacketParser.HEADER_LENGTH);
            Array.Copy(body, 0, sendData, PacketParser.HEADER_LENGTH, len);

            return sendData;
        }
    }
    public class ErpPacket
    {
        public ErpPacket()
        {
            this.Header = new PacketHeader();
        }
        public PacketHeader Header { get; set; }
        public byte[] Body { get; set; }
    }

    public class PacketParser
    {
        public const int HEADER_LENGTH = 14;
        private int _HeaderIndex;
        private int _BodyIndex;
        private byte[] _HeaderBuffer;
        private byte[] _IC;

        private ErpPacket _CurrentPacket;
        public PacketParser()
        {
            this._HeaderBuffer = new byte[HEADER_LENGTH];
            this._IC = new byte[4];
            this._IC[0] = (byte)'F';
            this._IC[1] = (byte)'E';
            this._IC[2] = (byte)'R';
            this._IC[3] = (byte)'P';
        }


        #region PacketStep
        private PacketStepEnum _PacketStep;
        public PacketStepEnum PacketStep
        {
            get { return _PacketStep; }
            set { _PacketStep = value; }
        }
        #endregion


        #region BodyLength
        public int BodyLength { get; private set; }
        #endregion

        #region HeaderComplete
        public bool HeaderComplete
        {
            get { return this._HeaderIndex == HEADER_LENGTH; }
        }
        #endregion


        #region BodyComplete
        public bool BodyComplete
        {
            get { return this._BodyIndex == this.BodyLength; }
        }
        #endregion

        private bool ParsePacketHeader()
        {
            bool icVaild = false;
            int cmdInt = 0;
            try
            {
                if (this._CurrentPacket == null)
                {
                    this._CurrentPacket = new ErpPacket();
                    this._CurrentPacket.Header.IC = this._IC;
                }

                int index = 0;

                icVaild = this._CurrentPacket.Header.CheckIC(this._HeaderBuffer, 0);
                if (!icVaild)
                {
                    return false;
                }
                index += 4;
                this._CurrentPacket.Header.CMD  = (ushort)BitConverter.ToInt16(_HeaderBuffer, index);
                index += 2;
                this._CurrentPacket.Header.Response = (ushort)BitConverter.ToInt16(_HeaderBuffer, index);
                index += 2;
                this._CurrentPacket.Header.ScrapIndex = (ushort)BitConverter.ToInt16(_HeaderBuffer, index);
                index += 2;
                this.BodyLength = BitConverter.ToInt32(this._HeaderBuffer, index);


                //Thread.Sleep(5);

                this._CurrentPacket.Body = new byte[this.BodyLength];
                this._CurrentPacket.Header.Length = this.BodyLength;
                this.PacketStep = PacketStepEnum.Body;
            }
            catch (Exception ex)
            {
                throw;
            }
            return true;
        }


        public int CopyHeader(byte[] data, int offset)
        {
            int readLen = 0;
            int copyLen = HEADER_LENGTH - this._HeaderIndex;
            int dataRemainLen = data.Length - offset;
            readLen = Math.Min(copyLen, dataRemainLen);

            Array.Copy(data, offset, this._HeaderBuffer, this._HeaderIndex, readLen);

            int beforeIndex = this._HeaderIndex;

            this._HeaderIndex += readLen;

            if (this.HeaderComplete)
            {
                if (!ParsePacketHeader())
                {
                    this._HeaderIndex = 0;
                    readLen = 0;
                }
            }
            return readLen;
        }

        public int CopyBody(byte[] data, int offset)
        {
            int copyLen = this.BodyLength - this._BodyIndex;
            int dataRemainLen = data.Length - offset;
            int readLen = Math.Min(copyLen, dataRemainLen);

            if (this.BodyLength >= 0)
                Array.Copy(data, offset, this._CurrentPacket.Body, this._BodyIndex, readLen);
            this._BodyIndex += readLen;

            return readLen;
        }

        public ErpPacket GetCompletePacket()
        {
            ErpPacket compPack = this._CurrentPacket;
            this._CurrentPacket = null;
            return compPack;
        }


        public byte[] CreateSendData(COMMAND cmd, string rawData)
        {
            byte[] bodyByte = Encoding.Default.GetBytes(rawData);
            int bodyLength = bodyByte.Length;

            byte[] rslt = new byte[HEADER_LENGTH + bodyLength];
            byte[] cmdByte = BitConverter.GetBytes((int)cmd);
            byte[] lenByte = BitConverter.GetBytes(bodyLength);

            int index = 0;
            Array.Copy(this._IC, 0, rslt, index, this._IC.Length);
            index += this._IC.Length;

            Array.Copy(cmdByte, 0, rslt, index, cmdByte.Length);
            index += cmdByte.Length;

            Array.Copy(lenByte, 0, rslt, index, lenByte.Length);
            index += lenByte.Length;

            Array.Copy(bodyByte, 0, rslt, index, bodyLength);


            return rslt;
        }

        public byte[] CreateSendData(COMMAND cmd, byte[] bodyByte)
        {
            int bodyLength = bodyByte.Length;

            byte[] rslt = new byte[HEADER_LENGTH + bodyLength];
            byte[] cmdByte = BitConverter.GetBytes((int)cmd);
            byte[] lenByte = BitConverter.GetBytes(bodyLength);

            int index = 0;
            Array.Copy(this._IC, 0, rslt, index, this._IC.Length);
            index += this._IC.Length;

            Array.Copy(cmdByte, 0, rslt, index, cmdByte.Length);
            index += cmdByte.Length;

            Array.Copy(lenByte, 0, rslt, index, lenByte.Length);
            index += lenByte.Length;

            Array.Copy(bodyByte, 0, rslt, index, bodyLength);


            return rslt;
        }

        public void InitIndex()
        {
            this._BodyIndex = 0;
            this._HeaderIndex = 0;
            this.PacketStep = PacketStepEnum.Header;
        }
    }
}
