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
    public class DeliveryDataAgent : PageDataAgent
    {
        private IDeliveryRepository repo = null;

        public DeliveryDataAgent(IDeliveryRepository repo):base(repo)
        {
            this.repo = repo;
        }
        public void GetDeliveryList(JObject msg)
        {
            this.repo.GetDeliveryList(msg);
        }
        public void CreateDeliveryList(JObject msg)
        {
            this.repo.CreateDeliveryList(msg);
        }
        public void UpdateDeliveryList(JObject msg)
        {
            this.repo.UpdateDeliveryList(msg);
        }
        public void DeleteDeliveryList(JObject msg)
        {
            this.repo.DeleteDeliveryList(msg);
        }
    }
}
