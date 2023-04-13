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
    public class BankListDataAgent :PageDataAgent
    {
        IReceiptRepository repo = null;
        public BankListDataAgent(IReceiptRepository repo) : base(repo)
        {
            this.repo = repo;
        }

        public void GetAccountList() {
            repo.GetBankCardList();
        }
        public void GetProductCategory() {
            repo.GetProductCategoryList();
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
        public void GetConnectedContract(JObject msg) {
            this.repo.GetConnectedContract(msg);
        }
    }
}
