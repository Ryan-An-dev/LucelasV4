using DataAccess.NetWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interface
{
    public interface ILoginRepository: IBaseRepository
    {
        public void CheckLogin(string ip, int port, string id, string pw);

        public void RequestGlobalData();

    }
}
