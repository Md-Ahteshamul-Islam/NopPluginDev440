using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.NopStation.NopChat.Domains;

namespace Nop.Plugin.NopStation.NopChat.Data
{
    public class NopChatMessageBuilder : NopEntityBuilder<NopChatMessage>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(NopChatMessage.Text))
                .AsString(500)
                .Nullable()
                .WithColumn(nameof(NopChatMessage.DateCreated))
                .AsDate()
                .Nullable()
                .WithColumn(nameof(NopChatMessage.CustomerId))
                .AsInt32()
                .Nullable()
                .WithColumn(nameof(NopChatMessage.VendorId))
                .AsInt32()
                .Nullable()
                .WithColumn(nameof(NopChatMessage.VendorCustomerId))
                .AsInt32()
                .Nullable()
                .WithColumn(nameof(NopChatMessage.IsVendorResponse))
                .AsBoolean()
                .Nullable()
                .WithColumn(nameof(NopChatMessage.IsChecked))
                .AsBoolean()
                .Nullable();
        }
    }
}