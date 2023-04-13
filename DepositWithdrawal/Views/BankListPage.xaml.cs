using Prism.Regions;
using System.Windows.Controls;

namespace DepositWithdrawal.Views
{
    /// <summary>
    /// Interaction logic for BankListPage
    /// </summary>
    public partial class BankListPage : UserControl
    {
        public BankListPage(IRegionManager regionManager)
        {
            InitializeComponent();
            if (regionManager.Regions.ContainsRegionWithName("BankListSingleRegion")) return;
            CommonModule.Logic.Utility.SetRegionManager(regionManager, Cc, "BankListSingleRegion");
        }
    }
}
