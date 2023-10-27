using Newtonsoft.Json.Linq;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace PrsimCommonBase
{
    public abstract class PrismCommonViewModelBase : BindableBase, IDisposable
    {
        #region  프로퍼티

        protected CompositeDisposable disposable { get; }
            = new CompositeDisposable();

        public ReactiveProperty<bool> isChanged { get; set; }
        public ReactiveProperty<JObject> ChangedItem { get; set; }
        public IRegionManager regionManager { get; } = null;

        #endregion

        #region 생성자
        public PrismCommonViewModelBase()
        {
            this.isChanged = new ReactiveProperty<bool>(false).AddTo(disposable);
            this.ChangedItem = new ReactiveProperty<JObject>().AddTo(disposable);
        }

        public PrismCommonViewModelBase(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
            this.isChanged = new ReactiveProperty<bool>(false).AddTo(disposable);
            this.ChangedItem = new ReactiveProperty<JObject>().AddTo(disposable);
        }

        #endregion

        #region  디스포저블 구현

        private void disposeRegions()
        {
            if (this.regionManager == null)
                return;
            foreach (var region in this.regionManager.Regions)
            {
                region.RemoveAll();
            }
        }

        private bool disposed = false;

        public void Dispose()
        {
            if (!disposed)
            {
                this.disposable.Dispose();

                //this.disposeRegions();
            }

            disposed = true;
        }
        #endregion
    }
}
