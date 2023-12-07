using CommonModel;
using CommonModel.Model;
using DataAccess;
using DataAccess.NetWork;
using LogWriter;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using CommonModel;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace SettingPage.ViewModels
{
    public class ProductListViewModel : PrsimListViewModelBase, INetReceiver
    {
        public ReactiveProperty<CompanyProductSelect> CompanyProductTypeSelect { get; set; } //검색옵션
        public IEnumerable<CompanyProductSelect> CompanyProductTypeSelectValues //검색옵션
        {
            get { return Enum.GetValues(typeof(CompanyProductSelect)).Cast<CompanyProductSelect>(); }
        }
        public ReactiveCommand List_MouseDoubleClick { get; set; }

        [TypeConverter(typeof(EnumDescriptionTypeConverter))]
        public enum CompanyProductSelect
        {
            [Description("제품명")]
            ProductName = 0,
            [Description("회사명")]
            CompanyName = 1,
        }
        public ReactiveCollection<FurnitureType> FurnitureInfos { get; set; }
        public ProductListViewModel(IContainerProvider containerprovider, IRegionManager regionManager, IDialogService dialogService) : base(regionManager, containerprovider, dialogService)
        {
            List_MouseDoubleClick = new ReactiveCommand().WithSubscribe(()=> RowDoubleClickEvent()).AddTo(disposable);
            FurnitureInfos = new ReactiveCollection<FurnitureType>().AddTo(disposable);
            CompanyProductTypeSelect = new ReactiveProperty<CompanyProductSelect>(CompanyProductSelect.ProductName).AddTo(disposable);
        }

        public override void UpdatePageItem(MovePageType param, int count)
        {
            using (var network = ContainerProvider.Resolve<DataAgent.ProductDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                jobj["next_preview"] = (int)param;
                jobj["page_unit"] = (ListCount.Value * CurrentPage.Value) > TotalItemCount.Value ? TotalItemCount.Value - (ListCount.Value * (CurrentPage.Value - 1)) : ListCount.Value;
                jobj["page_start_pos"] = (CurrentPage.Value - 1) * ListCount.Value;
                network.repo.Read(jobj);
            }
        }
        public void SendBasicData(INetReceiver receiver)
        {
            using (var network = ContainerProvider.Resolve<DataAgent.ProductDataAgent>())
            {
                network.SetReceiver(receiver);
                JObject jobj = new JObject();
                jobj["next_preview"] = (int)0;
                jobj["page_unit"] = ListCount.Value;
                jobj["page_start_pos"] = (CurrentPage.Value - 1) * ListCount.Value;
                network.repo.Read(jobj);
            }
        }
        public void OnConnected()
        {

        }
        private Company SetCompanyInfo(JObject jobj)
        {
            Company temp = new Company();
            if (jobj["company_name"] != null)
                temp.CompanyName.Value = jobj["company_name"].ToString();
            if (jobj["company_phone"] != null)
                temp.CompanyPhone.Value = jobj["company_phone"].ToString();
            if (jobj["company_id"] != null)
                temp.Id.Value = jobj["company_id"].ToObject<int>();
            if (jobj["company_address"] != null)
                temp.CompanyAddress.Value = jobj["company_address"].ToString();
            return temp;

        }

        public void OnRceivedData(ErpPacket packet)
        {
            string msg = Encoding.UTF8.GetString(packet.Body);
            ErpLogWriter.LogWriter.Debug(msg);
            if (packet.Header.CMD < (ushort)COMMAND.CREATEPRODUCTINFO
                || packet.Header.CMD > (ushort)COMMAND.DELETEPRODUCTINFO)
            {
                return;
            }
            switch (packet.Header.CMD)
            {
                case (ushort)COMMAND.CREATEPRODUCTINFO:
                case (ushort)COMMAND.DELETEPRODUCTINFO:
                case (ushort)COMMAND.UPDATEPRODUCTINFO:
                    SearchTitle(this.Keyword.Value);
                    break;
                case (ushort)COMMAND.GETPRODUCTINFO:
                    JObject jobject = null;
                    try { jobject = new JObject(JObject.Parse(msg)); }
                    catch (Exception)
                    {
                        return;
                    }
                    ErpLogWriter.LogWriter.Trace(jobject.ToString());
                    SettingPageViewModel temp = this.ContainerProvider.Resolve<SettingPageViewModel>("GlobalData");
                    FurnitureInfos = temp.FurnitureInfos;
                    Application.Current.Dispatcher.BeginInvoke(() => { List.Clear(); });
                    if (jobject.ToString().Trim() != string.Empty)
                    {
                        try
                        {
                            if (jobject["product_list"] == null)
                                return;
                            JArray jarr = new JArray();
                            jarr = jobject["product_list"] as JArray;
                            if (jobject["history_count"] != null)
                                TotalItemCount.Value = jobject["history_count"].ToObject<int>();

                            int i = CurrentPage.Value == 1 ? 1 : ListCount.Value * (CurrentPage.Value - 1) + 1;

                            foreach (JObject jobj in jarr)
                            {
                                Product inventory = new Product();
                                inventory.No.Value = i++;
                                if (jobj["company"] != null)
                                    inventory.Company.Value = SetCompanyInfo(jobj["company"] as JObject);
                                if (jobj["product_type"] != null)
                                    inventory.ProductType.Value = FurnitureInfos.FirstOrDefault(x => x.Id.Value == jobj["product_type"].ToObject<int>());
                                if (jobj["product_name"] != null)
                                    inventory.Name.Value = jobj["product_name"].ToString();
                                if (jobj["product_price"] != null)
                                    inventory.Price.Value = jobj["product_price"].ToObject<int>();
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    this.List.Add(inventory);
                                });
                            }
                        }
                        catch (Exception e) { LogWriter.ErpLogWriter.LogWriter.Debug(e.ToString()); }
                    }
                    break;
            }
        }

        public void OnSent()
        {

        }

        public override void AddButtonClick()
        {
            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("object", new Product());

            dialogService.ShowDialog("ProductAddPage", dialogParameters, r =>
            {
                try
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        Product item = r.Parameters.GetValue<Product>("object");
                        if (item != null)
                        {
                            using (var network = ContainerProvider.Resolve<DataAgent.ProductDataAgent>())
                            {
                                network.SetReceiver(this);
                                JObject jobj = new JObject();
                                jobj["product_id"] = (int)0;
                                jobj["product_type"] = (int)item.ProductType.Value.Id.Value;
                                jobj["product_name"] = item.Name.Value;
                                jobj["product_price"] = item.Price.Value;
                                jobj["company_id"] = item.Company.Value.Id.Value;
                                network.Create(jobj);
                                IsLoading.Value = true;
                            }
                        }
                    }
                }
                catch (Exception) { }

            }, "CommonDialogWindow");
        }

        public override void DeleteButtonClick(PrismCommonModelBase selecteditem)
        {
            using (var network = ContainerProvider.Resolve<DataAgent.ProductDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                jobj["aci_id"] = (int)(selecteditem as Product).Company.Value.Id.Value;
                jobj["acpi_id"] = (int)(selecteditem as Product).Id.Value;
                network.Delete(jobj);
                IsLoading.Value = true;
            }
        }

        public override void RowDoubleClickEvent()
        {
            if (SelectedItem.Value == null)
                return;
            DialogParameters dialogParameters = new DialogParameters();
            SelectedItem.Value.ClearJson();
            dialogParameters.Add("object", SelectedItem.Value as Product);

            dialogService.ShowDialog("ProductAddPage", dialogParameters, r =>
            {
                try
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        Product item = r.Parameters.GetValue<Product>("object");
                        if (item != null)
                        {
                            using (var network = ContainerProvider.Resolve<DataAgent.ProductDataAgent>())
                            {
                                network.SetReceiver(this);
                                JObject jobj = new JObject();
                                jobj["changed_item"] = item.ChangedItem;
                                jobj["product_id"] = item.Id.Value;
                                network.Update(jobj);
                            }
                        }
                    }
                }
                catch (Exception) { }

            }, "CommonDialogWindow");
        }

        public override void SearchTitle(string Keyword)
        {
            using (var network = ContainerProvider.Resolve<DataAgent.ProductDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                JObject search = new JObject();
                if (this.CompanyProductTypeSelect.Value == CompanyProductSelect.ProductName)
                {
                    search["product_name"] = Keyword;
                }
                else {
                    search["company_name"] = Keyword;
                }
                jobj["page_unit"] = (ListCount.Value);
                jobj["page_start_pos"] = (CurrentPage.Value - 1) * ListCount.Value;
                jobj["search_option"] = search;
                network.Get(jobj);
            }
        }
    }
}
