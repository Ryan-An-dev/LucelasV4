using ContractPage.ViewModels;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ContractPage.Views
{
    /// <summary>
    /// Interaction logic for FindInventoryItem
    /// </summary>
    public partial class FindInventoryItem : UserControl
    {
        public FindInventoryItem()
        {
            InitializeComponent();
        }

        //private void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        //{

        //    if (DataContext is FindInventoryItemViewModel viewModel)
        //    {
        //        if (viewModel.SelectedFurniture.Value != null)
        //        {
        //            viewModel.SelectedFurniture.Value.Memo.Value = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd).Text;
        //        }
        //    }
        //}
    }
}
