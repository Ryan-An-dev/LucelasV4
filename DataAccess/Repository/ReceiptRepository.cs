using CommonModel.Model;
using DataAccess.Interface;
using DataAccess.NetWork;
using Newtonsoft.Json.Linq;
using Prism.Ioc;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class ReceiptRepository : IReceiptRepository, ICMDReceiver , IDisposable
    {
        private SocketClientV2 NetManager;

        private IContainerProvider _Container;

        INetReceiver _Receiver = null;

        public ReceiptRepository(IContainerProvider Container)
        {
            this._Container = Container;
            NetManager=this._Container.Resolve<SocketClientV2>();
            NetManager.SetReceiver(this);
        }

        public void SetReceiver(INetReceiver netReceiver) {
            this._Receiver = netReceiver;
        }
        public void OnRceivedData(ErpPacket packet)
        {
            this._Receiver.OnRceivedData(packet);
        }

        public void OnConnected()
        {
            Console.WriteLine("Connected");
        }

        public void OnSent()
        {
            Console.WriteLine("OnSent");
        }

        public void Dispose()
        {
            NetManager.Close();
            Console.WriteLine("Dispose");
        }

        public void GetReceiptCategoryList()
        {
            if (NetManager.session_id != 0) {
                NetManager.Send(COMMAND.CategoryList);
            }
        }

        public void GetProductCategoryList()
        {
            if (NetManager.session_id != 0)
            {
                NetManager.Send(COMMAND.ProductCategoryList);
            }
        }

        public void GetBankCardList()
        {
            if (NetManager.session_id != 0)
            {
                NetManager.Send(COMMAND.AccountLIst);
            }
        }

        public void CreateBankHistory(JObject msg)
        {
            if (NetManager.session_id != 0)
            {
                NetManager.Send(msg,COMMAND.CreateBankHistory);
            }
        }

        public void GetBankHistoryList(JObject msg)
        {
            if (NetManager.session_id != 0)
            {
                NetManager.Send(msg, COMMAND.GetBankHistory);
            }
        }

        public void UpdateBankHistory(JObject msg)
        {
            if (NetManager.session_id != 0)
            {
                NetManager.Send(msg, COMMAND.UpdateBankHistory);
            }
        }

        public void DeleteBankHistory(JObject msg)
        {
            if (NetManager.session_id != 0)
            {
                NetManager.Send(msg, COMMAND.DeleteBankHistory);
            }
        }
        public void GetConnectedContract(JObject msg) {
            if (NetManager.session_id != 0)
            {
                NetManager.Send(msg, COMMAND.GetConnectedContract);
            }
        }

        public void OnConeectedFail(object sender, Exception ex)
        {
            Console.WriteLine("OnConeectedFail");
        }

        public void OnSendFail(object sender, Exception ex)
        {
            NetManager.Disconnect();
        }

        public void OnReceiveFail(object sender, Exception ex)
        {
            Console.WriteLine("OnReceiveFail");
        }
    }
}
