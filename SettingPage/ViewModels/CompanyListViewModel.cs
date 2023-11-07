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
using System.Windows.Interop;

namespace SettingPage.ViewModels
{
    public class CompanyListViewModel : PrsimListViewModelBase, INetReceiver
    {
        public CompanyListViewModel(IContainerProvider containerprovider, IRegionManager regionManager, IDialogService dialogService) : base(regionManager, containerprovider,dialogService)
        {
            
        }

        public override void UpdatePageItem(MovePageType param, int count)
        {
            using (var network = ContainerProvider.Resolve<DataAgent.CompanyDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                jobj["page_unit"] = (ListCount.Value * CurrentPage.Value) > TotalItemCount.Value ? TotalItemCount.Value - (ListCount.Value * (CurrentPage.Value - 1)) : ListCount.Value;
                jobj["page_start_pos"] = (CurrentPage.Value - 1) * ListCount.Value;
                network.repo.Read(jobj);
            }
        }
        public void OnConnected()
        {

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
            switch ((COMMAND)packet.Header.CMD)
            {
                case COMMAND.GETCOMPANYINFO: //데이터 조회
                    List.Clear();
                    if (jobject.ToString().Trim() != string.Empty)
                    {
                        try
                        {
                            if (jobject["company_list"] == null)
                                return;
                            JArray jarr = new JArray();
                            jarr = jobject["company_list"] as JArray;
                            foreach (JObject jobj in jarr)
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
                                if (jobj["company_address_detail"] != null)
                                    temp.CompanyAddressDetail.Value = jobj["company_address_detail"].ToString();

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
            dialogParameters.Add("object", new Company());

            dialogService.ShowDialog("CompanyAddPage", dialogParameters, r =>
            {
                try
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        Company item = r.Parameters.GetValue<Company>("object");
                        if (item != null)
                        {
                            using (var network = ContainerProvider.Resolve<DataAgent.CompanyDataAgent>())
                            {
                                network.SetReceiver(this);
                                JObject jobj = new JObject();
                                jobj["company_id"] = (int)0;
                                jobj["company_name"] = item.CompanyName.Value;
                                jobj["company_address_detail"] = item.CompanyAddressDetail.Value;
                                jobj["company_phone"] = item.CompanyPhone.Value;
                                jobj["company_address"] = item.CompanyAddress.Value;
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
            using (var network = ContainerProvider.Resolve<DataAgent.CompanyDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                jobj["company_id"] = (int)(selecteditem as Company).Id.Value;
                network.Delete(jobj);
                IsLoading.Value = true;
            }
        }

        public override void RowDoubleClickEvent()
        {
            DialogParameters dialogParameters = new DialogParameters();
            SelectedItem.Value.ClearJson();
            dialogParameters.Add("object", SelectedItem.Value as Company);

            dialogService.ShowDialog("CompanyAddPage", dialogParameters, r =>
            {
                try
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        Company item = r.Parameters.GetValue<Company>("object");
                        if (item != null)
                        {
                            using (var network = ContainerProvider.Resolve<DataAgent.CompanyDataAgent>())
                            {
                                network.SetReceiver(this);
                                JObject jobj = new JObject();
                                jobj["changed_item"] = item.ChangedItem;
                                jobj["company_id"] = item.Id.Value;
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
            using (var network = ContainerProvider.Resolve<DataAgent.CompanyDataAgent>())
            {
                network.SetReceiver(settingPageViewModel);
                JObject jobj = new JObject();
                jobj["next_preview"] = (int)0;
                jobj["page_unit"] = (ListCount.Value);
                jobj["page_start_pos"] = (CurrentPage.Value - 1) * ListCount.Value;
                network.Get(jobj);
            }
        }
    }
}
