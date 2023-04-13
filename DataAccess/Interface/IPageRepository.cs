
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interface
{
    public interface IPageRepository <T>
    {
        List<T> GetPageData();
        
        bool DeleteItemData(List<T> item);

        bool UpdateItem(List<T> item);

        bool InsertItem(List<T> item);

    }
}
