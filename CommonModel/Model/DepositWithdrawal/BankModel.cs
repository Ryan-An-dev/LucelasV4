
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
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
    public class BankModel : PrismCommonModelBase
    {
        public IEnumerable<BankType> SearchBankTypeValues
        {
            get { return Enum.GetValues(typeof(BankType)).Cast<BankType>(); }
        }
        public ReactiveProperty<int> No { get; set; } //순서
        public ReactiveProperty<BankType> Type { get; set; } //타입
        public ReactiveProperty<string> Name { get; set; } //사용자 지정 이름
        public ReactiveProperty<int> AccountSerial { get; set; } //DB 번호
        public ReactiveProperty<string> AccountNum { get; set; } //계좌 or 카드 번호
        public ReactiveProperty<DateTime?> LastUpdate { get; set; }
        public ReactiveProperty<bool> IsChecked { get; set; }//선택됬는지 안됫는지
        public BankModel()
        {
            this.No = new ReactiveProperty<int>().AddTo(disposable);
            this.IsChecked = new ReactiveProperty<bool>().AddTo(disposable);
            this.Type = new ReactiveProperty<BankType>().AddTo(disposable); 
            this.Name= CreateProperty<string>("이름");
            this.AccountNum = CreateProperty<string>("계좌번호");
            this.AccountSerial = new ReactiveProperty<int>().AddTo(disposable);
            this.LastUpdate = new ReactiveProperty<DateTime?>().AddTo(disposable);
            SetObserver();
        }
        public BankModel(BankType type, string name, int AccountSerial, string AccountNum)
        {
            this.IsChecked.Value = false;
            this.Type.Value = type;
            this.Name.Value = name;
            this.AccountSerial.Value = AccountSerial;
            this.AccountNum.Value = AccountNum;
        }

        public override void SetObserver()
        {
            Type.Subscribe(x => ChangedJson("account_type", x));
            Name.Subscribe(x => ChangedJson("account_name", x));
            AccountNum.Subscribe(x => ChangedJson("account_num", x));
        }
    }
}
