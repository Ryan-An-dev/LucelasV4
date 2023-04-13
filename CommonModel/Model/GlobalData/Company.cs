using PrsimCommonBase;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModel.Model
{
    public class CompanyList : PrismCommonModelBase {
        public ReactiveProperty<int> Id { get; set; }
        public ReactiveProperty<string> CompanyName { get; set; }
        public ReactiveCollection<Product> ProductList { get; set; }
        public CompanyList(int categoryId, string CompanyName) : base() 
        {
            this.Id = new ReactiveProperty<int>(categoryId).AddTo(disposable);
            this.CompanyName = new ReactiveProperty<string>(CompanyName).AddTo(disposable);
            this.ProductList = new ReactiveCollection<Product>().AddTo(disposable);
        }
    }
    public class Company : PrismCommonModelBase
    {
        public ReactiveProperty<int> Id { get; set; }
        public ReactiveProperty<string> CompanyName { get; set; }
        public ReactiveProperty<string> CompanyPhone { get; set; }
        public ReactiveProperty<string> CompanyAddress { get; set; }
        public ReactiveCollection<Product> ProductList { get; set; }
        public Company(int categoryId, string CompanyName)
        {
            this.Id = new ReactiveProperty<int>(categoryId).AddTo(disposable);
            this.CompanyName = new ReactiveProperty<string>(CompanyName).AddTo(disposable);
            this.ProductList = new ReactiveCollection<Product>().AddTo(disposable);
        }
    }

    public class Product : PrismCommonModelBase
    {
        public ReactiveProperty<int> ProductId { get; set; }
        public ReactiveProperty<int> ProductPrice { get; set; }
        public ReactiveProperty<string> ProductName { get; set; }
        public ReactiveProperty<int> ProductMargin { get; set; }

        public Product(int productId, string productName, int productPrice,int productMargin)
        {
            this.ProductName = new ReactiveProperty<string>(productName).AddTo(disposable);
            this.ProductPrice = new ReactiveProperty<int>(productPrice).AddTo(disposable);
            this.ProductId= new ReactiveProperty<int>(productId).AddTo(disposable);
            this.ProductMargin= new ReactiveProperty<int>(productMargin).AddTo(disposable);
        }
        public Product Clone() {
            Product tmp = new Product(this.ProductId.Value, this.ProductName.Value, this.ProductPrice.Value, this.ProductMargin.Value);
            return tmp;
        }
    }
}
