using DataAccess.Interface;
using DataAccess.NetWork;
using Newtonsoft.Json.Linq;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class AccountRepository : IAccountRepository, ICMDReceiver, IDisposable
    {
        private SocketClientV2 NetManager;

        INetReceiver _Receiver = null;

        private IContainerProvider _Container;

        public AccountRepository(IContainerProvider Container)
        {
            this._Container = Container;
            NetManager = this._Container.Resolve<SocketClientV2>();
            NetManager.SetReceiver(this);
        }
        public void SetReceiver(INetReceiver netReceiver)
        {
            this._Receiver = netReceiver;
        }
        public void OnRceivedData(ErpPacket packet)
        {
            this._Receiver.OnRceivedData(packet);
        }

        public void OnConeectedFail(object sender, Exception ex)
        {
           
        }

        public void OnConnected()
        {
           
        }

     
        public void OnReceiveFail(object sender, Exception ex)
        {
            
        }

        public void OnSendFail(object sender, Exception ex)
        {
          
        }


        public void Dispose()
        {
            this.NetManager.Close();
        }

        public void CreateAccount(JObject msg)
        {
            if (NetManager.session_id != 0)
            {
                NetManager.Send(msg, COMMAND.CREATE_ACCOUNT_INFO);
            }
        }

        public void GetAccount(JObject msg)
        {
            if (NetManager.session_id != 0)
            {
                NetManager.Send(msg, COMMAND.AccountLIst);
            }
        }

        public void UpdateAccount(JObject msg)
        {
            if (NetManager.session_id != 0)
            {
                NetManager.Send(msg, COMMAND.UPDATE_ACCOUNT_INFO);
            }
        }

        public void DeleteAccount(JObject msg)
        {
            if (NetManager.session_id != 0)
            {
                NetManager.Send(msg, COMMAND.DELETE_ACCOUNT_INFO);
            }
        }
    }
}
