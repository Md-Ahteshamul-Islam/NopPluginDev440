using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.NopStation.NopChat.Factories;
using Nop.Plugin.NopStation.NopChat.Hubs;
using Nop.Plugin.NopStation.NopChat.Services;

namespace Nop.Plugin.NopStation.NopChat.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public int Order => 1;

        public void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            services.AddScoped<NopChatHub, NopChatHub>();
            services.AddScoped<INopChatMessageService, NopChatMessageService>();
            
            services.AddScoped<INopChatMessageModelFactory, NopChatMessageModelFactory>();
        }
    }
}
