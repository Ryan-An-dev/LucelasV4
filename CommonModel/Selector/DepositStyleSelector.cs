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
    public class DepositStyleSelector: StyleSelector
    {
        public Style CostDataStyle { get; set; }
        public Style IncomeDataStyle { get; set; }
        /// <summary>
        /// 스타일 선택 로직 실행 메소드
        /// </summary>
        /// <param name="item"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public override Style SelectStyle(object item, DependencyObject container)
        {
            //item을 MessageModel인지 확인합니다.
            if (item is ReceiptModel message)
            {
                //MessageType에 따라서 Style을 반환합니다.
                switch (message.IncomeCostType.Value)
                {
                    case IncomeCostType.Cost:
                        return CostDataStyle;
                    case IncomeCostType.Income:
                        return IncomeDataStyle;
                }
            }
            return base.SelectStyle(item, container);
        }
    }
}
