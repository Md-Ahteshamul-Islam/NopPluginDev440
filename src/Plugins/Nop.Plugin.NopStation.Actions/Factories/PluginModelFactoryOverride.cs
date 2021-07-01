using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services.Authentication.External;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Cms;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Plugins.Marketplace;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Pickup;
using Nop.Services.Tax;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Plugins;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Areas.Admin.Factories;

namespace Nop.Plugin.NopStation.Core.Factories
{
    public class PluginModelFactoryOverride : PluginModelFactory
    {
        #region Fields

        private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
        private readonly IAuthenticationPluginManager _authenticationPluginManager;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IMultiFactorAuthenticationPluginManager _multiFactorAuthenticationPluginManager;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPickupPluginManager _pickupPluginManager;
        private readonly IPluginService _pluginService;
        private readonly IShippingPluginManager _shippingPluginManager;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
        private readonly ITaxPluginManager _taxPluginManager;
        private readonly IWidgetPluginManager _widgetPluginManager;
        private readonly IWorkContext _workContext;
        private readonly OfficialFeedManager _officialFeedManager;

        #endregion

        #region Ctor

        public PluginModelFactoryOverride(IAclSupportedModelFactory aclSupportedModelFactory,
            IAuthenticationPluginManager authenticationPluginManager,
            IBaseAdminModelFactory baseAdminModelFactory,
            ILocalizationService localizationService,
            IMultiFactorAuthenticationPluginManager multiFactorAuthenticationPluginManager,
            ILocalizedModelFactory localizedModelFactory,
            IPaymentPluginManager paymentPluginManager,
            IPickupPluginManager pickupPluginManager,
            IPluginService pluginService,
            IShippingPluginManager shippingPluginManager,
            IStaticCacheManager staticCacheManager,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
            ITaxPluginManager taxPluginManager,
            IWidgetPluginManager widgetPluginManager,
            IWorkContext workContext,
            OfficialFeedManager officialFeedManager) :
                base(aclSupportedModelFactory,
                    authenticationPluginManager,
                    baseAdminModelFactory,
                    localizationService,
                    multiFactorAuthenticationPluginManager,
                    localizedModelFactory,
                    paymentPluginManager,
                    pickupPluginManager,
                    pluginService,
                    shippingPluginManager,
                    staticCacheManager,
                    storeMappingSupportedModelFactory,
                    taxPluginManager,
                    widgetPluginManager,
                    workContext,
                    officialFeedManager)
        {
            _aclSupportedModelFactory = aclSupportedModelFactory;
            _authenticationPluginManager = authenticationPluginManager;
            _baseAdminModelFactory = baseAdminModelFactory;
            _localizationService = localizationService;
            _localizedModelFactory = localizedModelFactory;
            _multiFactorAuthenticationPluginManager = multiFactorAuthenticationPluginManager;
            _paymentPluginManager = paymentPluginManager;
            _pickupPluginManager = pickupPluginManager;
            _pluginService = pluginService;
            _shippingPluginManager = shippingPluginManager;
            _staticCacheManager = staticCacheManager;
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
            _taxPluginManager = taxPluginManager;
            _widgetPluginManager = widgetPluginManager;
            _workContext = workContext;
            _officialFeedManager = officialFeedManager;
        }

        #endregion

        #region Methods

        public override async Task<PluginListModel> PreparePluginListModelAsync(PluginSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter plugins
            var group = string.IsNullOrEmpty(searchModel.SearchGroup) || searchModel.SearchGroup.Equals("0") ? null : searchModel.SearchGroup;
            var loadMode = (LoadPluginsMode)searchModel.SearchLoadModeId;
            var friendlyName = string.IsNullOrEmpty(searchModel.SearchFriendlyName) ? null : searchModel.SearchFriendlyName;
            var author = string.IsNullOrEmpty(searchModel.SearchAuthor) ? null : searchModel.SearchAuthor;

            //filter visible plugins
            var plugins = (await _pluginService.GetPluginDescriptorsAsync<IPlugin>(group: group, loadMode: loadMode, friendlyName: friendlyName, author: author))
                .Where(p => p.ShowInPluginsList)
                .OrderByDescending(plugin => plugin.AssemblyFileName.Contains("NopStation"))
                .ThenBy(plugin => plugin.Group)
                .ThenBy(plugin => plugin.DependsOn.ToList().Contains("NopStation.Core"))
                .ThenBy(plugin => plugin.FriendlyName).ToList()
                .ToPagedList(searchModel);

            //prepare list model
            var model = await new PluginListModel().PrepareToGridAsync(searchModel, plugins, () =>
            {
                return plugins.SelectAwait(async pluginDescriptor =>
                {
                    //fill in model values from the entity
                    var pluginModel = pluginDescriptor.ToPluginModel<PluginModel>();

                    //fill in additional values (not existing in the entity)
                    pluginModel.LogoUrl = await _pluginService.GetPluginLogoUrlAsync(pluginDescriptor);

                    if (pluginDescriptor.Installed)
                        PrepareInstalledPluginModel(pluginModel, pluginDescriptor.Instance<IPlugin>());

                    return pluginModel;
                });
            });

            return model;
        }

        #endregion
    }
}
