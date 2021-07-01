using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Common;
using Nop.Services.Plugins;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using Nop.Plugin.NopStation.Core;
using Nop.Plugin.NopStation.Core.Services;

namespace Nop.Plugin.NopStation.NopChat
{
    /// <summary>
    /// PLugin
    /// </summary>
    public class NopChatPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin
    {

        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;
        public NopChatPlugin(
            IWebHelper webHelper,
            INopStationCoreService nopStationCoreService
            )
        {
            _webHelper = webHelper;
            _nopStationCoreService = nopStationCoreService;
        }
        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/NopChatAdmin/Configure";
        }
        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> { PublicWidgetZones.Footer, PublicWidgetZones.VendorDetailsTop });
        }
        public string GetWidgetViewComponentName(string widgetZone)
        {
            return "WidgetsNopChat";
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "NopChatAdmin";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.NopStation.NopChat.Areas.Admin.Controllers" }, { "area", null } };
        }


        /// <summary>
        /// Gets a route for displaying widget
        /// </summary>
        /// <param name="widgetZone">Widget zone where it's displayed</param>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetDisplayWidgetRoute(string widgetZone, out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PublicInfo";
            controllerName = "NopChat";
            routeValues = new RouteValueDictionary
            {
                {"Namespaces", "Nop.Plugin.NopStation.NopChat.Controllers"},
                {"area", null},
                {"widgetZone", widgetZone}
            };
        }



        public bool HideInWidgetList => false;

        public override async Task InstallAsync()
        {
            // custom logic like adding settings, locale resources and database table(s) here

            await base.InstallAsync();
        }
        public override async Task UninstallAsync()
        {
            // custom logic like removing settings, locale resources and database table(s) which was created during widget installation

            await base.UninstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = "Nop Chat v.2",
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            var configure = new SiteMapNode()
            {
                Title = "Configure",
                Url = "/Admin/NopChatAdmin/Configure",
                Visible = true,
                IconClass = "far fa-circle",
                SystemName = "NopChat.Configure"
            };
            menuItem.ChildNodes.Add(configure);
            var chatBox = new SiteMapNode()
            {
                Title = "Chat box",
                Url = "/Admin/NopChatAdmin/AdminChatBox",
                Visible = true,
                IconClass = "far fa-circle",
                SystemName = "NopChat.ChatBox"
            };
            menuItem.ChildNodes.Add(chatBox);

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }
    }
}