using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Nop.Core;
using Nop.Plugin.NopStation.NopChat.Hubs;
using Nop.Plugin.NopStation.NopChat.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Plugin.NopStation.NopChat.Infrastructure;
using Nop.Plugin.NopStation.NopChat.Domains;
using Nop.Plugin.NopStation.NopChat.Services;
using System;
using System.Collections.Generic;

namespace Nop.Plugin.NopStation.NopChat.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    [AutoValidateAntiforgeryToken]
    public class NopChatAdminController : BasePluginController
    {
        private readonly IWorkContext _workContext;
        private readonly NopChatHub _nopChatHub;
        private readonly INopChatMessageService _nopChatMessageService;

        public NopChatAdminController(
            IWorkContext workContext,
            NopChatHub nopChatHub,
            INopChatMessageService nopChatMessageService
            )
        {
            _workContext = workContext;
            _nopChatHub = nopChatHub;
            _nopChatMessageService = nopChatMessageService;
        }

        public async Task<IActionResult> Configure()
        {
            var Customer = _workContext.GetCurrentCustomerAsync();
            ViewBag.CustomerName = Customer.Result.Username;

            return View("~/Plugins/NopStation.NopChat/Areas/Admin/Views/WidgetsNopChat/Configure.cshtml");
        }

        public async Task<IActionResult> AdminChatBox(int id)
        {
            var customerId = (await _workContext.GetCurrentCustomerAsync()).Id;
            var vendorId = (await _workContext.GetCurrentCustomerAsync()).VendorId;
            NopChatMessageModel model = new NopChatMessageModel();
            model.VendorId = vendorId;
            var contactList = await _nopChatMessageService.GetVendorChatListListAsync(vendorId);


            model.VendorCustomerId = customerId;
            model.ContactList = contactList;

            return View("~/Plugins/NopStation.NopChat/Areas/Admin/Views/WidgetsNopChat/AdminChatBox.cshtml", model);
        }
    }
}
