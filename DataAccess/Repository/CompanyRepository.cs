﻿using DataAccess.Interface;
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
    public class CompanyRepository : ICompanyRepository, ICMDReceiver, IDisposable
    {
        private SocketClientV2 NetManager;

        INetReceiver _Receiver = null;

        private IContainerProvider _Container;
        public CompanyRepository(IContainerProvider Container)
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
                NetManager.Send(msg, COMMAND.CREATECOMPANYINFO);
            }
        }
        public void Read(JObject msg)
        {
            if (NetManager.session_id != 0)
            {
                NetManager.Send(msg, COMMAND.GETCOMPANYINFO);
            }
        }
      

        public void Update(JObject msg)
        {
            if (NetManager.session_id != 0)
            {
                NetManager.Send(msg, COMMAND.UPDATECOMPANYINFO);
            }
        }

        public void Delete(JObject msg)
        {
            if (NetManager.session_id != 0)
            {
                NetManager.Send(msg, COMMAND.DELETECOMPANYINFO);
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
