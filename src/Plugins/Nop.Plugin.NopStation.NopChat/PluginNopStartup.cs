using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.NopStation.Core.Infrastructure;
using Nop.Plugin.NopStation.NopChat.Hubs;
using System;

namespace Nop.Plugin.NopStation.NopChat
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSignalR();
        }

        public void Configure(IApplicationBuilder application)
        {
            application.UseEndpoints(routes =>
            {
                routes.MapHub<NopChatHub>("/nopChatHub");
            });
        }

        public int Order => 1000; //UseEndpoints should be loaded last
    }
}
