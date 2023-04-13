using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.NetWork
{
    public interface INetReceiver
    {
        void OnRceivedData(ErpPacket packet);
        void OnConnected();
        void OnSent();
    }
}
