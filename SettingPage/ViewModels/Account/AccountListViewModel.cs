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
    public class AccountListViewModel : PrsimListViewModelBase, INetReceiver
    {
        public AccountListViewModel(IContainerProvider containerprovider, IRegionManager regionManager, IDialogService dialogService) : base(regionManager, containerprovider, dialogService)
        {

        }

        public override void UpdatePageItem(MovePageType param, int count)
        {
            using (var network = ContainerProvider.Resolve<DataAgent.AccountDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                jobj["page_unit"] = (ListCount.Value * CurrentPage.Value) > TotalItemCount.Value ? TotalItemCount.Value - (ListCount.Value * (CurrentPage.Value - 1)) : ListCount.Value;
                jobj["page_start_pos"] = (CurrentPage.Value - 1) * ListCount.Value;
                network.GetAccountList(jobj);
            }
        }
        public void OnConnected()
        {

        }

        public void OnRceivedData(ErpPacket packet)
        {
            string msg = Encoding.UTF8.GetString(packet.Body);
            ErpLogWriter.LogWriter.Debug(msg);
            if (packet.Header.CMD < (ushort)COMMAND.CREATE_ACCOUNT_INFO
                || packet.Header.CMD > (ushort)COMMAND.DELETE_ACCOUNT_INFO
                || packet.Header.CMD == (ushort)COMMAND.AccountLIst)
            {
                return;
            }
            switch (packet.Header.CMD)
            {
                case (ushort)COMMAND.CREATE_ACCOUNT_INFO:
                case (ushort)COMMAND.DELETE_ACCOUNT_INFO:
                case (ushort)COMMAND.UPDATE_ACCOUNT_INFO:
                    SearchTitle(this.Keyword.Value);
                    break;
                case (ushort)COMMAND.AccountLIst:
                    JObject jobject = null;
                    try { jobject = new JObject(JObject.Parse(msg)); }
                    catch (Exception)
                    {
                        return;
                    }
                    Application.Current.Dispatcher.Invoke(() => { List.Clear(); });
                    if (jobject.ToString().Trim() != string.Empty)
                    {
                        try
                        {
                            if (jobject["user_account"] == null)
                                return;
                            JArray jarr = new JArray();
                            jarr = jobject["user_account"] as JArray;
                            if (jobject["history_count"] != null)
                                TotalItemCount.Value = jobject["history_count"].ToObject<int>();
                            int i = CurrentPage.Value == 1 ? 1 : ListCount.Value * (CurrentPage.Value - 1) + 1;
                            foreach (JObject jobj in jarr)
                            {
                                BankModel temp = new BankModel();
                                temp.No.Value = i++;
                                if (jobj["account_serial"] != null)
                                    temp.AccountSerial.Value = jobj["account_serial"].ToObject<int>();
                                if (jobj["account_type"] != null)
                                    temp.Type.Value = jobj["account_type"].ToObject<BankType>();
                                if (jobj["account_name"] != null)
                                    temp.Name.Value = jobj["account_name"].ToString();
                                if (jobj["account_num"] != null)
                                    temp.AccountNum.Value = jobj["account_num"].ToString();
                                try
                                {
                                    if (jobj["last_update"] != null)
                                        temp.LastUpdate.Value = jobj["last_update"].ToObject<DateTime>();
                                }
                                catch (Exception e)
                                {
                                    temp.LastUpdate.Value = null;
                                }
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    List.Add(temp);
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
            dialogParameters.Add("object", new BankModel());

            dialogService.ShowDialog("AccountAddPage", dialogParameters, r =>
            {
                try
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        BankModel item = r.Parameters.GetValue<BankModel>("object");
                        if (item != null)
                        {
                            using (var network = ContainerProvider.Resolve<DataAgent.AccountDataAgent>())
                            {
                                network.SetReceiver(this);
                                JObject jobj = new JObject();
                                jobj["account_serial"] = (int)0;
                                jobj["account_type"] = (int)item.Type.Value;
                                jobj["account_name"] = item.Name.Value;
                                jobj["account_num"] = item.AccountNum.Value;
                                network.CreateAccountList(jobj);
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
            using (var network = ContainerProvider.Resolve<DataAgent.AccountDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                jobj["account_serial"] = (int)(selecteditem as BankModel).AccountSerial.Value;
                network.DeleteAccountList(jobj);
                IsLoading.Value = true;
            }
        }

        public override void RowDoubleClickEvent()
        {
            DialogParameters dialogParameters = new DialogParameters();
            SelectedItem.Value.ClearJson();
            dialogParameters.Add("object", SelectedItem.Value as BankModel);

            dialogService.ShowDialog("AccountAddPage", dialogParameters, r =>
            {
                try
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        BankModel item = r.Parameters.GetValue<BankModel>("object");
                        if (item != null)
                        {
                            using (var network = ContainerProvider.Resolve<DataAgent.AccountDataAgent>())
                            {
                                network.SetReceiver(this);
                                JObject jobj = new JObject();
                                jobj["changed_item"] = item.ChangedItem;
                                jobj["account_serial"] = item.AccountSerial.Value;
                                network.UpdateAccountList(jobj);
                            }
                        }
                    }
                }
                catch (Exception) { }

            }, "CommonDialogWindow");
        }

        internal void SendBasicData(SettingPageViewModel settingPageViewModel)
        {
            using (var network = ContainerProvider.Resolve<DataAgent.AccountDataAgent>())
            {
                network.SetReceiver(settingPageViewModel);
                JObject jobj = new JObject();
                jobj["next_preview"] = (int)0;
                jobj["page_unit"] = (ListCount.Value);
                jobj["page_start_pos"] = (CurrentPage.Value - 1) * ListCount.Value;
                network.GetAccountList(jobj);
            }
        }

        public override void SearchTitle(string Keyword)
        {
            using (var network = ContainerProvider.Resolve<DataAgent.AccountDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                JObject search = new JObject();
                search["account_name"] = Keyword;
                jobj["page_unit"] = (ListCount.Value);
                jobj["page_start_pos"] = (CurrentPage.Value - 1) * ListCount.Value;
                jobj["search_option"] = search;
                network.GetAccountList(jobj);
            }
        }
    }
}
