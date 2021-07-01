using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Plugin.NopStation.Core.Services;
using Nop.Services.Customers;
using Nop.Services.Plugins;

namespace Nop.Plugin.NopStation.Core.Infrastructure
{
    public class WorkContextPluginManager : PluginManager<IWorkContextPlugin>, IWorkContextPluginManager
    {
        #region Fields

        private readonly NopStationCoreSettings _nopStationCoreSettings;

        #endregion

        #region Ctor

        public WorkContextPluginManager(NopStationCoreSettings nopStationCoreSettings,
            IPluginService pluginService,
            ICustomerService customerService) : base(customerService, pluginService)
        {
            _nopStationCoreSettings = nopStationCoreSettings;
        }

        #endregion

        public virtual IList<IWorkContextPlugin> LoadWorkContextPlugins(Customer customer = null, string pluginSystemName = "", 
            int storeId = 0)
        {
            var systemNames = !string.IsNullOrWhiteSpace(pluginSystemName) ? new List<string> { pluginSystemName } :
                _nopStationCoreSettings.ActiveNopStationSystemNames;

            var workContextPlugins = LoadActivePluginsAsync(systemNames, customer, storeId).Result;
            return workContextPlugins;
        }
    }
}
