using ContractPage.ViewModels;
using Reactive.Bindings;
using System.Windows.Controls;
using System.Windows.Input;

namespace ContractPage.Views
{
    /// <summary>
    /// Interaction logic for SearchNamePage
    /// </summary>
    public partial class SearchNamePage : UserControl
    {
        private ReactiveProperty<SearchNamePageViewModel> ViewModel;
        public SearchNamePage()
        {
            InitializeComponent();
            ViewModel = new ReactiveProperty<SearchNamePageViewModel>((SearchNamePageViewModel)DataContext);
        }

        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                ViewModel.Value.SearchName(this.Search.Text);
            }
        }
    }
}
