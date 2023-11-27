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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace SettingPage.ViewModels
{
    public class ApiListViewModel : PrsimListViewModelBase, INetReceiver
    {
        public ApiListViewModel(IContainerProvider containerprovider, IRegionManager regionManager, IDialogService dialogService) : base(regionManager, containerprovider, dialogService)
        {

        }

        public override void UpdatePageItem(MovePageType param, int count)
        {
            using (var network = ContainerProvider.Resolve<DataAgent.ApiDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                jobj["page_unit"] = (ListCount.Value * CurrentPage.Value) > TotalItemCount.Value ? TotalItemCount.Value - (ListCount.Value * (CurrentPage.Value - 1)) : ListCount.Value;
                jobj["page_start_pos"] = (CurrentPage.Value - 1) * ListCount.Value;
                network.Get(jobj);
            }
        }
        public void OnConnected()
        {

        }

        public void OnRceivedData(ErpPacket packet)
        {
            string msg = Encoding.UTF8.GetString(packet.Body);
            ErpLogWriter.LogWriter.Debug(msg);
            if (packet.Header.CMD < (ushort)COMMAND.CREATE_API_INFO
                || packet.Header.CMD > (ushort)COMMAND.DELETE_API_INFO)
            {
                return;
            }
            switch (packet.Header.CMD)
            {
                case (ushort)COMMAND.CREATE_API_INFO:
                case (ushort)COMMAND.DELETE_API_INFO:
                case (ushort)COMMAND.UPDATE_API_INFO:
                    SearchTitle(this.Keyword.Value);
                    break;
                case (ushort)COMMAND.READ_API_INFO:
                    JObject jobject = null;
                    try { jobject = new JObject(JObject.Parse(msg)); }
                    catch (Exception)
                    {
                        return;
                    }
                    Application.Current.Dispatcher.BeginInvoke(() => { List.Clear(); });
                    if (jobject.ToString().Trim() != string.Empty)
                    {
                        try
                        {
                            if (jobject["api_list"] == null)
                                return;
                            JArray jarr = new JArray();
                            jarr = jobject["api_list"] as JArray;
                            if (jobject["history_count"] != null)
                                TotalItemCount.Value = jobject["history_count"].ToObject<int>();
                            int i = CurrentPage.Value == 1 ? 1 : ListCount.Value * (CurrentPage.Value - 1) + 1;
                            foreach (JObject jobj in jarr)
                            {
                                API temp = new API();
                                temp.No.Value = i++;
                                if (jobj["api_id"] != null)
                                    temp.Id.Value = jobj["api_id"].ToObject<int>();
                                if (jobj["api_type"] != null)
                                    temp.Type.Value = jobj["api_type"].ToObject<APIType>();
                                if (jobj["api_key"] != null)
                                    temp.ApiKey.Value = jobj["api_key"].ToString();
                                if (jobj["api_account"] != null)
                                    temp.ApiID.Value = jobj["api_account"].ToString();
                                if (jobj["api_cert_num"] != null)
                                    temp.CertNum.Value = jobj["api_cert_num"].ToString();
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
            dialogParameters.Add("object", new API());

            dialogService.ShowDialog("ApiAddPage", dialogParameters, r =>
            {
                try
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        API item = r.Parameters.GetValue<API>("object");
                        if (item != null)
                        {
                            using (var network = ContainerProvider.Resolve<DataAgent.ApiDataAgent>())
                            {
                                network.SetReceiver(this);
                                JObject jobj = new JObject();
                                jobj["api_id"] = (int)0;
                                jobj["api_type"] = (int)item.Type.Value;
                                jobj["api_key"] = item.ApiKey.Value;
                                jobj["api_account"] = item.ApiID.Value;
                                jobj["api_cert_num"] = item.CertNum.Value;
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
            using (var network = ContainerProvider.Resolve<DataAgent.ApiDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                jobj["api_id"] = (int)(selecteditem as API).Id.Value;
                network.Delete(jobj);
                IsLoading.Value = true;
            }
        }

        public override void RowDoubleClickEvent()
        {
            DialogParameters dialogParameters = new DialogParameters();
            SelectedItem.Value.ClearJson();
            dialogParameters.Add("object", SelectedItem.Value as API);

            dialogService.ShowDialog("ApiAddPage", dialogParameters, r =>
            {
                try
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        API item = r.Parameters.GetValue<API>("object");
                        if (item != null)
                        {
                            using (var network = ContainerProvider.Resolve<DataAgent.ApiDataAgent>())
                            {
                                network.SetReceiver(this);
                                JObject jobj = new JObject();
                                jobj["changed_item"] = item.ChangedItem;
                                jobj["api_id"] = item.Id.Value;
                                network.Update(jobj);
                            }
                        }
                    }
                }
                catch (Exception) { }

            }, "CommonDialogWindow");
        }

        internal void SendBasicData(SettingPageViewModel settingPageViewModel)
        {
            using (var network = ContainerProvider.Resolve<DataAgent.ApiDataAgent>())
            {
                network.SetReceiver(settingPageViewModel);
                JObject jobj = new JObject();
                jobj["next_preview"] = (int)0;
                jobj["page_unit"] = (ListCount.Value);
                jobj["page_start_pos"] = (CurrentPage.Value - 1) * ListCount.Value;
                network.Get(jobj);
            }
        }

        public override void SearchTitle(string Keyword)
        {
            using (var network = ContainerProvider.Resolve<DataAgent.ApiDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                JObject search = new JObject();
                search["api_name"] = Keyword;
                jobj["page_unit"] = (ListCount.Value);
                jobj["page_start_pos"] = (CurrentPage.Value - 1) * ListCount.Value;
                jobj["search_option"] = search;
                network.Get(jobj);
            }
        }
    }
}
