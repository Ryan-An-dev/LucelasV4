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
using PrsimCommonBase;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SettingPage.ViewModels
{
    public class ProductListViewModel : PrsimListViewModelBase, INetReceiver
    {
        public ReactiveCollection<FurnitureType> FurnitureInfos { get; set; }
        public ProductListViewModel(IContainerProvider containerprovider, IRegionManager regionManager, IDialogService dialogService) : base(regionManager, containerprovider, dialogService)
        {
            FurnitureInfos = new ReactiveCollection<FurnitureType>().AddTo(disposable);
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
                jobj["page_unit"] = (ListCount.Value * CurrentPage.Value) > TotalItemCount.Value ? TotalItemCount.Value - (ListCount.Value * (CurrentPage.Value - 1)) : ListCount.Value;
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
            JObject jobject = null;
            try { jobject = new JObject(JObject.Parse(msg)); }
            catch (Exception)
            {
                return;
            }
            ErpLogWriter.LogWriter.Trace(jobject.ToString());
            SettingPageViewModel temp = this.ContainerProvider.Resolve<SettingPageViewModel>("GlobalData");
            FurnitureInfos = temp.FurnitureInfos;
            switch ((COMMAND)packet.Header.CMD)
            {
                case COMMAND.GETPRODUCTINFO: //데이터 조회
                    List.Clear();
                    if (jobject.ToString().Trim() != string.Empty)
                    {
                        try
                        {
                            if (jobject["product_list"] == null)
                                return;
                            JArray jarr = new JArray();
                            jarr = jobject["product_list"] as JArray;
                            foreach (JObject jobj in jarr)
                            {
                                FurnitureInventory inventory = new FurnitureInventory();
                                if (jobj["company"] != null)
                                    inventory.Company.Value = SetCompanyInfo(jobj["company"] as JObject);
                                if (jobj["count"] != null)
                                    inventory.Count.Value = jobj["count"].ToObject<int>();
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
            dialogParameters.Add("object", new FurnitureInventory());

            dialogService.ShowDialog("ProductAddPage", dialogParameters, r =>
            {
                try
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        FurnitureInventory item = r.Parameters.GetValue<FurnitureInventory>("object");
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
                jobj["aci_id"] = (int)(selecteditem as FurnitureInventory).Company.Value.Id.Value;
                jobj["acpi_id"] = (int)(selecteditem as FurnitureInventory).Id.Value;
                network.Delete(jobj);
                IsLoading.Value = true;
            }
        }

        public override void RowDoubleClickEvent()
        {
            DialogParameters dialogParameters = new DialogParameters();
            SelectedItem.Value.ClearJson();
            dialogParameters.Add("object", SelectedItem.Value as FurnitureInventory);

            dialogService.ShowDialog("ProductAddPage", dialogParameters, r =>
            {
                try
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        FurnitureInventory item = r.Parameters.GetValue<FurnitureInventory>("object");
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
    }
}
