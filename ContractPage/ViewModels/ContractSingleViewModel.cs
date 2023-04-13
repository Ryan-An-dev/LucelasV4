using CommonModel.Model;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows.Controls;

namespace ContractPage.ViewModels
{
    public class ContractSingleViewModel : BindableBase, INavigationAware, IDisposable
    {
        public DelegateCommand SaveButton { get; }
        public ReactiveProperty<string> Title { get; } = new();
        private readonly CompositeDisposable _disposable = new();
        public ReactiveProperty<Contract> Contract { get; set; }
        public DelegateCommand DeleteButton { get; }
        private IRegionManager RegionManager { get; }
        public ContractSingleViewModel(IRegionManager regionManager)
        {
            RegionManager = regionManager;

            //Title.AddTo(_disposable);

            SaveButton = new DelegateCommand(SaveButtonExecute);
            DeleteButton = new DelegateCommand(DeleteButtonExecute);
            Title.Value = "신규등록";
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
        public bool IsNavigationTarget(NavigationContext navigationContext) => false;

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            //var author = navigationContext.Parameters[nameof(CommonModule.Entity.Extended.Author)] as Author;

            //if (author == null)
            //{
            //    Title.Value = "Create New Author";
            //    VisibilityDeleteButton.Value = Visibility.Collapsed;
            //}
            //else if (!Utility.IsNewEntity(author))
            //{
            //    Title.Value = "Edit Author";
            //    VisibilityDeleteButton.Value = Visibility.Visible;
            //    Author.Id = author.Id;
            //    Author.RpName.Value = author.RpName.Value;
            //    Author.RpPenName.Value = author.RpPenName.Value;
            //    Author.RpBirthday.Value = author.RpBirthday.Value;

            //    AuthorForDelete = author;
            //}
        }
        private void SaveButtonExecute()
        {
            //Author.SetToBaseParam();
            Save();
        }

        private void DeleteButtonExecute()
        {
            //Author.SetToBaseParam(AuthorForDelete);
            Save();
        }

        private void Save()
        {
            //AuthorModel.Merge(Author);

            DrawerHost.CloseDrawerCommand.Execute(Dock.Right, null);
            Dispose();

            RegionManager.RequestNavigate("ContentRegion", nameof(ContractPage));
        }
    }
}
