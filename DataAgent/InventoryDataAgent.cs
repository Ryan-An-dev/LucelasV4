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
    public class InventoryDataAgent : PageDataAgent
    {
        private IInventoryRepository repo = null;

        public InventoryDataAgent(IInventoryRepository repo):base(repo)
        {
            this.repo = repo;
        }
        public void Get(JObject msg)
        {
            this.repo.Get(msg);
        }
        public void Create(JObject msg)
        {
            this.repo.Create(msg);
        }
        public void Update(JObject msg)
        {
            this.repo.Update(msg);
        }
        public void Delete(JObject msg)
        {
            this.repo.Delete(msg);
        }
    }
}
