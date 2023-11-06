using DataAccess.NetWork;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interface
{
    public interface IProductCategoryRepository : IBaseRepository
    {
        #region LIST CRUD
        public void CreateProductCategory(JObject msg);
        public void GetProductCategory(JObject msg);

        public void UpdateProductCategory(JObject msg);
        public void DeleteProductCategory(JObject msg);
        #endregion

    }
}
