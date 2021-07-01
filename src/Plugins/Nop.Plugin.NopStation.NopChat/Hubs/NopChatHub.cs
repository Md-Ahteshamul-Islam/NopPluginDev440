using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Nop.Plugin.NopStation.NopChat.Models;
using Nop.Core;
using Nop.Plugin.NopStation.NopChat.Services;
using Nop.Plugin.NopStation.NopChat.Domains;
using Nop.Services.Customers;
using Nop.Services.Vendors;

namespace Nop.Plugin.NopStation.NopChat.Hubs
{
    public class NopChatHub : Hub
    {
        #region Fields
        private readonly IWorkContext _workContext;
        private readonly IHubContext<NopChatHub> _hubContext;
        private readonly INopChatMessageService _chatNopMessageService;
        private readonly ICustomerService _customerService;
        private readonly IVendorService _vendorService;
        #endregion
        #region ctor
        public NopChatHub(
            IWorkContext workContext,
            IHubContext<NopChatHub> hubContext,
            INopChatMessageService chatNopMessageService,
            ICustomerService customerService,
            IVendorService vendorService)
        {
            _hubContext = hubContext;
            _workContext = workContext;
            _chatNopMessageService = chatNopMessageService;
            _customerService = customerService;
            _vendorService = vendorService;
        }
        #endregion
        
        #region methods
        public async Task SendNewMessage(NopChatMessageModel model)
        {
            var vendor = await _vendorService.GetVendorByIdAsync(model.VendorId);
            //var vendorName = "";
            if (vendor.Name != "")
                model.VendorName = vendor.Name;
            //vendorName = vendor.Name;

            if (model.IsVendorResponse == false)
            {

                await _hubContext.Clients.Group(model.VendorName).SendAsync("NewMessagesHub", new
                {
                    message = model
                });
            }
            else
            {
                var userName = _customerService.GetCustomerByIdAsync(model.CustomerId).Result.Username.ToString();
                await _hubContext.Clients.Groups(userName, model.VendorName).SendAsync("NewMessagesHub", new
                {
                    message = model
                });
            }
        }


        #endregion

        #region connections
        public override async Task OnConnectedAsync()
        {
            var connectionName = Context.User.Identity.Name;
            var vendorName = _chatNopMessageService.GetVendorNameByCustomerNameIfExxistAsync(connectionName).Result;

            if (vendorName != "")
                connectionName = vendorName.ToString();
            if (connectionName == "")
                connectionName = "Guest";

            await Groups.AddToGroupAsync(Context.ConnectionId, connectionName);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, Context.User.Identity.Name);
            await base.OnDisconnectedAsync(ex);
        }

        #endregion

    }
}
