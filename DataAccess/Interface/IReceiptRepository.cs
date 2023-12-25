using CommonModel.Model;
using DataAccess.NetWork;
using Newtonsoft.Json.Linq;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interface
{
    public interface IReceiptRepository : IBaseRepository
    {
        #region 사전 데이터
        public void GetReceiptCategoryList();
        public void GetProductCategoryList();
        public void GetBankCardList();
        #endregion

        #region LIST CRUD
        public void CreateBankHistory(JObject msg);
        public void GetBankHistoryList(JObject msg);
        
        public void UpdateBankHistory(JObject msg);
        public void DeleteBankHistory(JObject msg);

        public void SetConnectedContract(JObject msg);
        #endregion




    }
}
