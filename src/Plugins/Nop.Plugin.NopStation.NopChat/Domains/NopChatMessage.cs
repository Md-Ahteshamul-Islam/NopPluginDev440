using System;
using Nop.Core;

namespace Nop.Plugin.NopStation.NopChat.Domains
{
    /// <summary>
    /// Represents a shipping by weight record
    /// </summary>
    public partial class NopChatMessage : BaseEntity
    {
        public string Text { get; set; }
        public DateTime? DateCreated { get; set; }
        public int CustomerId { get; set; }
        public int VendorCustomerId { get; set; }
        public int VendorId { get; set; }
        public bool IsVendorResponse { get; set; }
        public bool IsChecked { get; set; }

    }
}