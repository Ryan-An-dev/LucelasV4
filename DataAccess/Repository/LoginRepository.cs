using DataAccess.Interface;
using DataAccess.NetWork;
using LogWriter;
using Newtonsoft.Json.Linq;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class LoginRepository : ILoginRepository, ICMDReceiver
    {
        private SocketClientV2 NetManager;

        INetReceiver _Receiver = null;

        private IContainerProvider _Container;
        private string ip = "";
        private int port = 0;
        private string id = "";
        private string pw = "";
        public LoginRepository(IContainerProvider Container)
        {
            this._Container = Container;
            NetManager = this._Container.Resolve<SocketClientV2>();
            NetManager.SetReceiver(this);
        }
        public void Clear() {
            this.NetManager.Disconnect();
        }
        public void SetReceiver(INetReceiver netReceiver)
        {
            this._Receiver = netReceiver;
        }

        public void CheckLogin(string ip , int port, string id , string pw)
        {
            this.ip = ip;
            this.port = port;
            this.id = id;
            this.pw = pw;
            if (this.NetManager.state == ConnectState.Disconnected)
            {
                this.NetManager.Connect(ip, port);
            }
            else {
                this.NetManager.TryLogin(id, pw);
            }
        }
        public void TryLogin(string id, string pass)
        {
            this.NetManager.TryLogin(id, pass); 
            
        }
        public void Reconnect() {
            this.NetManager.Reconnect();
        }
        public void RequestGlobalData()
        {
            this.NetManager.Send(COMMAND.AccountLIst);
            this.NetManager.Send(COMMAND.CategoryList);
            //this.NetManager.Send(COMMAND.ProductCategoryList);
        }

        public void OnRceivedData(ErpPacket packet)
        {
            this._Receiver.OnRceivedData(packet);
        }

        public void OnConnected()
        {
            TryLogin(this.id, this.pw);
        }

        public void OnConeectedFail(object sender, Exception ex)
        {
            Console.WriteLine("OnConnectedFail");
        }

        public void OnSendFail(object sender, Exception ex)
        {
            Console.WriteLine("OnSendFail");
        }

        public void OnReceiveFail(object sender, Exception ex)
        {
            Console.WriteLine("OnReceiveFail");
        }

    }
}
