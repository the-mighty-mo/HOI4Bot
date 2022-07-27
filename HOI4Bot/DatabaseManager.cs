using HOI4Bot.Databases;
using System.Threading.Tasks;

namespace HOI4Bot
{
    public static class DatabaseManager
    {
        public static readonly UserDatabase userDatabase = new();

        public static Task InitAsync() =>
            Task.WhenAll(
                userDatabase.InitAsync()
            );

        public static Task CloseAsync() =>
            Task.WhenAll(
                userDatabase.CloseAsync()
            );
    }
}