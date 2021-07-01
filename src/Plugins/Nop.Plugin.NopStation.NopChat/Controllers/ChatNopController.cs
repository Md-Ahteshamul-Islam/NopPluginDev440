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
using Nop.Services.Vendors;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.NopStation.NopChat.Factories;

namespace Nop.Plugin.NopStation.NopChat.Controllers
{
    //[AuthorizeAdmin]
    //[Area(AreaNames.Admin)]
    [AutoValidateAntiforgeryToken]
    public class NopChatController : BasePluginController
    {
        private readonly IWorkContext _workContext;
        private readonly NopChatHub _nopChatHub;
        private readonly INopChatMessageService _nopChatMessageService;
        private readonly IVendorService _vendorService;
        private readonly INopChatMessageModelFactory _nopChatMessageModelFactory;

        public NopChatController(
            IWorkContext workContext,
            NopChatHub nopChatHub,
            INopChatMessageService nopChatMessageService,
            IVendorService vendorService,
            INopChatMessageModelFactory nopChatMessageModelFactory
            )
        {
            _workContext = workContext;
            _nopChatHub = nopChatHub;
            _nopChatMessageService = nopChatMessageService;
            _vendorService = vendorService;
            _nopChatMessageModelFactory = nopChatMessageModelFactory;
        }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SendMessage(NopChatMessage model)
        {

            model.DateCreated = DateTime.Now;
            model.IsChecked = false;
            try
            {
                await _nopChatMessageService.InsertNopChatMessageAsync(model);

                var newMessage = await _nopChatMessageModelFactory.PrepareNopChatMessageModelAsync(model);

                await _nopChatHub.SendNewMessage(newMessage);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Result = false,
                    Error = ex
                });
            }
            return Json(new { Result = true, Message = model });
        }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> GetChatHistory(int customerId, int vendorId)
        {
            var data = await _nopChatMessageService.GetChatHistoryAsync(customerId, vendorId);
            
            if(data == null)
                return Json(new { Result = false });

            return Json(new { Result = data });
        }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> GetVendorById(int vendorId)
        {
            var data = new Vendor();
            try
            {
                data = await _vendorService.GetVendorByIdAsync(vendorId);
            }
            catch (Exception ex)
            {
                return Json(new { Result = false });
            }
            return Json(new { Result = data });
        }
    }
}