using System.Threading.Tasks;
using HOI4Bot.Databases;

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
