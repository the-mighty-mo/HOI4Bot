using Discord.Commands;
using System.Threading.Tasks;

namespace HOI4Bot.Modules
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        public async Task HelpAsync() =>
            await Context.Channel.SendMessageAsync(
                "**Hearts of Randomizer**\n" +
                $"Prefix: {Context.Client.CurrentUser.Mention} or \\\\\n" +
                "\n" +
                "Commands:\n" +
                "help\n" +
                "  - Displays this help message\n" +
                "countries\n" +
                "  - Displays a list of valid countries\n" +
                "opt-in [user mention (admin only)]\n" +
                "  - Joins the next war\n" +
                "opt-out [user mention (admin only)]\n" +
                "  - Leaves the next war\n" +
                "\n" +
                "Admin Only:\n" +
                "add-role [role mention] [country]\n" +
                "  - Sets the role for the country or the Victory role\n" +
                "remove-role [country]\n" +
                "  - Removes any role assigned to the country or the Victory role\n" +
                "victory [alliance]\n" +
                "  - Gives the winning alliance the Victory role\n" +
                "clear-victory\n" +
                "  - Removes the Victory role from all members\n" +
                "new-war\n" +
                "  - Creates a new war\n" +
                "view-war\n" +
                "  - Displays the war info\n" +
                "assign [user mention] [country]\n" +
                "  - Assigns a user to a specific country"
            );
    }
}
