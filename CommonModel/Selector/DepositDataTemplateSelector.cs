using CommonModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CommonModel
{
    public class DepositDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate InComeTemplate { get; set; }
        public DataTemplate CostTemplate { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            //item을 MessageModel인지 확인합니다.
            if (item is ReceiptModel message)
            {
                //MessageType에 따라서 DataTemplate를 각각 반환합니다.
                switch (message.IncomeCostType.Value)
                {
                    case IncomeCostType.Income:
                        return InComeTemplate;
                    case IncomeCostType.Cost:
                        return CostTemplate;
                }
            }
            return base.SelectTemplate(item, container);
        }
    }
}
