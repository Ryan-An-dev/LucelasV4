using ContractPage.ViewModels;
using Reactive.Bindings;
using System.Windows.Controls;
using System.Windows.Input;

namespace ContractPage.Views
{
    /// <summary>
    /// Interaction logic for SearchAdressPage
    /// </summary>
    public partial class SearchAdressPage : UserControl
    {
        private ReactiveProperty<SearchAdressPageViewModel> ViewModel;
        public SearchAdressPage()
        {
            InitializeComponent();
            ViewModel = new ReactiveProperty<SearchAdressPageViewModel>((SearchAdressPageViewModel)DataContext);
        }

        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            
            if (e.Key == Key.Enter)
            {
                ViewModel.Value.SearchAddress(this.Search.Text);
            }
        }
    }
}
