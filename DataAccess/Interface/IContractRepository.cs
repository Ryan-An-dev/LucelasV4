using DataAccess.NetWork;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interface
{
    public interface IContractRepository: IBaseRepository
    {
        #region LIST CRUD
        public void CreateContractHistory(JObject msg);
        public void GetContractList(JObject msg);

        public void UpdateContract(JObject msg);
        public void DeleteContract(JObject msg);
        public void GetConnectedContract(JObject msg);

        public void GetContractForReceipt(JObject msg);
        #endregion

    }
}
