using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModel.Model
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum BankType {
        [Description("계좌")]
        Account =1,
        [Description("카드")]
        Card,
    }
    public class BankModel
    {
        public ReactiveProperty<BankType> Type { get; set; } //타입
        public ReactiveProperty<string> Name { get; set; } //사용자 지정 이름
        public ReactiveProperty<int> AccountSerial { get; set; } //DB 번호
        public ReactiveProperty<string> AccountNum { get; set; } //계좌 or 카드 번호
        public ReactiveProperty<bool> IsChecked { get; set; }//선택됬는지 안됫는지
        public BankModel()
        {
            this.IsChecked = new ReactiveProperty<bool>();
            this.Type = new ReactiveProperty<BankType>();
            this.Name= new ReactiveProperty<string>();
            this.AccountNum =  new ReactiveProperty<string>();
            this.AccountSerial = new ReactiveProperty<int>();
        }
        public BankModel(BankType type, string name, int AccountSerial, string AccountNum)
        {
            this.IsChecked.Value = false;
            this.Type.Value = type;
            this.Name.Value = name;
            this.AccountSerial.Value = AccountSerial;
            this.AccountNum.Value = AccountNum;
        }
    }
}
