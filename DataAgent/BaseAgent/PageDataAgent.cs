using CommonModel;
using DataAccess;
using DataAccess.Interface;
using DataAccess.NetWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAgent.BaseAgent
{
    public class PageDataAgent : IDisposable, ICMDReceiver
    {
        public IBaseRepository repo = null;

        public INetReceiver ReceiverViewModel = null;
        public PageDataAgent(IBaseRepository repo)
        {
            this.repo = repo;
        }
        public void SetReceiver(INetReceiver viewmodel)
        {
            this.ReceiverViewModel = viewmodel;
            repo.SetReceiver(this.ReceiverViewModel);
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

        public void OnRceivedData(ErpPacket packet)
        {
            this.ReceiverViewModel.OnRceivedData(packet);
        }

        public void OnReceiveFail(object sender, Exception ex)
        {
            
        }

        public void OnSendFail(object sender, Exception ex)
        {
            
        }
    }
}
