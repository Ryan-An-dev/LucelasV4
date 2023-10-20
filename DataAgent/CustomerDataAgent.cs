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
    public class CustomerDataAgent : PageDataAgent
    {
        private ICustomerRepository repo = null;

        public CustomerDataAgent(ICustomerRepository repo):base(repo)
        {
            this.repo = repo;
        }
        public void GetCustomerList(JObject msg)
        {
            this.repo.GetCustomer(msg);
        }
        public void CreateCustomerList(JObject msg)
        {
            this.repo.CreateCustomer(msg);
        }
        public void UpdateCustomerList(JObject msg)
        {
            this.repo.UpdateCustomer(msg);
        }
        public void DeleteCustomerList(JObject msg)
        {
            this.repo.DeleteCustomer(msg);
        }
    }
}
