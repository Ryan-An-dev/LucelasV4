using Prism.Regions;
using System.Windows.Controls;

namespace DeliveryPage.Views
{
    /// <summary>
    /// Interaction logic for MesPage
    /// </summary>
    public partial class DeliveryPage : UserControl
    {
        public DeliveryPage(IRegionManager regionManager)
        {
            InitializeComponent();
            if (regionManager.Regions.ContainsRegionWithName("DeliverySinglePageRegion")) return;
            CommonModule.Logic.Utility.SetRegionManager(regionManager, Cc, "DeliverySinglePageRegion");
        }
    }
}
