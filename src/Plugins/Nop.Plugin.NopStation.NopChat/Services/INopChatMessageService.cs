using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.NopStation.NopChat;
using Nop.Plugin.NopStation.NopChat.Domains;
using Nop.Plugin.NopStation.NopChat.Models;

namespace Nop.Plugin.NopStation.NopChat.Services
{
    
    public partial interface INopChatMessageService
    {

        Task<IPagedList<NopChatMessage>> GetAllAsync(int pageIndex = 0, int pageSize = int.MaxValue);
        Task<NopChatMessage> GetByIdAsync(int id);
        Task<IPagedList<NopChatMessage>> GetByCustomerIdAsync(int customerId, int pageIndex = 0, int pageSize = int.MaxValue);
        Task<IPagedList<NopChatMessage>> GetByVendorIdAsync(int vendorId, int pageIndex = 0, int pageSize = int.MaxValue);
        Task<IPagedList<NopChatMessage>> GetByVendorCustomerIdAsync(int vendorCustomerId, int pageIndex = 0, int pageSize = int.MaxValue);
        Task<IList<ChatListModel>> GetCustomerChatListListAsync(int customerId);
        Task<IList<ChatListModel>> GetVendorChatListListAsync(int vendorId);
        Task InsertNopChatMessageAsync(NopChatMessage nopChatMessage);
        Task UpdateNopChatMessageAsync(NopChatMessage nopChatMessage);
        Task DeleteNopChatMessageAsync(NopChatMessage nopChatMessage);
        //Task<IList<NopChatMessage>> GetChatHistoryAsync(int customerId, int vendorId);
        Task<IList<NopChatMessageModel>> GetChatHistoryAsync(int customerId, int vendorId);
        Task<string> GetVendorNameByCustomerNameIfExxistAsync(string customerName);
    }
}
