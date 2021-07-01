using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
//using Nop.Plugin.Widgets.NivoSlider.Infrastructure.Cache;
using Nop.Plugin.NopStation.NopChat.Models;
using Nop.Plugin.NopStation.NopChat.Services;
using Nop.Services.Configuration;
using Nop.Services.Media;
using Nop.Services.Vendors;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.NopStation.NopChat.Components
{
    [ViewComponent(Name = "WidgetsNopChat")]
    public class WidgetsNopChatViewComponent : NopViewComponent
    {
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IVendorService _vendorService;
        private readonly INopChatMessageService _chatNopMessageService;

        public WidgetsNopChatViewComponent(
            IWebHelper webHelper,
            IWorkContext workContext,
            IVendorService vendorService,
            INopChatMessageService chatNopMessageService
            )
        {
            _webHelper = webHelper;
            _workContext = workContext;
            _vendorService = vendorService;
            _chatNopMessageService = chatNopMessageService;
        }


        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                var customer = (await _workContext.GetCurrentCustomerAsync());
                var customerId = customer.Id;
                //var vendorId = customer.VendorId;
                NopChatMessageModel model = new NopChatMessageModel();
                
                var contactList = await _chatNopMessageService.GetCustomerChatListListAsync(customerId);
                
                model.ContactList = contactList;
                model.CustomerId = customerId;

                if (widgetZone == "vendordetails_top")
                {
                    return View("~/Plugins/NopStation.NopChat/Views/PublicInfo.cshtml", model);
                }
                else
                {
                    return View("~/Plugins/NopStation.NopChat/Views/PublicChatBox.cshtml", model);
                }
            }
            else
            {
                return View("~/Plugins/NopStation.NopChat/Views/NoComponent.cshtml");
            }
        }
    }
}
