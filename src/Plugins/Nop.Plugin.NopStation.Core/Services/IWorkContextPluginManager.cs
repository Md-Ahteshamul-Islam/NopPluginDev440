using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.NopStation.Core.Services;

namespace Nop.Plugin.NopStation.Core.Infrastructure
{
    public interface IWorkContextPluginManager
    {
        IList<IWorkContextPlugin> LoadWorkContextPlugins(Customer customer = null, string pluginSystemName = "",
            int storeId = 0);
    }
}