using DataAccess.NetWork;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interface
{
    public interface IAccountRepository : IBaseRepository
    {
        #region LIST CRUD
        public void CreateAccount(JObject msg);
        public void GetAccount(JObject msg);

        public void UpdateAccount(JObject msg);
        public void DeleteAccount(JObject msg);
        #endregion

    }
}
