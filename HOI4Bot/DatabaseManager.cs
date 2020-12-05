using HOI4Bot.Databases;
using System.Threading.Tasks;

namespace HOI4Bot
{
    public static class DatabaseManager
    {
        public static readonly UserDatabase userDatabase = new UserDatabase();

        public static async Task InitAsync()
        {
            await Task.WhenAll(
                userDatabase.InitAsync()
            );
        }

        public static async Task CloseAsync()
        {
            await Task.WhenAll(
                userDatabase.CloseAsync()
            );
        }
    }
}