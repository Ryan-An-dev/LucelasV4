using DataAccess.NetWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interface
{
    public interface ICMDReceiver
    {
        void OnRceivedData(ErpPacket packet);
        void OnConnected();
        void OnConeectedFail(object sender, Exception ex);
        void OnSendFail(object sender, Exception ex);
        void OnReceiveFail(object sender, Exception ex);
    }
}
