using DataAccess;
using DataAccess.Interface;
using DataAccess.NetWork;
using DataAgent.BaseAgent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAgent
{
    public class LoginAgent : PageDataAgent
    {
        private ILoginRepository repo = null;
        public LoginAgent(ILoginRepository repo) : base(repo)
        {
            this.repo = repo;
        }

        public void TryLogin(string ip, int port, string id, string pw) {
            this.repo.CheckLogin(ip, port, id, pw);
        }
    }
}
