using CommonModel.Model;
using DataAccess.Repository;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interface
{
    public interface IHomePageRepository : IBaseRepository
    {
        public void GetHomePage(JObject msg);
    }
}
