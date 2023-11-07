using CommonModel.Model;
using DataAccess;
using DataAccess.NetWork;
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
    }
}
