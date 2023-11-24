using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace PrsimCommonBase
{
    public abstract class PrismCommonModelBase : BindableBase, IDisposable
    {
        public List<ReactiveProperty<string>> _stringProperties = new List<ReactiveProperty<string>>();
        public List<ReactiveProperty<int>> _intProperties = new List<ReactiveProperty<int>>();
        public List<ReactiveProperty<DateTime>> _dateTimeProperties = new List<ReactiveProperty<DateTime>>();

        #region  프로퍼티

        protected CompositeDisposable disposable { get; }
            = new CompositeDisposable();
        public bool isinit { get; set; } = true;
        public bool isChanged { get; set; }
        public JObject ChangedItem { get; set; } = new JObject();
        public IRegionManager regionManager { get; } = null;
        #endregion
        public void SetReactiveProperty<T>(ReactiveProperty<T> property ,string propertyName)
        {
            if (typeof(T) == typeof(string))
            {
                _stringProperties.Add(property as ReactiveProperty<string>);
                property.SetValidateNotifyError(x =>
                {
                    // 유효성 검사 로직 추가
                    if (string.IsNullOrEmpty(x as string))
                    {
                        return $"{propertyName}을(를) 입력하세요.";
                    }
                    return null; // 유효성 검사 통과
                }).Skip(1);
            }
            else if (typeof(T) == typeof(int))
            {
                _intProperties.Add(property as ReactiveProperty<int>);
                property.SetValidateNotifyError(x =>
                {
                    // 유효성 검사 로직 추가
                    if (typeof(T) == typeof(int))
                    {
                        return $"{propertyName}을(를) 입력하세요.";
                    }
                    return null; // 유효성 검사 통과
                }).Skip(1);
            }
            else if (typeof(T) == typeof(DateTime)) {
                _dateTimeProperties.Add(property as ReactiveProperty<DateTime>);
                property.SetValidateNotifyError(x =>
                {
                    // 유효성 검사 로직 추가
                    if (typeof(T) == typeof(DateTime))
                    {
                        return $"{propertyName}을(를) 입력하세요.";
                    }
                    return null; // 유효성 검사 통과
                }).Skip(1);

            }
        }

        public bool ValidateAllProperties()
        {
            bool check = false;
            foreach (var property in _stringProperties) { 
                property.ForceValidate();
                check=property.HasErrors;
            }
            foreach (var property in _intProperties) { 
                property.ForceValidate();
                check = property.HasErrors;
            }
            return false;
        }
        #region 생성자
        public PrismCommonModelBase()
        {

        }

        public PrismCommonModelBase(IRegionManager regionManager)
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
        public JObject GetChangedItem() {
            return ChangedItem;
        }
        public void ClearJson() {
            this.ChangedItem.RemoveAll();
            this.isChanged = false;
        }
        public abstract void SetObserver();

        public void ChangedJson(string name, object value)
        {
            if (value != null)
            {
                if (value is int)
                {
                    ChangedItem[name] = (int)value;
                }
                else if (value is string)
                {
                    ChangedItem[name] = value.ToString();
                }
                else if (value is JToken)
                {
                    ChangedItem[name] = value.ToString();
                }
                else if (value is Enum) {
                    ChangedItem[name] = (int)value;
                }
                else if (value is DateTime)
                {
                    DateTime time = (DateTime)value;
                    ChangedItem[name] = time.ToString("yyyy-MM-dd");
                }

                isChanged = true;
            }
        }
    }
}
