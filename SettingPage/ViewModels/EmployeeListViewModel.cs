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
        public void OnConnected()
        {

        }

        public void OnRceivedData(ErpPacket packet)
        {
            throw new NotImplementedException();
        }

        public void OnSent()
        {

        }

        public override void AddButtonClick()
        {
            dialogService.ShowDialog("EmployeeAddPage", null, r =>
            {
                try
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        //CompanyList item = r.Parameters.GetValue<CompanyList>("Company");
                        //this.CompanyInfos.Add(item);
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
            throw new NotImplementedException();
        }
    }
}
