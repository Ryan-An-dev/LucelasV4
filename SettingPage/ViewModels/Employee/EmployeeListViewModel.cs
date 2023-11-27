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

namespace SettingPage.ViewModels
{
    public class EmployeeListViewModel : PrsimListViewModelBase, INetReceiver
    {
        public EmployeeListViewModel(IContainerProvider containerprovider, IRegionManager regionManager, IDialogService dialogService) : base(regionManager, containerprovider, dialogService)
        {
        
        }

        public override void UpdatePageItem(MovePageType param, int count)
        {
            using (var network = ContainerProvider.Resolve<DataAgent.EmployeeDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                jobj["next_preview"] = (int)param;
                jobj["page_unit"] = (ListCount.Value * CurrentPage.Value) > TotalItemCount.Value ? TotalItemCount.Value - (ListCount.Value * (CurrentPage.Value - 1)) : ListCount.Value;
                jobj["page_start_pos"] = (CurrentPage.Value - 1) * ListCount.Value;
                network.GetEmployeeList(jobj);
            }
        }
        public void SendBasicData(INetReceiver receiver) {
            using (var network = ContainerProvider.Resolve<DataAgent.EmployeeDataAgent>())
            {
                network.SetReceiver(receiver);
                JObject jobj = new JObject();
                jobj["next_preview"] = (int)0;
                jobj["page_unit"] = (ListCount.Value);
                jobj["page_start_pos"] = (CurrentPage.Value - 1) * ListCount.Value;
                network.GetEmployeeList(jobj);
            }
        }
        public void OnConnected()
        {

        }

        public void OnRceivedData(ErpPacket packet)
        {
            string msg = Encoding.UTF8.GetString(packet.Body);
            ErpLogWriter.LogWriter.Debug(msg);
            if (packet.Header.CMD < (ushort)COMMAND.CREATEEMPLOEEINFO 
                || packet.Header.CMD > (ushort)COMMAND.DELETEEMPLOEEINFO)
            {
                return;
            }
            switch (packet.Header.CMD) {
                case (ushort)COMMAND.CREATEEMPLOEEINFO:
                case (ushort)COMMAND.DELETEEMPLOEEINFO:
                case (ushort)COMMAND.UPDATEEMPLOEEINFO:
                    SearchTitle(this.Keyword.Value);
                    break;
                case (ushort)COMMAND.GETEMPLOEEINFO:
                    JObject jobject = new JObject(JObject.Parse(msg));
                    if (!msg.Contains("null"))
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            List.Clear();
                        });
                        if (jobject["employee_list"] == null)
                            return;
                        JArray jarr = new JArray();
                        jarr = jobject["employee_list"] as JArray;
                        if (jobject["history_count"] != null)
                            TotalItemCount.Value = jobject["history_count"].ToObject<int>();
                        int i = CurrentPage.Value == 1 ? 1 : ListCount.Value * (CurrentPage.Value - 1) + 1;
                        foreach (JObject jobj in jarr)
                        {
                            Employee temp = new Employee();
                            temp.No.Value = i++;
                            if (jobj["employee_id"] != null)
                                temp.Id.Value = jobj["employee_id"].ToObject<int>();
                            if (jobj["employee_name"] != null)
                                temp.Name.Value = jobj["employee_name"].ToString();
                            if (jobj["employee_phone"] != null)
                                temp.Phone.Value = jobj["employee_phone"].ToString();
                            if (jobj["employee_start"] != null)
                                temp.StartWorkTime.Value = jobj["employee_start"].ToObject<DateTime>();
                            if (jobj["employee_address"] != null)
                                temp.Address.Value = jobj["employee_address"].ToString();
                            if (jobj["employee_address_detail"] != null)
                                temp.AddressDetail.Value = jobj["employee_address_detail"].ToString();
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                List.Add(temp);
                            });
                        }
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
            dialogParameters.Add("object", new Employee());

            dialogService.ShowDialog("EmployeeAddPage", dialogParameters, r =>
            {
                try
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        Employee item = r.Parameters.GetValue<Employee>("object");
                        if (item != null)
                        {
                            using (var network = ContainerProvider.Resolve<DataAgent.EmployeeDataAgent>())
                            {
                                network.SetReceiver(this);
                                JObject jobj = new JObject();
                                jobj["employee_id"] = (int)0;
                                jobj["employee_name"] = item.Name.Value;
                                jobj["employee_phone"] = item.Phone.Value;
                                jobj["employee_start"] = item.StartWorkTime.Value.ToString("yyyy-MM-dd");
                                jobj["employee_address"] = item.Address.Value;
                                jobj["employee_address_detail"] = item.AddressDetail.Value;
                                network.CreateEmployeeList(jobj);
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
            using (var network = ContainerProvider.Resolve<DataAgent.EmployeeDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                jobj["employee_id"] = (int)(selecteditem as Employee).Id.Value;
                network.DeleteEmployeeList(jobj);
                IsLoading.Value = true;
            }
        }

        public override void RowDoubleClickEvent()
        {
            DialogParameters dialogParameters = new DialogParameters();
            SelectedItem.Value.ClearJson();
            dialogParameters.Add("object", SelectedItem.Value as Employee);

            dialogService.ShowDialog("EmployeeAddPage", dialogParameters, r =>
            {
                try
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        Employee item = r.Parameters.GetValue<Employee>("object");
                        if (item != null)
                        {
                            using (var network = ContainerProvider.Resolve<DataAgent.EmployeeDataAgent>())
                            {
                                network.SetReceiver(this);
                                JObject jobj = new JObject();
                                jobj["changed_item"] = item.ChangedItem;
                                jobj["employee_id"] = item.Id.Value;
                                network.UpdateEmployeeList(jobj);
                            }
                        }
                    }
                }
                catch (Exception) { }

            }, "CommonDialogWindow");
        }

        public override void SearchTitle(string Keyword)
        {
            using (var network = ContainerProvider.Resolve<DataAgent.EmployeeDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                JObject search = new JObject();
                search["employee_name"] = Keyword;
                jobj["page_unit"] = (ListCount.Value);
                jobj["page_start_pos"] = (CurrentPage.Value - 1) * ListCount.Value;
                jobj["search_option"] = search;
                network.GetEmployeeList(jobj);
            }
        }
    }
}
