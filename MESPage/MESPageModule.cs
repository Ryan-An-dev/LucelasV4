﻿using MESPage.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace MESPage
{
    public class MESPageModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.Resolve<IRegionManager>();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}