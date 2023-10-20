using DataAccess.NetWork;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interface
{
    public interface ICustomerRepository : IBaseRepository
    {
        #region LIST CRUD
        public void CreateCustomer(JObject msg);
        public void GetCustomer(JObject msg);

        public void UpdateCustomer(JObject msg);
        public void DeleteCustomer(JObject msg);
        #endregion

    }
}
