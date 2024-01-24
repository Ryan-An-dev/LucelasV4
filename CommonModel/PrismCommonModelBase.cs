using CommonModel.Model;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace CommonModel
{
    public abstract class PrismCommonModelBase : BindableBase, IDisposable
    {
        public List<ReactiveProperty<string>> _stringProperties = new List<ReactiveProperty<string>>();
        public List<ReactiveProperty<int>> _intProperties = new List<ReactiveProperty<int>>();
        public List<ReactiveProperty<DateTime>> _dateTimeProperties = new List<ReactiveProperty<DateTime>>();
        public List<ReactiveProperty<PrismCommonModelBase>> _objectProperties = new List<ReactiveProperty<PrismCommonModelBase>>();
        public List<ReactiveProperty<Enum>> _enumProperties = new List<ReactiveProperty<Enum>>();
        protected CompositeDisposable disposable { get; }
            = new CompositeDisposable();
        public bool isinit { get; set; } = true;
        public bool isChanged { get; set; }
        public JObject ChangedItem { get; set; } = new JObject();
        public IRegionManager regionManager { get; } = null;
        public ReactiveProperty<DateTime> CreateDateTimeProperty(string propertyName) 
        {
            ReactiveProperty<DateTime> temp = new ReactiveProperty<DateTime>(DateTime.Now).AddTo(disposable);
            if (typeof(DateTime) == typeof(DateTime))
            {
                _dateTimeProperties.Add(temp);
                temp.SetValidateNotifyError(x =>
                {
                    // 유효성 검사 로직 추가
                    if (typeof(DateTime) == typeof(DateTime))
                    {
                        if (x == null)
                        {
                            return $"{propertyName}을(를) 입력하세요.";
                        }
                    }
                    return null; // 유효성 검사 통과
                });
            }
            return temp;
        }
        public ReactiveProperty<T> CreateProperty<T>(string propertyName) {
            ReactiveProperty<T> temp = null;
            temp = new ReactiveProperty<T>(mode: ReactivePropertyMode.IgnoreInitialValidationError).AddTo(disposable);
            if (typeof(T) == typeof(string))
            {
                _stringProperties.Add(temp as ReactiveProperty<string>);
                temp.SetValidateNotifyError(x =>
                {
                    // 유효성 검사 로직 추가
                    if (string.IsNullOrEmpty(x as string))
                    {
                        return $"{propertyName}을(를) 입력하세요.";
                    }
                    return null; // 유효성 검사 통과
                });
            }
            else if (typeof(T) == typeof(int))
            {
                _intProperties.Add(temp as ReactiveProperty<int>);
                temp.SetValidateNotifyError(x =>
                {
                    if (x != null)
                    {
                        // 유효성 검사 로직 추가
                        if (typeof(T) == typeof(int))
                        {
                            if (!int.TryParse(x.ToString(), out int intValue))
                            {
                                return $"{propertyName}의 숫자만 입력가능합니다.";
                            }
                            if (intValue <= 0)
                            {
                                return $"{propertyName}의 값은 0보다 커야 합니다.";
                            }
                            return null;
                        }
                    }

                    return null; // 유효성 검사 통과
                });
            }
            
            return temp;
        }
        

        public bool ValidateAllProperties()
        {
            bool check = false;
            foreach (var property in _stringProperties) { 
                property.ForceValidate();
                check |= property.HasErrors;
            }
            foreach (var property in _intProperties) { 
                property.ForceValidate();
                check |= property.HasErrors;
            }
            foreach (var property in _dateTimeProperties)
            {
                property.ForceValidate();
                check |= property.HasErrors;
            }
            foreach (ReactiveProperty<PrismCommonModelBase> property in _objectProperties)
            {
                {
                    property.ForceValidate();
                }
                check |= property.HasErrors;
            }
            return check;
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
            try
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
                    else if (value is Enum)
                    {
                        ChangedItem[name] = (int)value;
                    }
                    else if (value is DateTime)
                    {
                        DateTime time = (DateTime)value;
                        ChangedItem[name] = time.ToString("yyyy-MM-dd");
                    }
                    else if (value is bool)
                    {
                        ChangedItem[name] = (bool)value;
                    }
                    isChanged = true;
                }
            } catch (Exception) {
                
            }
           
        }
    }
}
