using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HOI4Bot.Databases.UserDatabaseTables
{
    public class RolesTable : ITable
    {
        private readonly SqliteConnection connection;

        public RolesTable(SqliteConnection connection) => this.connection = connection;

        public Task InitAsync()
        {
            using SqliteCommand cmd = new("CREATE TABLE IF NOT EXISTS Roles (country TEXT NOT NULL, role_id TEXT NOT NULL, guild_id TEXT NOT NULL, UNIQUE(country, guild_id));", connection);
            return cmd.ExecuteNonQueryAsync();
        }

        public async Task AddRoleAsync(string country, string roleId, string guildId)
        {
            string update = "UPDATE Roles SET role_id = @role_id WHERE country = @country AND guild_id = @guild_id;";
            string insert = "INSERT INTO Roles (country, role_id, guild_id) SELECT @country, @role_id, @guild_id WHERE (SELECT Changes() = 0);";

            using SqliteCommand cmd = new(update + insert, connection);
            cmd.Parameters.AddWithValue("@country", country);
            cmd.Parameters.AddWithValue("@role_id", roleId);
            cmd.Parameters.AddWithValue("@guild_id", guildId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RemoveRoleAsync(string country, string guildId)
        {
            string delete = "DELETE FROM Roles WHERE country = @country AND guild_id = @guild_id;";

            using SqliteCommand cmd = new(delete, connection);
            cmd.Parameters.AddWithValue("@country", country);
            cmd.Parameters.AddWithValue("@guild_id", guildId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> IsCountryAsync(string country, string guildId)
        {
            bool hasCountry;

            string getCountry = "SELECT * FROM Roles WHERE country = @country AND guild_id = @guild_id;";

            using SqliteCommand cmd = new(getCountry, connection);
            cmd.Parameters.AddWithValue("@country", country);
            cmd.Parameters.AddWithValue("@guild_id", guildId);

            SqliteDataReader reader = await cmd.ExecuteReaderAsync();
            hasCountry = await reader.ReadAsync();

            return hasCountry;
        }

        public async Task<string> GetRoleAsync(string country, string guildId)
        {
            string roleId = null;

            string getRole = "SELECT role_id FROM Roles WHERE country = @country AND guild_id = @guild_id;";

            using SqliteCommand cmd = new(getRole, connection);
            cmd.Parameters.AddWithValue("@country", country);
            cmd.Parameters.AddWithValue("@guild_id", guildId);

            SqliteDataReader reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                roleId = reader["role_id"].ToString();
            }

            return roleId;
        }

        public async Task<Dictionary<string, string>> GetRolesAsync(string guildId)
        {
            Dictionary<string, string> roles = new();

            string getRoles = "SELECT * FROM Roles WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(getRoles, connection);
            cmd.Parameters.AddWithValue("@guild_id", guildId);

            SqliteDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                roles.Add(reader["country"].ToString(), reader["role_id"].ToString());
            }

            return roles;
        }
    }
}
