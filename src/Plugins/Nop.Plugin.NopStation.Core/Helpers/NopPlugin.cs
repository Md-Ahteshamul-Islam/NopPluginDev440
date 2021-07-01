using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Infrastructure;
using Nop.Services.Authentication.External;
using Nop.Services.Cms;
using Nop.Services.Logging;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Pickup;
using Nop.Services.Plugins;
using Nop.Core;

namespace Nop.Plugin.NopStation.Core.Helpers
{
    public static class NopPlugin
    {
        public static async Task<bool> IsActive<TPlugin>(this string systemName) where TPlugin : class, IPlugin
        {
            try
            {
                var pluginService = EngineContext.Current.Resolve<IPluginService>();
                var storeContext = EngineContext.Current.Resolve<IStoreContext>();
                var workContext = EngineContext.Current.Resolve<IWorkContext>();

                var pluginDescriptor = await pluginService.GetPluginDescriptorBySystemNameAsync<IWidgetPlugin>(systemName,
                    LoadPluginsMode.InstalledOnly, await workContext.GetCurrentCustomerAsync(), storeContext.GetCurrentStore().Id);

                switch (pluginDescriptor)
                {
                    case IPaymentMethod paymentMethod:
                        var paymentPluginManager = EngineContext.Current.Resolve<IPaymentPluginManager>();
                        return paymentPluginManager.IsPluginActive(paymentMethod);
                    case IShippingRateComputationMethod shippingRateComputationMethod:
                        var shippingPluginManager = EngineContext.Current.Resolve<IShippingPluginManager>();
                        return shippingPluginManager.IsPluginActive(shippingRateComputationMethod);
                    case IPickupPointProvider pickupPointProvider:
                        var pickupPluginManager = EngineContext.Current.Resolve<IPickupPluginManager>();
                        return pickupPluginManager.IsPluginActive(pickupPointProvider);
                    case IExternalAuthenticationMethod externalAuthenticationMethod:
                        var authenticationPluginManager = EngineContext.Current.Resolve<IAuthenticationPluginManager>();
                        return authenticationPluginManager.IsPluginActive(externalAuthenticationMethod);
                    case IWidgetPlugin widgetPlugin:
                        var widgetPluginManager = EngineContext.Current.Resolve<IWidgetPluginManager>();
                        return widgetPluginManager.IsPluginActive(widgetPlugin);
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                await EngineContext.Current.Resolve<ILogger>().ErrorAsync($"Failed to check {systemName}: {ex.Message}", ex);
                return false;
            }
        }
    }
}
