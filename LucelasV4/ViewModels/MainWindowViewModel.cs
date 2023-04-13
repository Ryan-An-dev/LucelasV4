using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Disposables;
using System.Windows;

namespace LucelasV4.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        public ReactiveCommand<string> MenuSelectCommand { get; }

        private CompositeDisposable disposables = new CompositeDisposable();

        private IRegionManager regionmanager;

        public MainWindowViewModel(IRegionManager regionmanager)
        {
            //Login module 생성하고, True일때 DataAccess 의 IMPCommandDistributor 를 전역에서 사용하도록 등록해준다.
            this.regionmanager = regionmanager;
            this.MenuSelectCommand = new ReactiveCommand<string>().WithSubscribe(i => this.ExecuteMenuSelectCommand(i)).AddTo(this.disposables);
           
        }

        private void ExecuteMenuSelectCommand(string SelectedItem)
        {
            
            if (SelectedItem != string.Empty) {
                regionmanager.RequestNavigate("ContentRegion", SelectedItem);
            }
        }
    }
}
