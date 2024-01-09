using DataAccess.NetWork;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interface
{
    public interface IDeliveryRepository : IBaseRepository
    {
        #region LIST CRUD
        public void CreateDeliveryList(JObject msg);
        public void GetDeliveryList(JObject msg);

        public void UpdateDeliveryList(JObject msg);
        public void DeleteDeliveryList(JObject msg);
        #endregion

    }
}
