using CommonModel.Model;
using DataAccess.Interface;
using DataAccess.NetWork;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class HomePageRepository : IHomePageRepository 
    {
        public HomePageRepository(IContainerProvider containerProvider)
        {

        }

        public bool DeleteItemData(List<HomeSummaryModel> item)
        {

            return true;
        }

        public List<HomeSummaryModel> GetPageData()
        {
            return null;
        }

        public bool InsertItem(List<HomeSummaryModel> item)
        {
            return true;
        }

        public void SetReceiver(INetReceiver netReceiver)
        {
            throw new NotImplementedException();
        }

        public bool UpdateItem(List<HomeSummaryModel> item)
        {
            return true;
        }
    }
}
