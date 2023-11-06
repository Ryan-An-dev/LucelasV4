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
    public class ProductCategoryDataAgent : PageDataAgent
    {
        private IProductCategoryRepository repo = null;

        public ProductCategoryDataAgent(IProductCategoryRepository repo):base(repo)
        {
            this.repo = repo;
        }
        public void GetProductCategory(JObject msg)
        {
            this.repo.GetProductCategory(msg);
        }
        public void CreateProductCategory(JObject msg)
        {
            this.repo.CreateProductCategory(msg);
        }
        public void UpdateProductCategory(JObject msg)
        {
            this.repo.UpdateProductCategory(msg);
        }
        public void DeleteProductCategory(JObject msg)
        {
            this.repo.DeleteProductCategory(msg);
        }
    }
}
