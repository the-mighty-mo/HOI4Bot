using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static HOI4Bot.Constants;
using static HOI4Bot.DatabaseManager;

namespace HOI4Bot.Modules
{
    public class Users : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("countries", "Displays a list of valid countries")]
        public async Task CountriesAsync()
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Countries");

            string allies = "";
            foreach (string country in majorAllies)
            {
                allies += $"- {country}\n";
            }
            foreach (string country in mijorAllies)
            {
                allies += $"- {country}\n";
            }
            allies += "**Minor Allies:**\n";
            foreach (string country in minorAllies)
            {
                allies += $"- {country}\n";
            }

            EmbedFieldBuilder allyField = new EmbedFieldBuilder()
                .WithIsInline(true)
                .WithName("Allies")
                .WithValue(allies);
            embed.AddField(allyField);

            string axis = "";
            foreach (string country in majorAxis)
            {
                axis += $"- {country}\n";
            }
            foreach (string country in mijorAxis)
            {
                axis += $"- {country}\n";
            }
            axis += "\n**Minor Axis:**\n";
            foreach (string country in minorAxis)
            {
                axis += $"- {country}\n";
            }

            EmbedFieldBuilder axisField = new EmbedFieldBuilder()
                .WithIsInline(true)
                .WithName("Axis")
                .WithValue(axis);
            embed.AddField(axisField);

            await Context.Interaction.RespondAsync(embed: embed.Build());
        }

        [SlashCommand("opt-in", "Joins the next war")]
        [RequireContext(ContextType.Guild)]
        public async Task OptInAsync() => await OptInAsync(Context.User);

        [SlashCommand("admin-opt-in", "Joins the next war")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task OptInAsync(SocketUser user)
        {
            user ??= Context.User;

            if (await userDatabase.Users.IsUserAsync(user.Id.ToString(), Context.Guild.Id.ToString()))
            {
                string? curCountry = await userDatabase.Users.GetCountryAsync(user.Id.ToString(), Context.Guild.Id.ToString());

                EmbedBuilder emb = new EmbedBuilder()
                    .WithColor(SecurityInfo.botColor)
                    .WithDescription($"You are already part of the next war as: {curCountry}.");

                await Context.Interaction.RespondAsync(embed: emb.Build());
                return;
            }
            IEnumerable<string> majorCountries = new List<string>().Concat(majorAllies).Concat(majorAxis);
            IEnumerable<string> mijorCountries = new List<string>().Concat(mijorAllies).Concat(mijorAxis);
            IEnumerable<string> minorCountries = new List<string>().Concat(minorAllies).Concat(minorAxis);

            List<string> usedCountries = await userDatabase.Users.GetCountriesAsync(Context.Guild.Id.ToString());
            List<string> availableCountries = majorCountries.Where(x => !usedCountries.Contains(x)).ToList();
            if (availableCountries.Count == 0)
            {
                availableCountries = mijorCountries.Where(x => !usedCountries.Contains(x)).ToList();
                if (availableCountries.Count == 0)
                {
                    availableCountries = minorCountries.Where(x => !usedCountries.Contains(x)).ToList();
                }
            }
            string country = availableCountries[random.Next(0, availableCountries.Count)];

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithDescription($"You have joined the next war as: {country}.");

            List<Task> cmds = new()
            {
                userDatabase.Users.AddUserAsync(user.Id.ToString(), country, Context.Guild.Id.ToString()),
                Context.Interaction.RespondAsync(embed: embed.Build())
            };

            SocketRole role;
            if (ulong.TryParse(await userDatabase.Roles.GetRoleAsync(country, Context.Guild.Id.ToString()), out ulong roleId) && (role = Context.Guild.GetRole(roleId)) != null)
            {
                cmds.Add(Context.Guild.GetUser(user.Id).AddRoleAsync(role));
            }

            await Task.WhenAll(cmds);
        }

        [SlashCommand("opt-out", "Leaves the next war")]
        [RequireContext(ContextType.Guild)]
        public async Task OptOutAsync() => await OptOutAsync(Context.User);

        [SlashCommand("admin-opt-out", "Leaves the next war")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task OptOutAsync(SocketUser user)
        {
            if (!await userDatabase.Users.IsUserAsync(user.Id.ToString(), Context.Guild.Id.ToString()))
            {
                EmbedBuilder emb = new EmbedBuilder()
                    .WithColor(SecurityInfo.botColor)
                    .WithDescription("You are already out of the next war.");

                await Context.Interaction.RespondAsync(embed: emb.Build());
                return;
            }

            IEnumerable<string> majorCountries = new List<string>().Concat(majorAllies).Concat(majorAxis);
            IEnumerable<string> mijorCountries = new List<string>().Concat(mijorAllies).Concat(mijorAxis);
            IEnumerable<string> minorCountries = new List<string>().Concat(minorAllies).Concat(minorAxis);

            string? country = await userDatabase.Users.GetCountryAsync(user.Id.ToString(), Context.Guild.Id.ToString());
            Dictionary<string, string> userCountries = await userDatabase.Users.GetUsersAsync(Context.Guild.Id.ToString());
            IEnumerable<string> userMinors = userCountries.Values.Where(x => minorCountries.Contains(x));

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithDescription("You have left the next war.");

            List<Task> cmds = new()
            {
                userDatabase.Users.RemoveUserAsync(user.Id.ToString(), Context.Guild.Id.ToString()),
                Context.Interaction.RespondAsync(embed: embed.Build())
            };

            SocketRole role;
            if (country != null && ulong.TryParse(await userDatabase.Roles.GetRoleAsync(country, Context.Guild.Id.ToString()), out ulong roleId) && (role = Context.Guild.GetRole(roleId)) != null)
            {
                cmds.Add(Context.Guild.GetUser(user.Id).RemoveRoleAsync(role));
            }

            await Task.WhenAll(cmds);

            if (country != null && (majorCountries.Contains(country) || mijorCountries.Contains(country)) && userMinors.Any())
            {
                SocketGuildUser? newUser = null;
                do
                {
                    int i = userCountries.Values.ToList().IndexOf(userMinors.ElementAt(random.Next(0, userMinors.Count())));
                    string userId = userCountries.Keys.ElementAt(i);
                    if (ulong.TryParse(userId, out ulong newUserId))
                    {
                        newUser = Context.Guild.GetUser(newUserId);
                    }
                }
                while (newUser == null);

                SocketRole oldRole;
                if (ulong.TryParse(await userDatabase.Roles.GetRoleAsync(country, Context.Guild.Id.ToString()), out ulong oldRoleId) && (oldRole = Context.Guild.GetRole(oldRoleId)) != null)
                {
                    await Context.Guild.GetUser(newUser.Id).RemoveRoleAsync(oldRole);
                }

                EmbedBuilder takeover = new EmbedBuilder()
                    .WithColor(SecurityInfo.botColor)
                    .WithDescription($"{newUser.Mention} has taken over as {country}.");

                await Task.WhenAll
                (
                    Context.Interaction.RespondAsync(embed: takeover.Build()),
                    userDatabase.Users.AddUserAsync(newUser.Id.ToString(), country, Context.Guild.Id.ToString())
                );

                SocketRole newRole;
                if (ulong.TryParse(await userDatabase.Roles.GetRoleAsync(country, Context.Guild.Id.ToString()), out ulong newRoleId) && (newRole = Context.Guild.GetRole(newRoleId)) != null)
                {
                    await Context.Guild.GetUser(newUser.Id).AddRoleAsync(newRole);
                }
            }
        }

        //[Command("surrender")]
        //public async Task SurrenderAsync()
        //{
        //}

        //[Command("unsurrender")]
        //public async Task UnsurrenderAsync()
        //{
        //}
    }
}