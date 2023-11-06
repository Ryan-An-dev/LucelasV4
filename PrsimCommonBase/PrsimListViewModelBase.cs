﻿using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrsimCommonBase
{
    public enum MovePageType { Next = 1, Prev }
    public abstract class PrsimListViewModelBase : PrismCommonViewModelBase
    {
        #region Paging
        public ObservableCollection<int> CountList { get; set; } = new ObservableCollection<int>();
        public ReactiveProperty<int> CurrentPage { get; set; }
        public ReactiveProperty<int> TotalPage { get; set; }
        public ReactiveProperty<int> TotalItemCount { get; set; }
        public ReactiveProperty<int> ListCount { get; set; }
        public ReactiveProperty<int> FirstItem { get; set; }
        public DelegateCommand<object> CmdGoPage { get; }
        public DelegateCommand<string> AddDeleteButton { get; }
        public ReactiveProperty<PrismCommonModelBase> SelectedItem { get; set; }
        public IDialogService dialogService { get; }
        public ReactiveCollection<PrismCommonModelBase> List { get; set; }

        #endregion
        public IContainerProvider ContainerProvider { get; }
        public PrsimListViewModelBase(IRegionManager regionManager, IContainerProvider containerProvider,IDialogService dialogService) : base(regionManager) {
            ContainerProvider = containerProvider;
            this.dialogService = dialogService;
            this.CurrentPage = new ReactiveProperty<int>(1).AddTo(this.disposable);
            this.ListCount = new ReactiveProperty<int>(30).AddTo(this.disposable);
            this.FirstItem = new ReactiveProperty<int>(0).AddTo(this.disposable);
            this.TotalPage = new ReactiveProperty<int>(0).AddTo(this.disposable);
            this.TotalItemCount = new ReactiveProperty<int>(0).AddTo(this.disposable);
            this.TotalItemCount.Subscribe(c => this.TotalPage.Value = (c / this.ListCount.Value) + 1);
            CmdGoPage = new DelegateCommand<object>(ExecCmdGoPage);
            AddDeleteButton = new DelegateCommand<string>(ExecAddDeleteButton);
            this.SelectedItem = new ReactiveProperty<PrismCommonModelBase>().AddTo(this.disposable);
            this.List = new ReactiveCollection<PrismCommonModelBase>().AddTo(this.disposable);

            this.CountList.Add(30);
            this.CountList.Add(50);
            this.CountList.Add(70);
            this.CountList.Add(100);
        }

        private void ExecCmdGoPage(object param)
        {
            MovePageType moveType = (MovePageType)param;
            if (this.CurrentPage.Value == this.TotalPage.Value && moveType == MovePageType.Next)
            {
                return;
            }
            if (this.CurrentPage.Value == 1 && moveType == MovePageType.Prev)
            {
                return;
            }
            switch (moveType)
            {
                case MovePageType.Next:
                    this.CurrentPage.Value = this.CurrentPage.Value == this.TotalPage.Value ? this.CurrentPage.Value : this.CurrentPage.Value + 1;
                    break;
                case MovePageType.Prev:
                    this.CurrentPage.Value = this.CurrentPage.Value == 1 ? 1 : this.CurrentPage.Value - 1;
                    break;
                default:
                    break;
            }
            UpdatePageItem(moveType, this.ListCount.Value);
        }
        public abstract void UpdatePageItem(MovePageType param, int count);

        private void ExecAddDeleteButton(string value) {
            switch (value)
            {
                case "Add":
                    AddButtonClick();
                    break;
                case "Delete":
                    DeleteButtonClick();
                    break;
            }
        }
        public abstract void AddButtonClick();
        public void DeleteButtonClick() {
            if (this.SelectedItem.Value == null)
                return;
            this.List.Remove(this.SelectedItem.Value);
        }
    }
}
