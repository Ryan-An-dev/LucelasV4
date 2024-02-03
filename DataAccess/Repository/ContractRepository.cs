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
    public class ContractRepository : IContractRepository, ICMDReceiver, IDisposable
    {
        private SocketClientV2 NetManager;

        INetReceiver _Receiver = null;

        private IContainerProvider _Container;

        public ContractRepository(IContainerProvider Container)
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
        public void GetConnectedPayment(JObject msg) {
            if (NetManager.session_id != 0)
            {
                NetManager.Send(msg, COMMAND.GET_CONNECTED_PAYMENT);
            }
        }

        public void CreateContractHistory(JObject msg)
        {
            if (NetManager.session_id != 0)
            {
                NetManager.Send(msg, COMMAND.CREATECONTRACT);
            }
        }

        public void DeleteContract(JObject msg)
        {
            if (NetManager.session_id != 0)
            {
                NetManager.Send(msg, COMMAND.DELETECONTRACT);
            }
        }

        public void GetContractList(JObject msg)
        {
            if (NetManager.session_id != 0)
            {
                NetManager.Send(msg,COMMAND.GetContractList);
            }
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

        public void UpdateContract(JObject msg)
        {
            if (NetManager.session_id != 0)
            {
                NetManager.Send(msg, COMMAND.UPDATECONTRACT);
            }
        }

        public void Dispose()
        {
            this.NetManager.Close();
        }

        public void GetConnectedContract(JObject msg)
        {
            if (NetManager.session_id != 0)
            {
                NetManager.Send(msg, COMMAND.GET_CONNECTED_CONTRACT);
            }
        }

        public void GetContractForReceipt(JObject msg)
        {
            if (NetManager.session_id != 0)
            {
                NetManager.Send(msg, COMMAND.GET_CONTRACT_FOR_RECEIPT);
            }
        }
    }
}
