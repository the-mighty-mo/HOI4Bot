using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace HOI4Bot.Databases.UserDatabaseTables
{
    public class SurrenderTable : ITable
    {
        private readonly SqliteConnection connection;

        public SurrenderTable(SqliteConnection connection) => this.connection = connection;

        public Task InitAsync()
        {
            using SqliteCommand cmd = new("CREATE TABLE IF NOT EXISTS Surrender (country TEXT NOT NULL, guild_id TEXT NOT NULL, UNIQUE(country, guild_id));", connection);
            return cmd.ExecuteNonQueryAsync();
        }
    }
}
