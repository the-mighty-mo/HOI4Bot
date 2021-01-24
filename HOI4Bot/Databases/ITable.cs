using System.Threading.Tasks;

namespace HOI4Bot.Databases
{
    interface ITable
    {
        public Task InitAsync();
    }
}
