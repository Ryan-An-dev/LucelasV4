using DataAccess;
using DataAccess.Interface;
using DataAccess.NetWork;
using DataAgent.BaseAgent;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAgent
{
    public class AccountDataAgent : PageDataAgent
    {
        private IAccountRepository repo = null;

        public AccountDataAgent(IAccountRepository repo):base(repo)
        {
            this.repo = repo;
        }
        public void GetAccountList(JObject msg)
        {
            this.repo.GetAccount(msg);
        }
        public void CreateAccountList(JObject msg)
        {
            this.repo.CreateAccount(msg);
        }
        public void UpdateAccountList(JObject msg)
        {
            this.repo.UpdateAccount(msg);
        }
        public void DeleteAccountList(JObject msg)
        {
            this.repo.DeleteAccount(msg);
        }
    }
}
