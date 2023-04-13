using DataAccess.Interface;
using LogWriter;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace DataAccess.NetWork
{
    public enum ConnectState { 
        Connected = 1,
        Disconnected = 2,
    }
    public class SocketClientV2
    {
        protected const int BUFFER_SIZE = 16384;
        protected bool _IsDisposed;
        protected int _RemainBufferSize = 0;
        protected int _RecvBufferPos = 0;
        protected int _SessionID;
        protected Stream _ClientStream;
        protected bool _IsConnected;
        protected int _beforeRemain = 0;
        protected int _bufferSize; 
        private PacketParser _packet;

        private ICMDReceiver _receiver;
        public ICMDReceiver Receiver
        {
            get { return _receiver; }
            set { _receiver = value; }
        }

        public void SetReceiver(ICMDReceiver recv)
        {
            this.Receiver = recv;
        }
        private byte[] Buffer { get; set; }
        public ConnectState state { get; set; }

        public SocketClientV2()
        {
            this.Buffer = new byte[BUFFER_SIZE]; 
            state = ConnectState.Disconnected;
            this._packet= new PacketParser();
        }

        public TcpClient mainSock;
        public Stream ClientStream
        {
            get { return _ClientStream; }
            set
            {
                _ClientStream = value;
            }
        }
        int m_port = 2001;
        private string IP = "";
        private int Port = 0;
        private string ID = "";
        private string PW = "";
        public int session_id = 0;
        public void Connect(string IP, int Port)
        {
            this.IP = IP;
            this.Port = Port;
            mainSock = new TcpClient();
            mainSock.Client.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName)27, false);
            this.mainSock.BeginConnect(this.IP, this.Port, ConnectCallback, this);
        }
        public void Close()
        {
            if (mainSock != null)
            {
                mainSock.Close();
                mainSock.Dispose();
            }
        }

        void ConnectCallback(IAsyncResult result)
        {
            try
            {
                SocketClientV2 clnt = result.AsyncState as SocketClientV2;
                clnt.mainSock.EndConnect(result);
                this.state = ConnectState.Connected;
                clnt.ClientStream = clnt.mainSock.GetStream();
                this.Receiver.OnConnected();
                StartReceive(Buffer,0, BUFFER_SIZE);
            }
            catch (Exception e)
            {
                this.Receiver.OnConeectedFail(this, e);
                this.state = ConnectState.Disconnected;
            }
        }
        public void StartReceive(byte[] data, int offset, int len)
        {
            try
            {
                if (_ClientStream != null && _ClientStream.CanRead)
                    _ClientStream.BeginRead(data, offset, len, OnReceived, this);
            }
            catch (IOException) { }

        }
        public void OnReceived(IAsyncResult result)
        {
            SocketClientV2 clnt = result.AsyncState as SocketClientV2;
            int recvLen = 0;

            try
            {
                if (mainSock.Connected)
                    recvLen = clnt.ClientStream.EndRead(result);
                //Begin Read에서 설정한 offset과 len의 차이만큼만 data가 들어온다.
            }
            catch (Exception ex)
            {
                ;
            }

            OnRceivedData(recvLen);
        }
        public void OnRceivedData(int len)
        {
            if (len > 0)
            {
                _beforeRemain = ProcessPakcet(Buffer, _RecvBufferPos - _beforeRemain, len + _beforeRemain);
                CheckBuffer(len, _beforeRemain);
            }
            else
            {
                Receiver.OnReceiveFail(null,null);
                return;
            }
            StartReceive();
        }
        public void StartReceive()
        {

            StartReceive(Buffer, _RecvBufferPos, BUFFER_SIZE - _RecvBufferPos);
        }
        private void CheckBuffer(int recvLen, int remain)
        {
            int pos = _RecvBufferPos + recvLen;

            if (pos >= BUFFER_SIZE)
            {
                int idx = BUFFER_SIZE - pos;
                Array.Clear(Buffer, idx, pos - remain);
                Array.Copy(Buffer, pos - remain, Buffer, 0, remain);
                _RecvBufferPos = remain;
            }
            else
                this._RecvBufferPos = pos;
        }
        
        public int ProcessPakcet(byte[] data, int startPos, int size)
        {
            int rslt = 0;
            DateTime start = DateTime.Now;
            while (rslt < size)
            {
                switch (this._packet.PacketStep)
                {
                    case PacketStepEnum.Header:
                        {
                            int headerLen = this._packet.CopyHeader(data, startPos + rslt);
                            if (headerLen == 0)
                            {
                                this.Receiver.OnReceiveFail(null, null);
                            }
                            if (this._packet.HeaderComplete && this._packet.BodyLength == 0)
                            {
                                SendCompletePacket();
                            }
                            rslt += headerLen;
                        }
                        break;
                    case PacketStepEnum.Body:
                        {
                            int bodyLen = this._packet.CopyBody(data, startPos + rslt);
                            if (this._packet.BodyComplete)
                            {
                                SendCompletePacket();
                            }
                            rslt += bodyLen;
                        }
                        break;
                    default:
                        break;
                }
            }
            Thread.Sleep(30);
            return 0;

        }

        private void SendCompletePacket()
        {
            if (this.Receiver != null)
            {
                ErpPacket msg = this._packet.GetCompletePacket();
                this.Receiver.OnRceivedData(msg);
                if ((COMMAND)msg.Header.CMD == COMMAND.LOGIN)
                {
                    string text = Encoding.UTF8.GetString(msg.Body);
                    JObject jobj = new JObject(JObject.Parse(text));
                    if (jobj["session_id"] != null)
                        this.session_id = jobj["session_id"].ToObject<int>();
                }
            }
            this._packet.InitIndex();
        }
        public void Disconnect()
        {
            this.state = ConnectState.Disconnected;
        }

        public void Send(JObject msg, COMMAND CMD)
        {
            ErpLogWriter.LogWriter.Trace(string.Format("CMD : {0} SendData : {1}", CMD.ToString(), msg.ToString()));
            PacketHeader header = new PacketHeader();
            header.CMD = (ushort)CMD;
            header.ScrapIndex = 0;
            header.Response = 0;
            header.Length = 0;
            if (header.CMD != (ushort)COMMAND.LOGIN)
            {
                msg["session_id"] = this.session_id;
            }
            string temp = msg.ToString();
            byte[] data = Encoding.UTF8.GetBytes(temp);
            data = header.GetBytes(data, data.Length, (COMMAND)header.CMD);
            this.Send(data, 0, data.Length);
        }
        public void Send(byte[] data, int offset, int len)
        {
            try
            {
                if (_ClientStream != null)
                    _ClientStream.BeginWrite(data, offset, len, OnSent, this);
            }
            catch (Exception ex)
            {
                if (Receiver != null)
                    Receiver.OnSendFail(this, ex);
            }
        }
        public void OnSent(IAsyncResult result)
        {
            SocketClientV2 clnt = result.AsyncState as SocketClientV2;

            try
            {
                clnt.ClientStream.EndWrite(result);
            }
            catch (Exception ex)
            {
                if (clnt.Receiver != null)
                    clnt.Receiver.OnSendFail(this, ex);
            }
        }
        public void Send(COMMAND CMD)
        {
            ErpLogWriter.LogWriter.Trace(string.Format("CMD : {0} SendData : NaN", CMD.ToString()));
            PacketHeader header = new PacketHeader();
            header.CMD = (ushort)CMD;
            header.ScrapIndex = 0;
            header.Response = 0;
            header.Length = 0;
            JObject jobj = new JObject();
            if (header.CMD != (ushort)COMMAND.LOGIN)
            {
                jobj["session_id"] = this.session_id;
            }
            string temp = jobj.ToString();
            byte[] data = Encoding.UTF8.GetBytes(temp);
            data = header.GetBytes(data, data.Length, (COMMAND)header.CMD);
            this.Send(data, 0, data.Length);
        }
    }
}
