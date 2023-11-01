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
    public class ProductRepository : IProductRepository, ICMDReceiver, IDisposable
    {
        private SocketClientV2 NetManager;

        INetReceiver _Receiver = null;

        private IContainerProvider _Container;
        public ProductRepository(IContainerProvider Container)
        {
            this._Container = Container;
            NetManager = this._Container.Resolve<SocketClientV2>();
            NetManager.SetReceiver(this);
        }
        public void SetReceiver(INetReceiver netReceiver)
        {
            this._Receiver = netReceiver;
        }
        public void Create(JObject msg)
        {
            if (NetManager.session_id != 0)
            {
                NetManager.Send(msg, COMMAND.CREATEPRODUCTINFO);
            }
        }
        public void Read(JObject msg)
        {
            if (NetManager.session_id != 0)
            {
                NetManager.Send(msg, COMMAND.GETPRODUCTINFO);
            }
        }
      

        public void Update(JObject msg)
        {
            if (NetManager.session_id != 0)
            {
                NetManager.Send(msg, COMMAND.UPDATEPRODUCTINFO);
            }
        }

        public void Delete(JObject msg)
        {
            if (NetManager.session_id != 0)
            {
                NetManager.Send(msg, COMMAND.DELETEPRODUCTINFO);
            }
        }
        public void OnRceivedData(ErpPacket packet)
        {
            this._Receiver.OnRceivedData(packet);
        }


        public void Dispose()
        {
            
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

        
    }
}
