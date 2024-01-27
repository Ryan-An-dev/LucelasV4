using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace MESPage.Views
{
    /// <summary>
    /// Interaction logic for MesSingle
    /// </summary>
    public partial class MesSingle : UserControl
    {
        public MesSingle()
        {
            InitializeComponent();
            Storyboard storyboard = this.FindResource("HighlightAnimation") as Storyboard;
            storyboard.Begin();
            storyboard = this.FindResource("HighlightAnimation2") as Storyboard;
            storyboard.Begin();
        }
    }
}
