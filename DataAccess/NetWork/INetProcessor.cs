using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.NetWork
{
    public interface INetProcessor
    {
        void OnRceivedData(int len);
        void OnConnected(bool isConnected);
        void OnSent();
        void OnAccept(SocketClientV2 clnt, bool isAccpeted);
    }
}
