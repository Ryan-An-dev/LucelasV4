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
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using System.Threading;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;

namespace SettingPage.ViewModels
{
    public class ProductListViewModel : PrsimListViewModelBase, INetReceiver
    {
        public DelegateCommand CmdExcelUpload { get; }
        public ReactiveProperty<CompanyProductSelect> CompanyProductTypeSelect { get; set; } //검색옵션
        public IEnumerable<CompanyProductSelect> CompanyProductTypeSelectValues //검색옵션
        {
            get { return Enum.GetValues(typeof(CompanyProductSelect)).Cast<CompanyProductSelect>(); }
        }
        public ReactiveCommand List_MouseDoubleClick { get; set; }

        public ReactiveCollection<Company> CompanyList { get; set; }

        [TypeConverter(typeof(EnumDescriptionTypeConverter))]
        public enum CompanyProductSelect
        {
            [Description("제품명")]
            ProductName = 0,
            [Description("회사명")]
            CompanyName = 1,
        }
        public ReactiveCollection<FurnitureType> FurnitureInfos { get; set; }
        public ProductListViewModel(IContainerProvider containerprovider, IRegionManager regionManager, IDialogService dialogService) : base(regionManager, containerprovider, dialogService)
        {
            CompanyList = new ReactiveCollection<Company>().AddTo(disposable);
            this.CmdExcelUpload = new DelegateCommand(ExcelUpload);
            List_MouseDoubleClick = new ReactiveCommand().WithSubscribe(()=> RowDoubleClickEvent()).AddTo(disposable);
            FurnitureInfos = new ReactiveCollection<FurnitureType>().AddTo(disposable);
            CompanyProductTypeSelect = new ReactiveProperty<CompanyProductSelect>(CompanyProductSelect.ProductName).AddTo(disposable);
        }
        private void ExcelUpload()
        {
            using (var network = ContainerProvider.Resolve<DataAgent.CompanyDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                jobj["next_preview"] = (int)0;
                jobj["all_mode"] = 1;
                jobj["page_unit"] = (ListCount.Value);
                jobj["page_start_pos"] = (CurrentPage.Value - 1) * ListCount.Value;
                network.Get(jobj);
            }
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
            var excelApp = new Excel.Application();
            Excel.Workbook workbook = excelApp.Workbooks.Open(filePath);
            try
            {
                Excel._Worksheet worksheet = workbook.Sheets[2];
                Excel.Range range = worksheet.UsedRange;

                for (int row = 2; row <= range.Rows.Count; row++)
                {
                    Product Product = new Product();
                    for (int col = 1; col <= range.Columns.Count; col++)
                    {
                        
                        if (col == 1)
                        {
                            string cellValue = range.Cells[row, col].Value2;
                            Product.Company.Value = FindCompany(cellValue);
                            if (Product.Company.Value == null) {
                                continue;
                            }
                        }
                        else if (col == 2)
                        {
                            string cellValue = range.Cells[row, col].Value2;
                            Product.ProductType.Value = FindProductType(cellValue);
                        }
                        else if (col == 3)
                        {
                            string cellValue = range.Cells[row, col].Value2;
                            Product.Name.Value = cellValue;
                        }
                        else {
                            double cellValue = range.Cells[row, col].Value2;
                            string convert = cellValue.ToString();
                            Product.Price.Value = int.Parse(convert);
                        }
                        // 여기에서 cellValue를 사용
                    }
                    using (var network = ContainerProvider.Resolve<DataAgent.ProductDataAgent>())
                    {
                        network.SetReceiver(this);
                        JObject jobj = new JObject();
                        jobj["product_id"] = (int)0;
                        jobj["product_type"] = (int)Product.ProductType.Value.Id.Value;
                        jobj["product_name"] = Product.Name.Value;
                        jobj["product_price"] = Product.Price.Value;
                        jobj["company_id"] = Product.Company.Value.Id.Value;
                        network.Create(jobj);
                        IsLoading.Value = true;
                    }
                    Thread.Sleep(500);
                }
                workbook.Close();
                excelApp.Quit();
                IsLoading.Value = false;
            }
            catch (Exception ex)
            {
                workbook.Close();
                excelApp.Quit();
                ErpLogWriter.LogWriter.Debug(ex.ToString());
            }
        }

        private FurnitureType FindProductType(string name)
        {
            SettingPageViewModel temp = ContainerProvider.Resolve<SettingPageViewModel>();
            return temp.FurnitureInfos.FirstOrDefault(x => x.Name.Value == name);
        }

        private Company FindCompany(string name)
        {
            return this.CompanyList.FirstOrDefault(x => x.CompanyName.Value == name);
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
                jobj["page_unit"] = ListCount.Value;
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
            JObject jobject = null;
            string msg = Encoding.UTF8.GetString(packet.Body);
            
            if (packet.Header.CMD == (ushort)COMMAND.GETCOMPANYINFO)
            {
                try { jobject = new JObject(JObject.Parse(msg)); }
                catch (Exception)
                {
                    return;
                }
                ErpLogWriter.LogWriter.Trace(jobject.ToString());
                System.Windows.Application.Current.Dispatcher.Invoke(() => { List.Clear(); });
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
                            Company item = new Company();
                            item.No.Value = i++;
                            if (jobj["company_name"] != null)
                                item.CompanyName.Value = jobj["company_name"].ToString();
                            if (jobj["company_phone"] != null)
                                item.CompanyPhone.Value = jobj["company_phone"].ToString();
                            if (jobj["company_id"] != null)
                                item.Id.Value = jobj["company_id"].ToObject<int>();
                            if (jobj["company_address"] != null)
                                item.CompanyAddress.Value = jobj["company_address"].ToString();
                            if (jobj["company_address_detail"] != null)
                                item.CompanyAddressDetail.Value = jobj["company_address_detail"].ToString();

                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                CompanyList.Add(item);
                            });
                        }
                    }
                    catch (Exception e) { LogWriter.ErpLogWriter.LogWriter.Debug(e.ToString()); }
                }
            }

            if (packet.Header.CMD < (ushort)COMMAND.CREATEPRODUCTINFO
                || packet.Header.CMD > (ushort)COMMAND.DELETEPRODUCTINFO)
            {
                return;
            }
            switch (packet.Header.CMD)
            {   case (ushort)COMMAND.GETCOMPANYINFO:
                case (ushort)COMMAND.CREATEPRODUCTINFO:
                case (ushort)COMMAND.DELETEPRODUCTINFO:
                case (ushort)COMMAND.UPDATEPRODUCTINFO:
                    SearchTitle(this.Keyword.Value);
                    break;
                case (ushort)COMMAND.GETPRODUCTINFO:
                    try { jobject = new JObject(JObject.Parse(msg)); }
                    catch (Exception)
                    {
                        return;
                    }
                    ErpLogWriter.LogWriter.Trace(jobject.ToString());
                    SettingPageViewModel temp = this.ContainerProvider.Resolve<SettingPageViewModel>();
                    FurnitureInfos = temp.FurnitureInfos;
                    System.Windows.Application.Current.Dispatcher.Invoke(() => { List.Clear(); });
                    if (jobject.ToString().Trim() != string.Empty)
                    {
                        try
                        {
                            if (jobject["product_list"] == null)
                                return;
                            JArray jarr = new JArray();
                            jarr = jobject["product_list"] as JArray;
                            if (jobject["history_count"] != null)
                                TotalItemCount.Value = jobject["history_count"].ToObject<int>();

                            int i = CurrentPage.Value == 1 ? 1 : ListCount.Value * (CurrentPage.Value - 1) + 1;

                            foreach (JObject jobj in jarr)
                            {
                                Product inventory = new Product();
                                inventory.No.Value = i++;
                                if (jobj["company"] != null)
                                    inventory.Company.Value = SetCompanyInfo(jobj["company"] as JObject);
                                if (jobj["product_type"] != null)
                                    inventory.ProductType.Value = FurnitureInfos.FirstOrDefault(x => x.Id.Value == jobj["product_type"].ToObject<int>());
                                if (jobj["product_name"] != null)
                                    inventory.Name.Value = jobj["product_name"].ToString();
                                if (jobj["product_price"] != null)
                                    inventory.Price.Value = jobj["product_price"].ToObject<int>();
                                if (jobj["product_id"]!=null)
                                    inventory.Id.Value = jobj["product_id"].ToObject<int>();
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
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
            dialogParameters.Add("object", new Product());

            dialogService.ShowDialog("ProductAddPage", dialogParameters, r =>
            {
                try
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        Product item = r.Parameters.GetValue<Product>("object");
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
                jobj["company_id"] = (int)(selecteditem as Product).Company.Value.Id.Value;
                jobj["product_id"] = (int)(selecteditem as Product).Id.Value;
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
            dialogParameters.Add("object", SelectedItem.Value as Product);

            dialogService.ShowDialog("ProductAddPage", dialogParameters, r =>
            {
                try
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        Product item = r.Parameters.GetValue<Product>("object");
                        if (item != null)
                        {
                            using (var network = ContainerProvider.Resolve<DataAgent.ProductDataAgent>())
                            {
                                network.SetReceiver(this);
                                JObject jobj = new JObject();
                                jobj["changed_item"] = item.ChangedItem;
                                jobj["product_id"] = item.Id.Value;
                                jobj["company_id"] = item.Company.Value.Id.Value;
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
            using (var network = ContainerProvider.Resolve<DataAgent.ProductDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                JObject search = new JObject();
                if (this.CompanyProductTypeSelect.Value == CompanyProductSelect.ProductName)
                {
                    search["product_name"] = Keyword;
                }
                else {
                    search["company_name"] = Keyword;
                }
                jobj["page_unit"] = (ListCount.Value);
                jobj["page_start_pos"] = (CurrentPage.Value - 1) * ListCount.Value;
                jobj["search_option"] = search;
                network.Get(jobj);
            }
        }
    }
}
