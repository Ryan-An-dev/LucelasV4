using CommonModel.Model;
using DataAccess;
using DataAccess.NetWork;
using DataAgent;
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
using System.Windows.Interop;

namespace SettingPage.ViewModels
{
    public class ProductCategoryListViewModel : PrsimListViewModelBase, INetReceiver
    {
        public ProductCategoryDataAgent network { get; set; }
        ReactiveProperty<int> Count { get; set; }
        public ProductCategoryListViewModel(IContainerProvider containerprovider, IRegionManager regionManager, IDialogService dialogService) : base(regionManager, containerprovider, dialogService)
        {
            
        }
        public void initData()
        {
            try
            {
                using (this.network = ContainerProvider.Resolve<DataAgent.ProductCategoryDataAgent>())
                {
                    network.SetReceiver(this);
                    JObject jobj = new JObject();
                    jobj["next_preview"] = 0;
                    jobj["page_unit"] = (ListCount.Value * CurrentPage.Value) > TotalItemCount.Value ? TotalItemCount.Value - (ListCount.Value * (CurrentPage.Value - 1)) : ListCount.Value;
                    jobj["page_start_pos"] = (CurrentPage.Value - 1) * ListCount.Value;
                    network.GetProductCategory(jobj);
                    IsLoading.Value = true;
                }
            }
            catch (Exception ex) { }

        }

        public override void UpdatePageItem(MovePageType param, int count)
        {
            using (var network = ContainerProvider.Resolve<DataAgent.ProductCategoryDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                jobj["next_preview"] = (int)param;
                jobj["page_unit"] = (ListCount.Value);
                jobj["page_start_pos"] = (CurrentPage.Value - 1) * ListCount.Value;
                network.GetProductCategory(jobj);
                IsLoading.Value = true;
            }
        }
        public void OnConnected()
        {

        }

        public void OnRceivedData(ErpPacket packet)
        {
            if (packet.Header.CMD != (ushort)COMMAND.ProductCategoryList)
            {
                return;
            }
            string msg = Encoding.UTF8.GetString(packet.Body);
            if (!msg.Contains("null"))
            {
                JObject body = new JObject(JObject.Parse(msg));
                ErpLogWriter.LogWriter.Trace(body.ToString());
                Application.Current.Dispatcher.BeginInvoke(() => { List.Clear(); });
                if (body.ToString().Trim() != string.Empty)
                {
                    try
                    {
                        if (body["category_list"] == null)
                            return;
                        JArray jarr = new JArray();
                        jarr = body["category_list"] as JArray;
                        if (body["history_count"] != null)
                            TotalItemCount.Value = body["history_count"].ToObject<int>();
                        int i = CurrentPage.Value == 1 ? 1 : ListCount.Value * (CurrentPage.Value - 1) + 1;
                        foreach (JObject jobj in jarr)
                        {
                            FurnitureType temp = new FurnitureType();
                            if (jobj["product_type_id"] != null)
                                temp.Id.Value = jobj["product_type_id"].ToObject<int>();
                            if (jobj["product_type_name"] != null)
                                temp.Name.Value = jobj["product_type_name"].ToString();
                            temp.No.Value = i++;
                            this.List.Add(temp);
                        }
                        IsLoading.Value = false;
                    }
                    catch (Exception) { IsLoading.Value = false; }
                }
            }
        }

        public void OnSent()
        {

        }

        public override void AddButtonClick()
        {
            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("object",new FurnitureType());

            dialogService.ShowDialog("ProductCategoryAddPage", dialogParameters, r =>
            {
                try
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        FurnitureType item = r.Parameters.GetValue<FurnitureType>("object");
                        if (item != null)
                        {
                            using (var network = ContainerProvider.Resolve<DataAgent.ProductCategoryDataAgent>())
                            {
                                network.SetReceiver(this);
                                JObject jobj = new JObject();
                                jobj["pti_enum_id"] = (int)0;
                                jobj["pti_name"] = item.Name.Value;
                                network.CreateProductCategory(jobj);
                                IsLoading.Value = true;
                            }
                        }                        
                    }
                }
                catch (Exception) { }

            }, "CommonDialogWindow");
        }

        public override void DeleteButtonClick(PrismCommonModelBase selectedItem)
        {
            using (var network = ContainerProvider.Resolve<DataAgent.ProductCategoryDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                jobj["pti_enum_id"] = (int)(selectedItem as FurnitureType).Id.Value;
                network.DeleteProductCategory(jobj);
                IsLoading.Value = true;
            }
        }

        public override void RowDoubleClickEvent()
        {
            DialogParameters dialogParameters = new DialogParameters();
            SelectedItem.Value.ClearJson();
            dialogParameters.Add("object", SelectedItem.Value as FurnitureType);
            dialogService.ShowDialog("ProductCategoryAddPage", dialogParameters, r =>
            {
                try
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        FurnitureType item = r.Parameters.GetValue<FurnitureType>("object");
                        if (item != null)
                        {
                            if (item.isChanged) {
                                using (var network = ContainerProvider.Resolve<DataAgent.ProductCategoryDataAgent>())
                                {
                                    network.SetReceiver(this);
                                    JObject jobj = new JObject();
                                    jobj["changed_item"] = item.ChangedItem;
                                    jobj["product_type_id"] = item.Id.Value;
                                    network.UpdateProductCategory(jobj);
                                }
                            }
                        }
                    }
                }
                catch (Exception) { }

            }, "CommonDialogWindow");
        }

        public override void SearchTitle(string Keyword)
        {
            using (var network = ContainerProvider.Resolve<DataAgent.ProductCategoryDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                JObject search = new JObject();
                search["product_type_name"] = Keyword;
                jobj["page_unit"] = (ListCount.Value);
                jobj["page_start_pos"] = (CurrentPage.Value - 1) * ListCount.Value;
                jobj["search_option"] = search;
                network.GetProductCategory(jobj);
            }
        }
    }
}
