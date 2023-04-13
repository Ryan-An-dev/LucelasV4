using CommonModel.Model;
using DataAccess;
using DataAccess.Interface;
using DataAccess.NetWork;
using DataAgent.BaseAgent;
using Newtonsoft.Json.Linq;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAgent
{
    public class SettingDataAgent : PageDataAgent
    {
        ISettingRepository repo = null;
        public SettingDataAgent(ISettingRepository repo) : base(repo)
        {
            this.repo = repo;
        }

        public void GetAccountList() {
            this.repo.GetBankCardList();
        }
        public void GetProductCategory() {
            this.repo.GetProductCategoryList();
        }
        public void GetCustomerList() {
            this.repo.GetCustomerList();
        }
        public void GetCategory() {
            this.repo.GetReceiptCategoryList();
        }
        public void GetBankHistory(JObject msg) {
            this.repo.GetBankHistoryList(msg);
        }
        public void CreateBankHistory(JObject msg) {
            this.repo.CreateBankHistory(msg);
        }
        public void UpdateBankHistory(JObject msg) { 
            this.repo.UpdateBankHistory(msg);
        }
        public void DeleteBankHistory(JObject msg) {
            this.repo.DeleteBankHistory(msg);
        }
    }
}
