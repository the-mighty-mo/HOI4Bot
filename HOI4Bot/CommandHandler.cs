using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace HOI4Bot
{
    public class CommandHandler
    {
        public const string prefix = "\\";
        private static int argPos = 0;

        private readonly DiscordSocketClient client;
        private readonly CommandService commands;
        private readonly InteractionService interactions;
        private readonly IServiceProvider services;

        public CommandHandler(DiscordSocketClient client, IServiceProvider services)
        {
            this.client = client;
            this.services = services;

            InteractionServiceConfig interactionCfg = new()
            {
                DefaultRunMode = Discord.Interactions.RunMode.Async
            };
            interactions = new(client.Rest, interactionCfg);

            CommandServiceConfig commandCfg = new()
            {
                DefaultRunMode = Discord.Commands.RunMode.Async
            };
            commands = new(commandCfg);
        }

        public async Task InitCommandsAsync()
        {
            client.Ready += ReadyAsync;
            client.Connected += SendConnectMessage;
            client.Disconnected += SendDisconnectError;
            client.MessageReceived += HandleCommandAsync;
            client.SlashCommandExecuted += HandleSlashCommandAsync;

            await Task.WhenAll(
                interactions.AddModulesAsync(Assembly.GetEntryAssembly(), services),
                commands.AddModulesAsync(Assembly.GetEntryAssembly(), services)
            ).ConfigureAwait(false);
            interactions.SlashCommandExecuted += SendInteractionErrorAsync;
            commands.CommandExecuted += SendCommandErrorAsync;
        }

        private Task ReadyAsync() =>
            interactions.RegisterCommandsGloballyAsync(true);

        private async Task SendInteractionErrorAsync(SlashCommandInfo info, IInteractionContext context, Discord.Interactions.IResult result)
        {
            if (!result.IsSuccess && info.RunMode == Discord.Interactions.RunMode.Async && result.Error is not InteractionCommandError.UnknownCommand)
            {
                if (result.Error is InteractionCommandError.UnmetPrecondition)
                {
                    await context.Interaction.RespondAsync($"Error: {result.ErrorReason}").ConfigureAwait(false);
                }
                else
                {
                    await context.Channel.SendMessageAsync($"Error: {result.ErrorReason}").ConfigureAwait(false);
                }
            }
        }

        private async Task SendCommandErrorAsync(Optional<CommandInfo> info, ICommandContext context, Discord.Commands.IResult result)
        {
            if (!result.IsSuccess && info.GetValueOrDefault()?.RunMode == Discord.Commands.RunMode.Async && result.Error is not (CommandError.UnknownCommand or CommandError.UnmetPrecondition))
            {
                await context.Channel.SendMessageAsync($"Error: {result.ErrorReason}").ConfigureAwait(false);
            }
        }

        private Task SendConnectMessage() =>
            Console.Out.WriteLineAsync($"{SecurityInfo.botName} is online");

        private Task SendDisconnectError(Exception e) =>
            Console.Out.WriteLineAsync(e.Message);

        private static Task<bool> CanBotRunCommandsAsync(SocketUser usr) => Task.FromResult(false);

        private static Task<bool> ShouldDeleteBotCommands() => Task.FromResult(true);

        private async Task HandleSlashCommandAsync(SocketSlashCommand m)
        {
            if (m.User.IsBot && !await CanBotRunCommandsAsync(m.User).ConfigureAwait(false))
            {
                return;
            }

            SocketInteractionContext Context = new(client, m);

            await interactions.ExecuteCommandAsync(Context, services).ConfigureAwait(false);

            List<Task> cmds = new();
            if (m.User.IsBot && await ShouldDeleteBotCommands().ConfigureAwait(false))
            {
                cmds.Add(m.DeleteOriginalResponseAsync());
            }

            await Task.WhenAll(cmds).ConfigureAwait(false);
        }

        private async Task HandleCommandAsync(SocketMessage m)
        {
            if (m is not SocketUserMessage msg || (msg.Author.IsBot && !await CanBotRunCommandsAsync(msg.Author).ConfigureAwait(false)))
            {
                return;
            }

            SocketCommandContext Context = new(client, msg);
            bool isCommand = msg.HasMentionPrefix(client.CurrentUser, ref argPos) || msg.HasStringPrefix(prefix, ref argPos);

            if (isCommand)
            {
                var result = await commands.ExecuteAsync(Context, argPos, services).ConfigureAwait(false);

                List<Task> cmds = new();
                if (msg.Author.IsBot && await ShouldDeleteBotCommands().ConfigureAwait(false))
                {
                    cmds.Add(msg.DeleteAsync());
                }
                else if (!result.IsSuccess && result.Error == CommandError.UnmetPrecondition)
                {
                    cmds.Add(Context.Channel.SendMessageAsync(result.ErrorReason));
                }

                await Task.WhenAll(cmds).ConfigureAwait(false);
            }
        }
    }
}