using Discord;
using Discord.Interactions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HOI4Bot.Modules
{
    public class Help : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("help", "List of commands")]
        public async Task HelpAsync()
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle(SecurityInfo.botName);

            List<EmbedFieldBuilder> fields = new();

            EmbedFieldBuilder commands = new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName("Commands")
                .WithValue(
                    "ping\n" +
                    "  - Returns the bot's Server and API latencies\n\n" +
                    "countries\n" +
                    "  - Displays a list of valid countries\n\n" +
                    "opt-in [user mention (admin only)]\n" +
                    "  - Joins the next war\n\n" +
                    "opt-out [user mention (admin only)]\n" +
                    "  - Leaves the next war\n\u200b"
                );
            fields.Add(commands);

            EmbedFieldBuilder admin = new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName("Admin Only")
                .WithValue(
                    "add-role [role mention] [country]\n" +
                    "  - Sets the role for the country or the Victory role\n\n" +
                    "remove-role [country]\n" +
                    "  - Removes any role assigned to the country or the Victory role\n\n" +
                    "view-roles\n" +
                    "  - Views the roles currently used for countries and the Victory role\n\n" +
                    "victory [alliance]\n" +
                    "  - Gives the winning alliance the Victory role\n\n" +
                    "clear-victory\n" +
                    "  - Removes the Victory role from all members\n\n" +
                    "new-war\n" +
                    "  - Creates a new war\n\n" +
                    "view-war\n" +
                    "  - Displays the war info\n\n" +
                    "assign [user mention] [country]\n" +
                    "  - Assigns a user to a specific country"
                );
            fields.Add(admin);
            embed.WithFields(fields);

            await Context.Interaction.RespondAsync(embed: embed.Build(), ephemeral: true);
        }
    }
}