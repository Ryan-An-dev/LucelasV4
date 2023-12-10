using Prism.Regions;
using System.Windows.Controls;

namespace MESPage.Views
{
    /// <summary>
    /// Interaction logic for MesPage
    /// </summary>
    public partial class MesPage : UserControl
    {
        public MesPage(IRegionManager regionManager)
        {
            InitializeComponent();
            if (regionManager.Regions.ContainsRegionWithName("MesSingleRegion")) return;
            CommonModule.Logic.Utility.SetRegionManager(regionManager, Cc, "MesSingleRegion");
        }
    }
}
