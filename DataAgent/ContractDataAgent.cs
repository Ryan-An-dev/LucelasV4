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
    public class ContractDataAgent : PageDataAgent
    {
        private IContractRepository repo = null;

        public ContractDataAgent(IContractRepository repo):base(repo)
        {
            this.repo = repo;
        }
        public void GetContractForReceipt(JObject msg)
        {
            this.repo.GetContractForReceipt(msg);
        }
        public void GetConnectedContract(JObject msg)
        {
            this.repo.GetConnectedContract(msg);
        }
        public void GetContractList(JObject msg)
        {
            this.repo.GetContractList(msg);
        }
        public void CreateContractHistory(JObject msg)
        {
            this.repo.CreateContractHistory(msg);
        }
        public void UpdateContract(JObject msg)
        {
            this.repo.UpdateContract(msg);
        }
        public void DeleteContract(JObject msg)
        {
            this.repo.DeleteContract(msg);
        }
    }
}
