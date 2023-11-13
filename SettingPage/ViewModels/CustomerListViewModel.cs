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
    

    public class CustomerListViewModel : PrsimListViewModelBase, INetReceiver
    {
        public CustomerListViewModel(IContainerProvider containerprovider, IRegionManager regionManager, IDialogService dialogService) : base(regionManager, containerprovider, dialogService)
        {
          
        }

        public override void UpdatePageItem(MovePageType param, int count)
        {
            using (var network = ContainerProvider.Resolve<DataAgent.CustomerDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                jobj["next_preview"] = (int)param;
                jobj["page_unit"] = (ListCount.Value * CurrentPage.Value) > TotalItemCount.Value ? TotalItemCount.Value - (ListCount.Value * (CurrentPage.Value - 1)) : ListCount.Value;
                jobj["page_start_pos"] = (CurrentPage.Value - 1) * ListCount.Value;
                network.GetCustomerList(jobj);
            }
        }
        public void OnConnected()
        {

        }

        public void OnRceivedData(ErpPacket packet)
        {
            if (packet.Header.CMD != (ushort)COMMAND.GETCUSTOMERINFO)
            {
                return;
            }
            string msg = Encoding.UTF8.GetString(packet.Body);
            ErpLogWriter.LogWriter.Debug(msg);
            JObject jobject = new JObject(JObject.Parse(msg));
            if (!msg.Equals("null\n"))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    List.Clear();
                });
                if (jobject["customer_list"] == null)
                    return;
                JArray jarr = new JArray();
                jarr = jobject["customer_list"] as JArray;
                if (jobject["history_count"] != null)
                    TotalItemCount.Value = jobject["history_count"].ToObject<int>();
                int i = CurrentPage.Value == 1 ? 1 : ListCount.Value * (CurrentPage.Value - 1) + 1;
                foreach (JObject inner in jobject["customer_list"] as JArray)
                {
                    Customer temp = new Customer();
                    temp.No.Value = i++;
                    if (inner["cui_id"] != null)
                        temp.Id.Value = inner["cui_id"].ToObject<int>();
                    if (inner["cui_name"] != null)
                        temp.Name.Value = inner["cui_name"].ToString().Trim();
                    if (inner["cui_phone_num"] != null)
                        temp.Phone.Value = inner["cui_phone_num"].ToString().Trim();
                    if (inner["cui_address"] != null)
                        temp.Address.Value = inner["cui_address"].ToString().Trim();
                    if (inner["cui_address_detail"] != null)
                        temp.Address1.Value = inner["cui_address_detail"].ToString().Trim();
                    if (inner["cui_memo"] != null)
                        temp.Memo.Value = inner["cui_memo"].ToString().Trim();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                       List.Add(temp);
                    });
                }
            }
        }

        public void OnSent()
        {

        }

        public override void AddButtonClick()
        {
            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("object", new Customer());

            dialogService.ShowDialog("CustomerAddPage", dialogParameters, r =>
            {
                try
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        Customer item = r.Parameters.GetValue<Customer>("object");
                        if (item != null)
                        {
                            using (var network = ContainerProvider.Resolve<DataAgent.CustomerDataAgent>())
                            {
                                network.SetReceiver(this);
                                JObject jobj = new JObject();
                                jobj["cui_id"] = (int)0;
                                jobj["cui_name"] = item.Name.Value;
                                jobj["cui_phone_num"] = item.Phone.Value;
                                jobj["cui_address"] = item.Address.Value;
                                jobj["cui_address_detail"] = item.Address1.Value;
                                jobj["cui_memo"] = item.Memo.Value;
                                network.CreateCustomerList(jobj);
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
            using (var network = ContainerProvider.Resolve<DataAgent.CustomerDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                jobj["cui_id"] = (int)(selecteditem as Customer).Id.Value;
                network.DeleteCustomerList(jobj);
                IsLoading.Value = true;
            }
        }
        public void SendBasicData(INetReceiver receiver)
        {
            using (var network = ContainerProvider.Resolve<DataAgent.CustomerDataAgent>())
            {
                network.SetReceiver(receiver);
                JObject jobj = new JObject();
                jobj["next_preview"] = (int)0;
                jobj["page_unit"] = (ListCount.Value);
                jobj["page_start_pos"] = (CurrentPage.Value - 1) * ListCount.Value;
                network.GetCustomerList(jobj);
            }
        }

        public override void RowDoubleClickEvent()
        {
            DialogParameters dialogParameters = new DialogParameters();
            SelectedItem.Value.ClearJson();
            dialogParameters.Add("object", SelectedItem.Value as Customer);

            dialogService.ShowDialog("CustomerAddPage", dialogParameters, r =>
            {
                try
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        Customer item = r.Parameters.GetValue<Customer>("object");
                        if (item != null)
                        {
                            using (var network = ContainerProvider.Resolve<DataAgent.ProductDataAgent>())
                            {
                                network.SetReceiver(this);
                                JObject jobj = new JObject();
                                jobj["changed_item"] = item.ChangedItem;
                                jobj["cui_id"] = item.Id.Value;
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
            using (var network = ContainerProvider.Resolve<DataAgent.CustomerDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                JObject search = new JObject();
                search["customer_name"] = Keyword;
                jobj["page_unit"] = (ListCount.Value);
                jobj["page_start_pos"] = (CurrentPage.Value - 1) * ListCount.Value;
                jobj["search_option"] = search;
                network.GetCustomerList(jobj);
            }
        }
    }
}
