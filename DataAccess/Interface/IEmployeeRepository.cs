using DataAccess.NetWork;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interface
{
    public interface IEmployeeRepository : IBaseRepository
    {
        #region LIST CRUD
        public void CreateEmployee(JObject msg);
        public void GetEmployee(JObject msg);

        public void UpdateEmployee(JObject msg);
        public void DeleteEmployee(JObject msg);
        #endregion

    }
}
