using System.Threading.Tasks;
using Nop.Plugin.NopStation.NopChat.Domains;
using Nop.Plugin.NopStation.NopChat.Models;

namespace Nop.Plugin.NopStation.NopChat.Factories
{
    public partial interface INopChatMessageModelFactory
    {
        Task<NopChatMessageModel> PrepareNopChatMessageModelAsync(NopChatMessage data);
    }
}
