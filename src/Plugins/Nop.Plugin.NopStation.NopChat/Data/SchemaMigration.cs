using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.NopStation.NopChat.Domains;

namespace Nop.Plugin.NopStation.NopChat.Data
{
    [SkipMigrationOnUpdate]
    [NopMigration("2021/06/16 08:40:55:1687547", "NopStation.NopChat base schema")]
    public class SchemaMigration : AutoReversingMigration
    {
        protected IMigrationManager _migrationManager;

        public SchemaMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            _migrationManager.BuildTable<NopChatMessage>(Create);
        }
    }
}
