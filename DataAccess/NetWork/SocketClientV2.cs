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
using System.Timers;
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
        protected int _beforeRemain = 0;
        protected int _SessionID;
        protected Stream _ClientStream;
        protected bool _IsConnected;
        protected int _bufferSize; 
        private PacketParser _packet;

        private ICMDReceiver _receiver;
        public ICMDReceiver Receiver
        {
            get { return _receiver; }
            set { _receiver = value; }
        }
        private ICMDReceiver _MainVM;
        public ICMDReceiver MainVM
        {
            get { return _MainVM; }
            set { _MainVM = value; }
        }

        public void SetReceiver(ICMDReceiver recv)
        {
            this.Receiver = recv;
        }

        public void SetMainConnectCheck(ICMDReceiver mainviewmodel) {
            this._MainVM = mainviewmodel;   
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
        System.Timers.Timer m_timer;
        private int m_nKeepAliveTime = 5;
        public void Connect(string IP, int Port)
        {
            this.IP = IP;
            this.Port = Port;
            mainSock = new TcpClient();
            mainSock.Client.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName)27, false);
            this.mainSock.BeginConnect(this.IP, this.Port, ConnectCallback, this);
        }
        public void TryLogin(string id, string pw) {
            if (id != string.Empty) { 
                this.ID = id;
                this.PW = pw;
            }
            JObject body = new JObject();
            body["login_id"] = this.ID;
            body["login_pw"] = this.PW;
            Send(body, COMMAND.LOGIN);
        }

        public void Reconnect()
        {
            if (mainSock != null) {
                this.mainSock.Close();
                this.mainSock.Dispose();
                this.mainSock = null;
            }
            mainSock = new TcpClient();
            mainSock.Client.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName)27, false);
            this.mainSock.BeginConnect(this.IP, this.Port, ConnectCallback, this);
        }

        public void Close()
        {
            ErpLogWriter.LogWriter.Trace(string.Format("socket Close And Null처리"));
            if (mainSock != null)
            {
                if (mainSock.Connected)
                {
                    mainSock.GetStream().Close();
                    mainSock.Close();
                }
                mainSock.Dispose();
                _packet = new PacketParser();
                _RecvBufferPos = 0;
            }
            mainSock = null;
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
                if(this.MainVM != null)
                    this.MainVM.OnConnected();
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
                this.state = ConnectState.Disconnected;
                MainVM.OnReceiveFail(null,null);
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
            Thread.Sleep(50);
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
                    //KeepAlive 가동시작
                    Initialize();

                }
            }
            this._packet.InitIndex();
        }

        private void Initialize()
        {
            //m_timer = new System.Timers.Timer();
            //m_timer.Interval = m_nKeepAliveTime * 1000; // 100 Milliseconds 
            //m_timer.Elapsed += M_timer_Elapsed;
            //m_timer.Start();
        }

        private void M_timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            JObject jobj = new JObject();
            jobj["keep_alive"] = 1;
            Send(jobj,COMMAND.KeepAlive);
            //System.Diagnostics.Debug.WriteLine("Send Keep Alive " + DateTime.Now);
        }

        public void Disconnect()
        {
            ErpLogWriter.LogWriter.Trace(string.Format("Keep Alive 종료시키기"));
            try { this.Close(); } catch (Exception e) { }
            this.state = ConnectState.Disconnected;
            if(this.m_timer!=null)
                this.m_timer.Stop();
            this.Close();
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
                this.state = ConnectState.Disconnected;
                if (Receiver != null) {
                    this.Close();
                    Receiver.OnSendFail(this, ex);
                    MainVM.OnSendFail(null, null);
                }

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
                this.state = ConnectState.Disconnected;
                if (clnt.Receiver != null) {
                    this.Close();
                    clnt.Receiver.OnSendFail(this, ex);
                    clnt.MainVM.OnSendFail(null, null);
                
                }
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
