using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Plugin.NopStation.NopChat.Domains;
using Nop.Plugin.NopStation.NopChat.Models;
using Nop.Services.Customers;
using Nop.Services.Vendors;

namespace Nop.Plugin.NopStation.NopChat.Factories
{
    public class NopChatMessageModelFactory: INopChatMessageModelFactory
    {
        private readonly ICustomerService _customerService;
        private readonly IVendorService _vendorService;

        public NopChatMessageModelFactory(
            ICustomerService customerService,
            IVendorService vendorService)
        {
            _customerService = customerService;
            _vendorService = vendorService;
        }
        public async Task<NopChatMessageModel> PrepareNopChatMessageModelAsync(NopChatMessage data)
        {
            NopChatMessageModel model = new NopChatMessageModel();
            model.Id = data.Id;
            model.Text = data.Text;
            model.DateCreated = data.DateCreated;
            model.VendorId = data.VendorId;
            model.VendorName = (await _vendorService.GetVendorByIdAsync(data.VendorId)).Name;
            model.CustomerId = data.CustomerId;
            model.CustomerName = data.CustomerId > 0 ? (await _customerService.GetCustomerByIdAsync(data.CustomerId)).Username : "";
            model.VendorCustomerId = data.VendorCustomerId;
            if(data.VendorCustomerId >0)
                model.VendorCustomerName = (await _customerService.GetCustomerByIdAsync(data.VendorCustomerId)).Username;

            model.IsVendorResponse = data.IsVendorResponse;
            model.IsChecked = data.IsChecked;

            return model;
        }
    }
}
