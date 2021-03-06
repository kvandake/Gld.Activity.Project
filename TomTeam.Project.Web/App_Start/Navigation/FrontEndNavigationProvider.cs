﻿using Abp.Application.Navigation;
using Abp.Localization;

namespace TomTeam.Project.Web.Navigation
{
    /// <summary>
    /// This class defines font-end web site's menu.
    /// It uses ABP's menu system.
    /// When you add menu items here, they are automatically appear in the front-end web site.
    /// </summary>
    public class FrontEndNavigationProvider : NavigationProvider
    {
        public const string MenuName = "Frontend";

        public override void SetNavigation(INavigationProviderContext context)
        {
            var frontEndMenu = new MenuDefinition(MenuName, new FixedLocalizableString("Frontend menu"));
            context.Manager.Menus[MenuName] = frontEndMenu;

            frontEndMenu

                //HOME
                .AddItem(new MenuItemDefinition(
                    PageNames.Frontend.Home,
                    L("HomePage"),
                    url: ""
                    )
                ).AddItem(new MenuItemDefinition(
                    PageNames.Frontend.ProvincialExamination,
                    L("HomeNews"),
                    url: "News"
                    )
                ).AddItem(new MenuItemDefinition(
                    PageNames.Frontend.ProvincialExamination,
                    L("ProvincialExamination"),
                    url: "Provincial"
                    )
                ).AddItem(new MenuItemDefinition(
                    PageNames.Frontend.MetropolitanExamination,
                    L("MetropolitanExamination"),
                    url: "Metropolitan"
                    )
                ).AddItem(new MenuItemDefinition(
                    PageNames.Frontend.PalaceExamination,
                    L("PalaceExamination"),
                    url: "Palace"
                    )
                );
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, TomAbpConsts.LocalizationSourceName);
        }
    }
}