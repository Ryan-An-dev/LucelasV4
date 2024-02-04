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
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using System.Threading;

namespace SettingPage.ViewModels
{
    public class CompanyListViewModel : PrsimListViewModelBase, INetReceiver
    {
        public DelegateCommand CmdExcelUpload { get; }
        public CompanyListViewModel(IContainerProvider containerprovider, IRegionManager regionManager, IDialogService dialogService) : base(regionManager, containerprovider,dialogService)
        {
            this.CmdExcelUpload = new DelegateCommand(ExcelUpload);
        }

        private void ExcelUpload()
        {
            OpenExcelFile();
        }
        public void OpenExcelFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                ReadExcelFile(filePath);
            }
        }

        public void ReadExcelFile(string filePath)
        {
            try
            {
                var excelApp = new Excel.Application();
                Excel.Workbook workbook = excelApp.Workbooks.Open(filePath);
                Excel._Worksheet worksheet = workbook.Sheets[1];
                Excel.Range range = worksheet.UsedRange;

                for (int row = 2; row <= range.Rows.Count; row++)
                {
                    Company company = new Company();
                    for (int col = 1; col <= range.Columns.Count; col++)
                    {
                        string cellValue = range.Cells[row, col].Value2;
                        if (col == 1)
                        {
                            company.CompanyName.Value = cellValue;
                        }
                        else {
                            company.CompanyPhone.Value = cellValue;
                        }
                        // 여기에서 cellValue를 사용
                        ErpLogWriter.LogWriter.Debug(cellValue);
                    }
                    using (var network = ContainerProvider.Resolve<DataAgent.CompanyDataAgent>())
                    {
                        network.SetReceiver(this);
                        JObject jobj = new JObject();
                        jobj["company_id"] = (int)0;
                        jobj["company_name"] = company.CompanyName.Value;
                        jobj["company_address_detail"] = company.CompanyAddressDetail.Value;
                        jobj["company_phone"] = company.CompanyPhone.Value;
                        jobj["company_address"] = company.CompanyAddress.Value;
                        network.Create(jobj);
                        IsLoading.Value = true;
                    }
                    Thread.Sleep(300);
                }
                workbook.Close();
                excelApp.Quit();
                IsLoading.Value = false;
            }
            catch (Exception ex)
            {
                ErpLogWriter.LogWriter.Debug(ex.ToString());
            }
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
            ErpLogWriter.LogWriter.Debug(msg);
            if (packet.Header.CMD < (ushort)COMMAND.CREATECOMPANYINFO
                || packet.Header.CMD > (ushort)COMMAND.DELETECOMPANYINFO)
            {
                return;
            }
            switch (packet.Header.CMD)
            {
                case (ushort)COMMAND.CREATECOMPANYINFO:
                case (ushort)COMMAND.DELETECOMPANYINFO:
                case (ushort)COMMAND.UPDATECOMPANYINFO:
                    SearchTitle(this.Keyword.Value);
                    break;
                case (ushort)COMMAND.GETCOMPANYINFO:
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
                            if (jobject["company_list"] == null)
                                return;
                            JArray jarr = new JArray();
                            jarr = jobject["company_list"] as JArray;
                            if (jobject["history_count"] != null)
                                TotalItemCount.Value = jobject["history_count"].ToObject<int>();
                            int i = CurrentPage.Value == 1 ? 1 : ListCount.Value * (CurrentPage.Value - 1) + 1;
                            foreach (JObject jobj in jarr)
                            {
                                Company temp = new Company();
                                temp.No.Value = i++;
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
            if (SelectedItem.Value == null)
                return;
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

        public override void SearchTitle(string Keyword)
        {
            using (var network = ContainerProvider.Resolve<DataAgent.CompanyDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                JObject search = new JObject();
                search["company_name"] = Keyword;
                jobj["page_unit"] = (ListCount.Value);
                jobj["page_start_pos"] = (CurrentPage.Value - 1) * ListCount.Value;
                jobj["search_option"] = search;
                network.Get(jobj);
            }
        }
    }
}
