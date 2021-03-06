﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static HOI4Bot.Constants;
using static HOI4Bot.DatabaseManager;

namespace HOI4Bot.Modules
{
    public class Admin : ModuleBase<SocketCommandContext>
    {
        [Command("add-role")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AddRoleAsync(SocketRole role, [Remainder] string country)
        {
            if (country.ToLower() == "victory")
            {
                EmbedBuilder emb = new EmbedBuilder()
                    .WithColor(SecurityInfo.botColor)
                    .WithDescription($"{role.Mention} has been assigned as the Victory role.");

                await Task.WhenAll(
                    userDatabase.Roles.AddRoleAsync("Victory", role.Id.ToString(), Context.Guild.Id.ToString()),
                    Context.Channel.SendMessageAsync(embed: emb.Build())
                );
                return;
            }

            IEnumerable<string> countries = new List<string>().Concat(majorAllies).Concat(majorAxis).Concat(mijorAllies).Concat(mijorAxis).Concat(minorAllies).Concat(minorAxis);
            if (!countries.Contains(country))
            {
                EmbedBuilder emb = new EmbedBuilder()
                    .WithColor(SecurityInfo.botColor)
                    .WithDescription($"{country} is not a valid country.");

                await Context.Channel.SendMessageAsync(embed: emb.Build());
                return;
            }

            if (await userDatabase.Roles.GetRoleAsync(country, Context.Guild.Id.ToString()) == role.Id.ToString())
            {
                EmbedBuilder emb = new EmbedBuilder()
                    .WithColor(SecurityInfo.botColor)
                    .WithDescription($"{role.Mention} is already assigned to: {country}.");

                await Context.Channel.SendMessageAsync(embed: emb.Build());
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithDescription($"{role.Mention} has been assigned to: {country}.");

            await Task.WhenAll(
                userDatabase.Roles.AddRoleAsync(country, role.Id.ToString(), Context.Guild.Id.ToString()),
                Context.Channel.SendMessageAsync(embed: embed.Build())
            );
        }

        [Command("remove-role")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task RemoveRoleAsync([Remainder] string country)
        {
            if (country.ToLower() == "victory")
            {
                EmbedBuilder emb = new EmbedBuilder()
                    .WithColor(SecurityInfo.botColor)
                    .WithDescription($"There are now no roles assigned as the Victory role.");

                await Task.WhenAll(
                    userDatabase.Roles.RemoveRoleAsync("Victory", Context.Guild.Id.ToString()),
                    Context.Channel.SendMessageAsync(embed: emb.Build())
                );
                return;
            }

            IEnumerable<string> countries = new List<string>().Concat(majorAllies).Concat(majorAxis).Concat(mijorAllies).Concat(mijorAxis).Concat(minorAllies).Concat(minorAxis);
            if (!countries.Contains(country))
            {
                EmbedBuilder emb = new EmbedBuilder()
                    .WithColor(SecurityInfo.botColor)
                    .WithDescription($"{country} is not a valid country.");

                await Context.Channel.SendMessageAsync(embed: emb.Build());
                return;
            }

            if (!await userDatabase.Roles.IsCountryAsync(country, Context.Guild.Id.ToString()))
            {
                EmbedBuilder emb = new EmbedBuilder()
                    .WithColor(SecurityInfo.botColor)
                    .WithDescription($"There is no role assigned to {country}.");

                await Context.Channel.SendMessageAsync(embed: emb.Build());
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithDescription($"There are now no roles assigned to {country}.");

            await Task.WhenAll(
                userDatabase.Roles.RemoveRoleAsync(country, Context.Guild.Id.ToString()),
                Context.Channel.SendMessageAsync(embed: embed.Build())
            );
        }

        [Command("view-roles")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ViewRolesAsync()
        {
            string roleInfo = "";
            Dictionary<string, string> roles = await userDatabase.Roles.GetRolesAsync(Context.Guild.Id.ToString());

            bool first = true;
            foreach (string country in roles.Keys.Where(x => majorAllies.Contains(x)))
            {
                SocketRole role;
                if (ulong.TryParse(roles[country], out ulong roleId) && (role = Context.Guild.GetRole(roleId)) != null)
                {
                    if (first)
                    {
                        roleInfo += "Allies:\n";
                        first = false;
                    }
                    roleInfo += $"{role.Mention} - {country}\n";
                }
            }
            foreach (string country in roles.Keys.Where(x => mijorAllies.Contains(x)))
            {
                SocketRole role;
                if (ulong.TryParse(roles[country], out ulong roleId) && (role = Context.Guild.GetRole(roleId)) != null)
                {
                    roleInfo += $"{role.Mention} - {country}\n";
                }
            }
            first = true;

            foreach (string country in roles.Keys.Where(x => minorAllies.Contains(x)))
            {
                SocketRole role;
                if (ulong.TryParse(roles[country], out ulong roleId) && (role = Context.Guild.GetRole(roleId)) != null)
                {
                    if (first)
                    {
                        roleInfo += "*Minor Allies:*\n";
                        first = false;
                    }
                    roleInfo += $"{role.Mention} - {country}\n";
                }
            }
            first = true;

            foreach (string country in roles.Keys.Where(x => x != "iTaLY" && majorAxis.Contains(x)))
            {
                SocketRole role;
                if (ulong.TryParse(roles[country], out ulong roleId) && (role = Context.Guild.GetRole(roleId)) != null)
                {
                    if (first)
                    {
                        roleInfo += "Axis:\n";
                        first = false;
                    }
                    roleInfo += $"{role.Mention} - {country}\n";
                }
            }
            foreach (string country in roles.Keys.Where(x => mijorAxis.Contains(x)))
            {
                SocketRole role;
                if (ulong.TryParse(roles[country], out ulong roleId) && (role = Context.Guild.GetRole(roleId)) != null)
                {
                    roleInfo += $"{role.Mention} - {country}\n";
                }
            }
            first = true;

            if (roles.Keys.Contains("iTaLY"))
            {
                SocketRole role;
                if (ulong.TryParse(roles["iTaLY"], out ulong roleId) && (role = Context.Guild.GetRole(roleId)) != null)
                {
                    roleInfo += "iTaLY:\n" +
                        $"{role.Mention}\n";
                }
            }

            foreach (string country in roles.Keys.Where(x => minorAxis.Contains(x)))
            {
                SocketRole role;
                if (ulong.TryParse(roles[country], out ulong roleId) && (role = Context.Guild.GetRole(roleId)) != null)
                {
                    if (first)
                    {
                        roleInfo += "*Minor Axis:*\n";
                        first = false;
                    }
                    roleInfo += $"{role.Mention} - {country}\n";
                }
            }
            first = true;

            if (roles.Keys.Contains("Victory"))
            {
                SocketRole role;
                if (ulong.TryParse(roles["Victory"], out ulong roleId) && (role = Context.Guild.GetRole(roleId)) != null)
                {
                    roleInfo += "\n" +
                        $"Victory Role: {role.Mention}";
                }
            }

            if (roleInfo.Length == 0)
            {
                roleInfo = "No roles have been set up.";
            }
            await Context.Channel.SendMessageAsync(roleInfo);
        }

        [Command("victory")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task VictoryAsync(string alliance)
        {
            string roleStr = await userDatabase.Roles.GetRoleAsync("Victory", Context.Guild.Id.ToString());
            SocketRole role;
            if (!ulong.TryParse(roleStr, out ulong roleId) || (role = Context.Guild.GetRole(roleId)) == null)
            {
                EmbedBuilder embed = new EmbedBuilder()
                    .WithColor(SecurityInfo.botColor)
                    .WithDescription("No role has been assigned as the Victory role.");

                await Context.Channel.SendMessageAsync(embed: embed.Build());
                return;
            }

            if (alliance.ToLower() == "allies")
            {
                EmbedBuilder embed = new EmbedBuilder()
                    .WithColor(SecurityInfo.botColor)
                    .WithDescription("Victory to the Allies!");

                IEnumerable<string> allies = new List<string>().Concat(majorAllies).Concat(mijorAllies).Concat(minorAllies);
                List<Task> cmds = new()
                {
                    Context.Channel.SendMessageAsync(embed: embed.Build())
                };

                Dictionary<string, string> users = await userDatabase.Users.GetUsersAsync(Context.Guild.Id.ToString());
                foreach (string key in users.Where(x => allies.Contains(x.Value)).ToDictionary(x => x.Key).Keys)
                {
                    SocketGuildUser user;
                    if (ulong.TryParse(key, out ulong userId) && (user = Context.Guild.GetUser(userId)) != null)
                    {
                        cmds.Add(user.AddRoleAsync(role));
                    }
                }

                await Task.WhenAll(cmds);
            }
            else if (alliance.ToLower() == "axis")
            {
                EmbedBuilder embed = new EmbedBuilder()
                    .WithColor(SecurityInfo.botColor)
                    .WithDescription("Victory to the Axis!");

                IEnumerable<string> axis = new List<string>().Concat(majorAxis).Concat(mijorAxis).Concat(minorAxis);
                List<Task> cmds = new()
                {
                    Context.Channel.SendMessageAsync(embed: embed.Build())
                };

                Dictionary<string, string> users = await userDatabase.Users.GetUsersAsync(Context.Guild.Id.ToString());
                foreach (string key in users.Where(x => axis.Contains(x.Value)).ToDictionary(x => x.Key).Keys)
                {
                    SocketGuildUser user;
                    if (ulong.TryParse(key, out ulong userId) && (user = Context.Guild.GetUser(userId)) != null)
                    {
                        cmds.Add(user.AddRoleAsync(role));
                    }
                }

                await Task.WhenAll(cmds);
            }
            else
            {
                EmbedBuilder embed = new EmbedBuilder()
                    .WithColor(SecurityInfo.botColor)
                    .WithDescription($"{alliance} is not a valid alliance.");

                await Context.Channel.SendMessageAsync(embed: embed.Build());
            }
        }

        [Command("clear-victory")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ClearVictoryAsync()
        {
            string roleStr = await userDatabase.Roles.GetRoleAsync("Victory", Context.Guild.Id.ToString());
            SocketRole role;
            if (!ulong.TryParse(roleStr, out ulong roleId) || (role = Context.Guild.GetRole(roleId)) == null)
            {
                EmbedBuilder emb = new EmbedBuilder()
                    .WithColor(SecurityInfo.botColor)
                    .WithDescription("No role has been assigned as the Victory role.");

                await Context.Channel.SendMessageAsync(embed: emb.Build());
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithDescription("The Victory role has been removed from all members.");

            List<Task> cmds = new()
            {
                Context.Channel.SendMessageAsync(embed: embed.Build())
            };
            foreach (SocketGuildUser user in Context.Guild.Users)
            {
                cmds.Add(user.RemoveRoleAsync(role));
            }

            await Task.WhenAll(cmds);
        }

        [Command("new-war")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task NewWarAsync()
        {
            List<Task> cmds = new();
            foreach (SocketGuildUser user in Context.Guild.Users)
            {
                string country = await userDatabase.Users.GetCountryAsync(user.Id.ToString(), Context.Guild.Id.ToString());
                if (country == default)
                {
                    continue;
                }

                SocketRole role;
                if (ulong.TryParse(await userDatabase.Roles.GetRoleAsync(country, Context.Guild.Id.ToString()), out ulong roleId) && (role = Context.Guild.GetRole(roleId)) != null)
                {
                    cmds.Add(Context.Guild.GetUser(user.Id).RemoveRoleAsync(role));
                }
            }
            await Task.WhenAll(cmds);

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithDescription("Previous war data has been cleared.");

            await Task.WhenAll
            (
                userDatabase.Users.ClearUserAsync(Context.Guild.Id.ToString()),
                Context.Channel.SendMessageAsync(embed: embed.Build())
            );
        }

        [Command("view-war")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ViewWarAsync()
        {
            Dictionary<string, string> users = await userDatabase.Users.GetUsersAsync(Context.Guild.Id.ToString());
            string userInfo = "";

            bool first = true;
            foreach (string key in users.Where(x => majorAllies.Contains(x.Value)).ToDictionary(x => x.Key).Keys)
            {
                SocketUser user;
                if (ulong.TryParse(key, out ulong userId) && (user = Context.Guild.GetUser(userId)) != null)
                {
                    if (first)
                    {
                        userInfo += "Allies:\n";
                        first = false;
                    }
                    userInfo += $"{user.Mention} - {users[key]}\n";
                }
            }
            foreach (string key in users.Where(x => mijorAllies.Contains(x.Value)).ToDictionary(x => x.Key).Keys)
            {
                SocketUser user;
                if (ulong.TryParse(key, out ulong userId) && (user = Context.Guild.GetUser(userId)) != null)
                {
                    userInfo += $"{user.Mention} - {users[key]}\n";
                }
            }
            first = true;

            foreach (string key in users.Where(x => minorAllies.Contains(x.Value)).ToDictionary(x => x.Key).Keys)
            {
                SocketUser user;
                if (ulong.TryParse(key, out ulong userId) && (user = Context.Guild.GetUser(userId)) != null)
                {
                    if (first)
                    {
                        userInfo += "*Minor Allies:*\n";
                        first = false;
                    }
                    userInfo += $"{user.Mention} - {users[key]}\n";
                }
            }
            first = true;

            foreach (string key in users.Where(x => x.Value != "iTaLY" && majorAxis.Contains(x.Value)).ToDictionary(x => x.Key).Keys)
            {
                SocketUser user;
                if (ulong.TryParse(key, out ulong userId) && (user = Context.Guild.GetUser(userId)) != null)
                {
                    if (first)
                    {
                        userInfo += "Axis:\n";
                        first = false;
                    }
                    userInfo += $"{user.Mention} - {users[key]}\n";
                }
            }
            foreach (string key in users.Where(x => mijorAxis.Contains(x.Value)).ToDictionary(x => x.Key).Keys)
            {
                SocketUser user;
                if (ulong.TryParse(key, out ulong userId) && (user = Context.Guild.GetUser(userId)) != null)
                {
                    userInfo += $"{user.Mention} - {users[key]}\n";
                }
            }
            first = true;

            if (users.Values.Contains("iTaLY"))
            {
                SocketUser user;
                if (ulong.TryParse(users.First(x => x.Value == "iTaLY").Key, out ulong userId) && (user = Context.Guild.GetUser(userId)) != null)
                {
                    userInfo += "iTaLY:\n" +
                        $"{user.Mention}\n";
                }
            }

            foreach (string key in users.Where(x => minorAxis.Contains(x.Value)).ToDictionary(x => x.Key).Keys)
            {
                SocketUser user;
                if (ulong.TryParse(key, out ulong userId) && (user = Context.Guild.GetUser(userId)) != null)
                {
                    if (first)
                    {
                        userInfo += "*Minor Axis:*\n";
                        first = false;
                    }
                    userInfo += $"{user.Mention} - {users[key]}\n";
                }
            }
            first = true;

            if (userInfo.Length == 0)
            {
                userInfo = "No wars have been set up.";
            }
            await Context.Channel.SendMessageAsync(userInfo);
        }

        [Command("assign")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AssignAsync(SocketUser user, [Remainder] string country)
        {
            IEnumerable<string> countries = new List<string>().Concat(majorAllies).Concat(majorAxis).Concat(mijorAllies).Concat(mijorAxis).Concat(minorAllies).Concat(minorAxis);
            if (!countries.Contains(country))
            {
                EmbedBuilder emb = new EmbedBuilder()
                    .WithColor(SecurityInfo.botColor)
                    .WithDescription($"{country} is not a valid country.");

                await Context.Channel.SendMessageAsync(embed: emb.Build());
                return;
            }

            SocketRole oldRole;
            string oldCountry = await userDatabase.Users.GetCountryAsync(user.Id.ToString(), Context.Guild.Id.ToString());
            if (oldCountry != default && ulong.TryParse(await userDatabase.Roles.GetRoleAsync(oldCountry, Context.Guild.Id.ToString()), out ulong oldRoleId) && (oldRole = Context.Guild.GetRole(oldRoleId)) != null)
            {
                await Context.Guild.GetUser(user.Id).RemoveRoleAsync(oldRole);
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithDescription($"{user.Mention} has been assigned: {country}.");

            List<Task> cmds = new()
            {
                userDatabase.Users.AddUserAsync(user.Id.ToString(), country, Context.Guild.Id.ToString()),
                Context.Channel.SendMessageAsync(embed: embed.Build())
            };

            SocketRole role;
            if (ulong.TryParse(await userDatabase.Roles.GetRoleAsync(country, Context.Guild.Id.ToString()), out ulong roleId) && (role = Context.Guild.GetRole(roleId)) != null)
            {
                cmds.Add(Context.Guild.GetUser(user.Id).AddRoleAsync(role));
            }

            await Task.WhenAll(cmds);
        }
    }
}