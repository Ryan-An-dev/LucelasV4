using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;


namespace PrsimCommonBase
{
    public abstract class PrismCommonModelBase : BindableBase, IDisposable
    {
        #region  프로퍼티

        protected CompositeDisposable disposable { get; }
            = new CompositeDisposable();

        public bool isChanged { get; set; }
        public JObject ChangedItem { get; set; } = new JObject();
        public IRegionManager regionManager { get; } = null;

        #endregion

        #region 생성자
        public PrismCommonModelBase()
        {

        }

        public PrismCommonModelBase(IRegionManager regionManager)
            : this()
        {
            this.regionManager = regionManager;
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
        public abstract JObject GetChangedItem();
    }
}
