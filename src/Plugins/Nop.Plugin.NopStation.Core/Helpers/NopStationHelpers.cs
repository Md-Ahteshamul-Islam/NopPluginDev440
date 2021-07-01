using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Infrastructure;
using Nop.Plugin.NopStation.Core.Services;
using Nop.Services.Authentication.External;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Pickup;
using Nop.Data;

namespace Nop.Plugin.NopStation.Core.Helpers
{
    public static class NopStationHelpers
    {
        private static async Task<PermissionRecord> GetPermissionRecordBySystemNameAsync(string systemName, IRepository<PermissionRecord> permissionRecordRepository)
        {
            if (string.IsNullOrWhiteSpace(systemName))
                return null;

            var query = from pr in permissionRecordRepository.Table
                        where pr.SystemName == systemName
                        orderby pr.Id
                        select pr;

            var permissionRecord = await query.FirstOrDefaultAsync();
            return permissionRecord;
        }

        private static async Task DeletePermissionRecordAsync(PermissionRecord permission, IRepository<PermissionRecord> permissionRecordRepository)
        {
            await permissionRecordRepository.DeleteAsync(permission);
        }

        public static async Task NopStationPluginInstallAsync<TPlugin>(this TPlugin plugin, IPermissionProvider provider = null, bool autoEnable = true) where TPlugin : class, INopStationPlugin
        {
            var settingService = EngineContext.Current.Resolve<ISettingService>();
            var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
            var keyValuePairs = plugin.PluginResouces();
            foreach (var keyValuePair in keyValuePairs)
            {
                await localizationService.AddOrUpdateLocaleResourceAsync(keyValuePair.Key, keyValuePair.Value);
            }

            if (provider != null)
            {
                var permissionService = EngineContext.Current.Resolve<IPermissionService>();
                await permissionService.InstallPermissionsAsync(provider);
            }

            var nopStationCoreSettings = EngineContext.Current.Resolve<NopStationCoreSettings>();
            if (!nopStationCoreSettings.ActiveNopStationSystemNames?.Any(x => x == plugin.PluginDescriptor.SystemName) ?? false)
            {
                nopStationCoreSettings.ActiveNopStationSystemNames.Add(plugin.PluginDescriptor.SystemName);
                await settingService.SaveSettingAsync(nopStationCoreSettings);
            }

            if (autoEnable)
                await EnablePlugin(plugin, settingService);
        }

        public static async Task NopStationPluginUninstallAsync<TPlugin>(this TPlugin plugin, IPermissionProvider provider = null) where TPlugin : class, INopStationPlugin
        {
            var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
            var keyValuePairs = plugin.PluginResouces();
            foreach (var keyValuePair in keyValuePairs)
            {
                await localizationService.DeleteLocaleResourceAsync(keyValuePair.Key);
            }

            if (provider != null)
            {
                var permissionService = EngineContext.Current.Resolve<IPermissionService>();
                var permissionRecordRepository = EngineContext.Current.Resolve<IRepository<PermissionRecord>>();
                var permissions = provider.GetPermissions();

                foreach (var permission in permissions)
                {
                    var permission1 = await GetPermissionRecordBySystemNameAsync(permission.SystemName, permissionRecordRepository);
                    if (permission1 == null)
                        continue;

                    await DeletePermissionRecordAsync(permission1, permissionRecordRepository);
                }
            }

            var nopStationCoreSettings = EngineContext.Current.Resolve<NopStationCoreSettings>();
            if (nopStationCoreSettings.ActiveNopStationSystemNames.Any(x => x == plugin.PluginDescriptor.SystemName))
            {
                var settingService = EngineContext.Current.Resolve<ISettingService>();
                nopStationCoreSettings.ActiveNopStationSystemNames.Remove(plugin.PluginDescriptor.SystemName);
                await settingService.SaveSettingAsync(nopStationCoreSettings);
            }
        }

        public static async Task EnablePlugin<T>(T plugin, ISettingService settingService) where T : INopStationPlugin
        {
            try
            {
                var pluginIsActive = false;
                switch (plugin)
                {
                    case IPaymentMethod paymentMethod:
                        var paymentPluginManager = EngineContext.Current.Resolve<IPaymentPluginManager>();
                        pluginIsActive = paymentPluginManager.IsPluginActive(paymentMethod);
                        if (!pluginIsActive)
                        {
                            var paymentSettings = EngineContext.Current.Resolve<PaymentSettings>();
                            paymentSettings.ActivePaymentMethodSystemNames.Add(plugin.PluginDescriptor.SystemName);
                            await settingService.SaveSettingAsync(paymentSettings);
                        }

                        break;
                    case IShippingRateComputationMethod shippingRateComputationMethod:
                        var shippingPluginManager = EngineContext.Current.Resolve<IShippingPluginManager>();
                        pluginIsActive = shippingPluginManager.IsPluginActive(shippingRateComputationMethod);
                        if (!pluginIsActive)
                        {
                            var shippingSettings = EngineContext.Current.Resolve<ShippingSettings>();
                            shippingSettings.ActiveShippingRateComputationMethodSystemNames.Add(plugin.PluginDescriptor.SystemName);
                            await settingService.SaveSettingAsync(shippingSettings);
                        }

                        break;
                    case IPickupPointProvider pickupPointProvider:
                        var pickupPluginManager = EngineContext.Current.Resolve<IPickupPluginManager>();
                        pluginIsActive = pickupPluginManager.IsPluginActive(pickupPointProvider);
                        if (!pluginIsActive)
                        {
                            var shippingSettings = EngineContext.Current.Resolve<ShippingSettings>();
                            shippingSettings.ActivePickupPointProviderSystemNames.Add(plugin.PluginDescriptor.SystemName);
                            await settingService.SaveSettingAsync(shippingSettings);
                        }

                        break;
                    case IExternalAuthenticationMethod externalAuthenticationMethod:
                        var authenticationPluginManager = EngineContext.Current.Resolve<IAuthenticationPluginManager>();
                        pluginIsActive = authenticationPluginManager.IsPluginActive(externalAuthenticationMethod);
                        if (!pluginIsActive)
                        {
                            var externalAuthenticationSettings = EngineContext.Current.Resolve<ExternalAuthenticationSettings>();
                            externalAuthenticationSettings.ActiveAuthenticationMethodSystemNames.Add(plugin.PluginDescriptor.SystemName);
                            await settingService.SaveSettingAsync(externalAuthenticationSettings);
                        }

                        break;
                    case IWidgetPlugin widgetPlugin:
                        var widgetPluginManager = EngineContext.Current.Resolve<IWidgetPluginManager>();
                        pluginIsActive = widgetPluginManager.IsPluginActive(widgetPlugin);
                        if (!pluginIsActive)
                        {
                            var widgetSettings = EngineContext.Current.Resolve<WidgetSettings>();
                            widgetSettings.ActiveWidgetSystemNames.Add(plugin.PluginDescriptor.SystemName);
                            await settingService.SaveSettingAsync(widgetSettings);
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                await EngineContext.Current.Resolve<ILogger>()
                      .ErrorAsync($"Failed to enable {plugin.PluginDescriptor.SystemName}: {ex.Message}", ex);
            }
        }
    }
}
