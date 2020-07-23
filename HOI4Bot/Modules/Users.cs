using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static HOI4Bot.Constants;

namespace HOI4Bot.Modules
{
    public class Users : ModuleBase<SocketCommandContext>
    {
        [Command("countries")]
        public async Task CountriesAsync()
        {
            string countries = "Allies:\n";
            foreach (string country in majorAllies) {
                countries += $"- {country}\n";
            }
            foreach (string country in mijorAllies)
            {
                countries += $"- {country}\n";
            }
            countries += "*Minor Allies:*\n";
            foreach (string country in minorAllies)
            {
                countries += $"- {country}\n";
            }
            countries += "Axis:\n";
            foreach (string country in majorAxis)
            {
                countries += $"- {country}\n";
            }
            foreach (string country in mijorAxis)
            {
                countries += $"- {country}\n";
            }
            countries += "*Minor Axis:*\n";
            foreach (string country in minorAxis)
            {
                countries += $"- {country}\n";
            }

            await Context.Channel.SendMessageAsync(countries);
        }

        [Command("opt-in")]
        public async Task OptInAsync() => await OptInAsync(Context.User);

        [Command("opt-in")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task OptInAsync(SocketUser user)
        {
            string country;
            if (await SQLCommands.IsUserAsync(user.Id.ToString(), Context.Guild.Id.ToString()))
            {
                country = await SQLCommands.GetCountryAsync(user.Id.ToString(), Context.Guild.Id.ToString());
                await Context.Channel.SendMessageAsync($"You are already part of the next war as: {country}.");
                return;
            }
            List<string> majorCountries = new List<string>().Concat(majorAllies).Concat(majorAxis).ToList();
            List<string> mijorCountries = new List<string>().Concat(mijorAllies).Concat(mijorAxis).ToList();
            List<string> minorCountries = new List<string>().Concat(minorAllies).Concat(minorAxis).ToList();

            List<string> usedCountries = await SQLCommands.GetCountriesAsync(Context.Guild.Id.ToString());
            List<string> availableCountries = majorCountries.Where(x => !usedCountries.Contains(x)).ToList();
            if (availableCountries.Count == 0)
            {
                availableCountries = mijorCountries.Where(x => !usedCountries.Contains(x)).ToList();
                if (availableCountries.Count == 0)
                {
                    availableCountries = minorCountries.Where(x => !usedCountries.Contains(x)).ToList();
                }
            }
            country = availableCountries[random.Next(0, availableCountries.Count)];

            List<Task> cmds = new List<Task>()
            {
                SQLCommands.AddUserAsync(user.Id.ToString(), country, Context.Guild.Id.ToString()),
                Context.Channel.SendMessageAsync($"You have joined the next war as: {country}.")
            };

            SocketRole role;
            if (ulong.TryParse(await SQLCommands.GetRoleAsync(country, Context.Guild.Id.ToString()), out ulong roleId) && (role = Context.Guild.GetRole(roleId)) != null)
            {
                cmds.Add(Context.Guild.GetUser(user.Id).AddRoleAsync(role));
            }

            await Task.WhenAll(cmds);
        }

        [Command("opt-out")]
        public async Task OptOutAsync() => await OptOutAsync(Context.User);

        [Command("opt-out")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task OptOutAsync(SocketUser user)
        {
            if (!await SQLCommands.IsUserAsync(user.Id.ToString(), Context.Guild.Id.ToString()))
            {
                await Context.Channel.SendMessageAsync("You are already out of the next war.");
                return;
            }

            List<string> majorCountries = new List<string>().Concat(majorAllies).Concat(majorAxis).ToList();
            List<string> mijorCountries = new List<string>().Concat(mijorAllies).Concat(mijorAxis).ToList();
            List<string> minorCountries = new List<string>().Concat(minorAllies).Concat(minorAxis).ToList();

            string country = await SQLCommands.GetCountryAsync(user.Id.ToString(), Context.Guild.Id.ToString());
            Dictionary<string, string> userCountries = await SQLCommands.GetUsersAsync(Context.Guild.Id.ToString());
            IEnumerable<string> userMinors = userCountries.Values.Where(x => minorCountries.Contains(x));

            List<Task> cmds = new List<Task>()
            {
                SQLCommands.RemoveUserAsync(user.Id.ToString(), Context.Guild.Id.ToString()),
                Context.Channel.SendMessageAsync("You have left the next war.")
            };

            SocketRole role;
            if (ulong.TryParse(await SQLCommands.GetRoleAsync(country, Context.Guild.Id.ToString()), out ulong roleId) && (role = Context.Guild.GetRole(roleId)) != null)
            {
                cmds.Add(Context.Guild.GetUser(user.Id).RemoveRoleAsync(role));
            }

            await Task.WhenAll(cmds);

            if ((majorCountries.Contains(country) || mijorCountries.Contains(country)) && userMinors.Count() > 0)
            {
                SocketGuildUser newUser = null;
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
                if (ulong.TryParse(await SQLCommands.GetRoleAsync(country, Context.Guild.Id.ToString()), out ulong oldRoleId) && (oldRole = Context.Guild.GetRole(oldRoleId)) != null)
                {
                    await Context.Guild.GetUser(newUser.Id).RemoveRoleAsync(oldRole);
                }

                await Task.WhenAll
                (
                    Context.Channel.SendMessageAsync($"{newUser.Mention} has taken over as {country}."),
                    SQLCommands.AddUserAsync(newUser.Id.ToString(), country, Context.Guild.Id.ToString())
                );

                SocketRole newRole;
                if (ulong.TryParse(await SQLCommands.GetRoleAsync(country, Context.Guild.Id.ToString()), out ulong newRoleId) && (newRole = Context.Guild.GetRole(newRoleId)) != null)
                {
                    await Context.Guild.GetUser(newUser.Id).AddRoleAsync(newRole);
                }
            }
        }

        [Command("surrender")]
        public async Task SurrenderAsync()
        {

        }

        [Command("unsurrender")]
        public async Task UnsurrenderAsync()
        {

        }
    }
}
