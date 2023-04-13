using System.Windows.Controls;
using Prism.Regions;

namespace ContractPage.Views
{
    /// <summary>
    /// Interaction logic for ContractPage
    /// </summary>
    public partial class ContractPage : UserControl
    {
        public ContractPage(IRegionManager regionManager)
        {
            InitializeComponent();
            if (regionManager.Regions.ContainsRegionWithName("ContractSingleRegion")) return;
            CommonModule.Logic.Utility.SetRegionManager(regionManager, Cc, "ContractSingleRegion");
        }
    }
}
