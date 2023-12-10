using DataAccess.NetWork;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interface
{
    public interface IInventoryRepository : IBaseRepository
    {
        #region LIST CRUD
        public void Create(JObject msg);
        public void Get(JObject msg);

        public void Update(JObject msg);
        public void Delete(JObject msg);
        #endregion

    }
}
