using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HOI4Bot.Databases
{
    public class UserDatabase
    {
        private readonly SqliteConnection cnUsers = new SqliteConnection("Filename=Users.db");

        public UsersTable Users;
        public RolesTable Roles;
        public SurrenderTable Surrender;

        public UserDatabase()
        {
            Users = new UsersTable(cnUsers);
            Roles = new RolesTable(cnUsers);
            Surrender = new SurrenderTable(cnUsers);
        }

        public async Task InitAsync()
        {
            await cnUsers.OpenAsync();

            List<Task> cmds = new List<Task>();
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS Users (user_id TEXT NOT NULL, country TEXT NOT NULL, guild_id TEXT NOT NULL, UNIQUE(country, guild_id), UNIQUE(user_id, guild_id));", cnUsers))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS Roles (country TEXT NOT NULL, role_id TEXT NOT NULL, guild_id TEXT NOT NULL, UNIQUE(country, guild_id));", cnUsers))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS Surrender (country TEXT NOT NULL, guild_id TEXT NOT NULL, UNIQUE(country, guild_id));", cnUsers))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }

            await Task.WhenAll(cmds);
        }

        public async Task CloseAsync() => await cnUsers.CloseAsync();

        public class UsersTable
        {
            private readonly SqliteConnection cnUsers;

            public UsersTable(SqliteConnection cnUsers) => this.cnUsers = cnUsers;

            public async Task AddUserAsync(string userId, string country, string guildId)
            {
                string update = "UPDATE Users SET country = @country WHERE user_id = @user_id AND guild_id = @guild_id;";
                string insert = "INSERT INTO Users (user_id, country, guild_id) SELECT @user_id, @country, @guild_id WHERE (SELECT Changes() = 0);";
                using (SqliteCommand cmd = new SqliteCommand(update + insert, cnUsers))
                {
                    cmd.Parameters.AddWithValue("@user_id", userId);
                    cmd.Parameters.AddWithValue("@country", country);
                    cmd.Parameters.AddWithValue("@guild_id", guildId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task RemoveUserAsync(string userId, string guildId)
            {
                string delete = "DELETE FROM Users WHERE user_id = @user_id AND guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(delete, cnUsers))
                {
                    cmd.Parameters.AddWithValue("@user_id", userId);
                    cmd.Parameters.AddWithValue("@guild_id", guildId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task<bool> IsUserAsync(string userId, string guildId)
            {
                bool hasUser = false;

                string getUser = "SELECT * FROM Users WHERE user_id = @user_id AND guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(getUser, cnUsers))
                {
                    cmd.Parameters.AddWithValue("@user_id", userId);
                    cmd.Parameters.AddWithValue("@guild_id", guildId);

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    hasUser = await reader.ReadAsync();
                    reader.Close();
                }

                return hasUser;
            }

            public async Task ClearUserAsync(string guildId)
            {
                string delete = "DELETE FROM Users WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(delete, cnUsers))
                {
                    cmd.Parameters.AddWithValue("@guild_id", guildId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task<List<string>> GetCountriesAsync(string guildId)
            {
                List<string> countries = new List<string>();

                string getCountries = "SELECT country FROM Users WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(getCountries, cnUsers))
                {
                    cmd.Parameters.AddWithValue("@guild_id", guildId);

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        countries.Add(reader["country"].ToString());
                    }
                }

                return countries;
            }

            public async Task<Dictionary<string, string>> GetUsersAsync(string guildId)
            {
                Dictionary<string, string> users = new Dictionary<string, string>();

                string getUsers = "SELECT user_id, country FROM Users WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(getUsers, cnUsers))
                {
                    cmd.Parameters.AddWithValue("@guild_id", guildId);

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        users.Add(reader["user_id"].ToString(), reader["country"].ToString());
                    }
                }

                return users;
            }

            public async Task<string> GetCountryAsync(string userId, string guildId)
            {
                string country = default;

                string getCountry = "SELECT country FROM Users WHERE user_id = @user_id AND guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(getCountry, cnUsers))
                {
                    cmd.Parameters.AddWithValue("@user_id", userId);
                    cmd.Parameters.AddWithValue("@guild_id", guildId);

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        country = reader["country"].ToString();
                    }
                }

                return country;
            }
        }

        public class RolesTable
        {
            private readonly SqliteConnection cnUsers;

            public RolesTable(SqliteConnection cnUsers) => this.cnUsers = cnUsers;

            public async Task AddRoleAsync(string country, string roleId, string guildId)
            {
                string update = "UPDATE Roles SET role_id = @role_id WHERE country = @country AND guild_id = @guild_id;";
                string insert = "INSERT INTO Roles (country, role_id, guild_id) SELECT @country, @role_id, @guild_id WHERE (SELECT Changes() = 0);";
                using (SqliteCommand cmd = new SqliteCommand(update + insert, cnUsers))
                {
                    cmd.Parameters.AddWithValue("@country", country);
                    cmd.Parameters.AddWithValue("@role_id", roleId);
                    cmd.Parameters.AddWithValue("@guild_id", guildId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task RemoveRoleAsync(string country, string guildId)
            {
                string delete = "DELETE FROM Roles WHERE country = @country AND guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(delete, cnUsers))
                {
                    cmd.Parameters.AddWithValue("@country", country);
                    cmd.Parameters.AddWithValue("@guild_id", guildId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task<bool> IsCountryAsync(string country, string guildId)
            {
                bool hasCountry = false;

                string getCountry = "SELECT * FROM Roles WHERE country = @country AND guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(getCountry, cnUsers))
                {
                    cmd.Parameters.AddWithValue("@country", country);
                    cmd.Parameters.AddWithValue("@guild_id", guildId);

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    hasCountry = await reader.ReadAsync();
                }

                return hasCountry;
            }

            public async Task<string> GetRoleAsync(string country, string guildId)
            {
                string roleId = null;

                string getRole = "SELECT role_id FROM Roles WHERE country = @country AND guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(getRole, cnUsers))
                {
                    cmd.Parameters.AddWithValue("@country", country);
                    cmd.Parameters.AddWithValue("@guild_id", guildId);

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        roleId = reader["role_id"].ToString();
                    }
                }

                return roleId;
            }

            public async Task<Dictionary<string, string>> GetRolesAsync(string guildId)
            {
                Dictionary<string, string> roles = new Dictionary<string, string>();

                string getRoles = "SELECT * FROM Roles WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(getRoles, cnUsers))
                {
                    cmd.Parameters.AddWithValue("@guild_id", guildId);

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        roles.Add(reader["country"].ToString(), reader["role_id"].ToString());
                    }
                }

                return roles;
            }
        }

        public class SurrenderTable
        {
            private readonly SqliteConnection cnUsers;

            public SurrenderTable(SqliteConnection cnUsers) => this.cnUsers = cnUsers;
        }
    }
}
