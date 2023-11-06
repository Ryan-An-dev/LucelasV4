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
    public class ProductCategoryRepository : IProductCategoryRepository, ICMDReceiver, IDisposable
    {
        private SocketClientV2 NetManager;

        INetReceiver _Receiver = null;

        private IContainerProvider _Container;
        public ProductCategoryRepository(IContainerProvider Container)
        {
            this._Container = Container;
            NetManager = this._Container.Resolve<SocketClientV2>();
            NetManager.SetReceiver(this);
        }
        public void SetReceiver(INetReceiver netReceiver)
        {
            this._Receiver = netReceiver;
        }
        public void CreateProductCategory(JObject msg)
        {
            if (NetManager.session_id != 0)
            {
                NetManager.Send(msg, COMMAND.CREATE_PRODUCTCATEGORY_INFO);
            }
        }
        public void GetProductCategory(JObject msg)
        {
            if (NetManager.session_id != 0)
            {
                NetManager.Send(msg, COMMAND.ProductCategoryList);
            }
        }

        public void UpdateProductCategory(JObject msg)
        {
            if (NetManager.session_id != 0)
            {
                NetManager.Send(msg, COMMAND.UPDATE_PRODUCTCATEGORY_INFO);
            }
        }

        public void DeleteProductCategory(JObject msg)
        {
            if (NetManager.session_id != 0)
            {
                NetManager.Send(msg, COMMAND.DELETE_PRODUCTCATEGORY_INFO);
            }
        }

        public void OnRceivedData(ErpPacket packet)
        {
            this._Receiver.OnRceivedData(packet);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void OnConeectedFail(object sender, Exception ex)
        {
            throw new NotImplementedException();
        }

        public void OnConnected()
        {
            throw new NotImplementedException();
        }

  
        public void OnReceiveFail(object sender, Exception ex)
        {
            throw new NotImplementedException();
        }

        public void OnSendFail(object sender, Exception ex)
        {
            throw new NotImplementedException();
        }



    }
}
