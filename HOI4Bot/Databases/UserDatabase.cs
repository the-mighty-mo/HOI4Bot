using HOI4Bot.Databases.UserDatabaseTables;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HOI4Bot.Databases
{
    public class UserDatabase
    {
        private readonly SqliteConnection connection = new SqliteConnection("Filename=Users.db");
        private readonly Dictionary<System.Type, ITable> tables = new Dictionary<System.Type, ITable>();

        public UsersTable Users => tables[typeof(UsersTable)] as UsersTable;
        public RolesTable Roles => tables[typeof(RolesTable)] as RolesTable;
        public SurrenderTable Surrender => tables[typeof(SurrenderTable)] as SurrenderTable;

        public UserDatabase()
        {
            tables.Add(typeof(UsersTable), new UsersTable(connection));
            tables.Add(typeof(RolesTable), new RolesTable(connection));
            tables.Add(typeof(SurrenderTable), new SurrenderTable(connection));
        }

        public async Task InitAsync()
        {
            await connection.OpenAsync();
            IEnumerable<Task> GetTableInits()
            {
                foreach (var table in tables.Values)
                {
                    yield return table.InitAsync();
                }
            }
            await Task.WhenAll(GetTableInits());
        }

        public async Task CloseAsync() => await connection.CloseAsync();
    }
}