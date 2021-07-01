using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;
using System;

namespace Nop.Plugin.NopStation.NopChat.Models
{
    public record ChatListModel
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? LastMesageDate { get; set; }
        public int NumberOfMessages { get; set; }
    }
}
