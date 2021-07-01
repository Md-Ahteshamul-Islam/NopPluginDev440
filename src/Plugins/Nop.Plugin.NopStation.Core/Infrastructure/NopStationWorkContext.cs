using System;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Tax;
using Microsoft.AspNetCore.Http;
using Nop.Services.Authentication;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Framework;
using Nop.Core.Security;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Nop.Plugin.NopStation.Core.Services;

namespace Nop.Plugin.NopStation.Core.Infrastructure
{
    public class NopStationWorkContext : WebWorkContext
    {
        #region Fields

        private IList<IWorkContextPlugin> _plugins;
        private Customer _cachedCustomer;
        private Customer _originalCustomerIfImpersonated;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICustomerService _customerService;
        private readonly IUserAgentHelper _userAgentHelper;
        private readonly IWorkContextPluginManager _workContextPluginManager;
        private readonly IAuthenticationService _authenticationService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public NopStationWorkContext(CookieSettings cookieSettings,
            CurrencySettings currencySettings,
            IAuthenticationService authenticationService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            IHttpContextAccessor httpContextAccessor,
            ILanguageService languageService,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IUserAgentHelper userAgentHelper,
            IVendorService vendorService,
            IWebHelper webHelper,
            LocalizationSettings localizationSettings,
            TaxSettings taxSettings,
            IWorkContextPluginManager workContextPluginManager)
            : base(cookieSettings,
                  currencySettings,
                  authenticationService,
                  currencyService,
                  customerService,
                  genericAttributeService,
                  httpContextAccessor,
                  languageService,
                  storeContext,
                  storeMappingService,
                  userAgentHelper,
                  vendorService,
                  webHelper,
                  localizationSettings,
                  taxSettings)
        {
            _httpContextAccessor = httpContextAccessor;
            _customerService = customerService;
            _userAgentHelper = userAgentHelper;
            _workContextPluginManager = workContextPluginManager;
            _authenticationService = authenticationService;
            _storeContext = storeContext;
            _genericAttributeService = genericAttributeService;
        }

        #endregion

        #region Utilities

        protected async Task<Customer> GetNopStationAuthenticatedCustomerAsync()
        {
            _plugins ??= _workContextPluginManager.LoadWorkContextPlugins(storeId: (await _storeContext.GetCurrentStoreAsync()).Id);

            foreach (var plugin in _plugins)
            {
                var customer = await plugin.GetAuthenticatedCustomerAsync();
                if (customer != null && !customer.Deleted && customer.Active && !customer.RequireReLogin)
                    return customer;
            }

            return null;
        }

        protected async Task<Customer> GetNopStationGuestCustomerAsync()
        {
            _plugins ??= _workContextPluginManager.LoadWorkContextPlugins(storeId: (await _storeContext.GetCurrentStoreAsync()).Id);

            foreach (var plugin in _plugins.Where(x => x.CheckGuestCustomer))
            {
                var customer = await plugin.GetGuestCustomerAsync();
                if (customer != null && !customer.Deleted && customer.Active && !customer.RequireReLogin)
                    return customer;
            }

            return null;
        }

        #endregion

        #region Properties

        public override async Task<Customer> GetCurrentCustomerAsync()
        {
            //whether there is a cached value
            if (_cachedCustomer != null)
                return _cachedCustomer;

            await SetCurrentCustomerAsync();

            return _cachedCustomer;
        }

        /// <summary>
        /// Sets the current customer
        /// </summary>
        /// <param name="customer">Current customer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task SetCurrentCustomerAsync(Customer customer = null)
        {
            if (customer == null)
            {
                //check whether request is made by a background (schedule) task
                if (_httpContextAccessor.HttpContext?.Request
                    ?.Path.Equals(new PathString($"/{Nop.Services.Tasks.NopTaskDefaults.ScheduleTaskPath}"), StringComparison.InvariantCultureIgnoreCase)
                    ?? true)
                {
                    //in this case return built-in customer record for background task
                    customer = await _customerService.GetOrCreateBackgroundTaskUserAsync();
                }

                if (customer == null || customer.Deleted || !customer.Active || customer.RequireReLogin)
                {
                    //check whether request is made by a search engine, in this case return built-in customer record for search engines
                    if (_userAgentHelper.IsSearchEngine())
                        customer = await _customerService.GetOrCreateSearchEngineUserAsync();
                }

                if (customer == null || customer.Deleted || !customer.Active || customer.RequireReLogin)
                {
                    //try to get registered user
                    customer = await GetNopStationAuthenticatedCustomerAsync();
                }

                if (customer == null || customer.Deleted || !customer.Active || customer.RequireReLogin)
                {
                    //try to get registered user
                    customer = await _authenticationService.GetAuthenticatedCustomerAsync();
                }

                if (customer != null && !customer.Deleted && customer.Active && !customer.RequireReLogin)
                {
                    //get impersonate user if required
                    var impersonatedCustomerId = await _genericAttributeService
                        .GetAttributeAsync<int?>(customer, NopCustomerDefaults.ImpersonatedCustomerIdAttribute);
                    if (impersonatedCustomerId.HasValue && impersonatedCustomerId.Value > 0)
                    {
                        var impersonatedCustomer = await _customerService.GetCustomerByIdAsync(impersonatedCustomerId.Value);
                        if (impersonatedCustomer != null && !impersonatedCustomer.Deleted &&
                            impersonatedCustomer.Active &&
                            !impersonatedCustomer.RequireReLogin)
                        {
                            //set impersonated customer
                            _originalCustomerIfImpersonated = customer;
                            customer = impersonatedCustomer;
                        }
                    }
                }

                if (customer == null || customer.Deleted || !customer.Active || customer.RequireReLogin)
                {
                    //try to get registered user
                    customer = await GetNopStationGuestCustomerAsync();
                }

                if (customer == null || customer.Deleted || !customer.Active || customer.RequireReLogin)
                {
                    //get guest customer
                    var customerCookie = GetCustomerCookie();
                    if (Guid.TryParse(customerCookie, out var customerGuid))
                    {
                        //get customer from cookie (should not be registered)
                        var customerByCookie = await _customerService.GetCustomerByGuidAsync(customerGuid);
                        if (customerByCookie != null && !await _customerService.IsRegisteredAsync(customerByCookie))
                            customer = customerByCookie;
                    }
                }

                if (customer == null || customer.Deleted || !customer.Active || customer.RequireReLogin)
                {
                    //create guest if not exists
                    customer = await _customerService.InsertGuestCustomerAsync();
                }
            }

            if (!customer.Deleted && customer.Active && !customer.RequireReLogin)
            {
                //set customer cookie
                SetCustomerCookie(customer.CustomerGuid);

                //cache the found customer
                _cachedCustomer = customer;
            }
        }

        public override Customer OriginalCustomerIfImpersonated => _originalCustomerIfImpersonated;

        #endregion
    }
}
