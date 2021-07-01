using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using Nop.Plugin.NopStation.NopChat.Domains;

namespace Nop.Plugin.NopStation.NopChat.Data
{
    public class BaseNameCompatibility: INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(NopChatMessage), "NS_NopChatMessage" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {
        };
    }
}
