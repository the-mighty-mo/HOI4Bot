using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HOI4Bot.Databases.UserDatabaseTables
{
    public class UsersTable : ITable
    {
        private readonly SqliteConnection connection;

        public UsersTable(SqliteConnection connection) => this.connection = connection;

        public Task InitAsync()
        {
            using SqliteCommand cmd = new("CREATE TABLE IF NOT EXISTS Users (user_id TEXT NOT NULL, country TEXT NOT NULL, guild_id TEXT NOT NULL, UNIQUE(country, guild_id), UNIQUE(user_id, guild_id));", connection);
            return cmd.ExecuteNonQueryAsync();
        }

        public async Task AddUserAsync(string userId, string country, string guildId)
        {
            string update = "UPDATE Users SET country = @country WHERE user_id = @user_id AND guild_id = @guild_id;";
            string insert = "INSERT INTO Users (user_id, country, guild_id) SELECT @user_id, @country, @guild_id WHERE (SELECT Changes() = 0);";

            using SqliteCommand cmd = new(update + insert, connection);
            cmd.Parameters.AddWithValue("@user_id", userId);
            cmd.Parameters.AddWithValue("@country", country);
            cmd.Parameters.AddWithValue("@guild_id", guildId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RemoveUserAsync(string userId, string guildId)
        {
            string delete = "DELETE FROM Users WHERE user_id = @user_id AND guild_id = @guild_id;";

            using SqliteCommand cmd = new(delete, connection);
            cmd.Parameters.AddWithValue("@user_id", userId);
            cmd.Parameters.AddWithValue("@guild_id", guildId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> IsUserAsync(string userId, string guildId)
        {
            bool hasUser;

            string getUser = "SELECT * FROM Users WHERE user_id = @user_id AND guild_id = @guild_id;";

            using SqliteCommand cmd = new(getUser, connection);
            cmd.Parameters.AddWithValue("@user_id", userId);
            cmd.Parameters.AddWithValue("@guild_id", guildId);

            SqliteDataReader reader = await cmd.ExecuteReaderAsync();
            hasUser = await reader.ReadAsync();
            reader.Close();

            return hasUser;
        }

        public async Task ClearUserAsync(string guildId)
        {
            string delete = "DELETE FROM Users WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(delete, connection);
            cmd.Parameters.AddWithValue("@guild_id", guildId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<string>> GetCountriesAsync(string guildId)
        {
            List<string> countries = new();

            string getCountries = "SELECT country FROM Users WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(getCountries, connection);
            cmd.Parameters.AddWithValue("@guild_id", guildId);

            SqliteDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                string? country = reader["country"].ToString();
                if (country != null)
                {
                    countries.Add(country);
                }
            }

            return countries;
        }

        public async Task<Dictionary<string, string>> GetUsersAsync(string guildId)
        {
            Dictionary<string, string> users = new();

            string getUsers = "SELECT user_id, country FROM Users WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(getUsers, connection);
            cmd.Parameters.AddWithValue("@guild_id", guildId);

            SqliteDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                string? userId = reader["user_id"].ToString();
                string? country = reader["country"].ToString();
                if (userId != null && country != null)
                {
                    users.Add(userId, country);
                }
            }

            return users;
        }

        public async Task<string?> GetCountryAsync(string userId, string guildId)
        {
            string? country = default;

            string getCountry = "SELECT country FROM Users WHERE user_id = @user_id AND guild_id = @guild_id;";

            using SqliteCommand cmd = new(getCountry, connection);
            cmd.Parameters.AddWithValue("@user_id", userId);
            cmd.Parameters.AddWithValue("@guild_id", guildId);

            SqliteDataReader reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                country = reader["country"].ToString();
            }

            return country;
        }
    }
}
