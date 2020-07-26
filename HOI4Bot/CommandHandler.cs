﻿using Discord;
using Discord.Commands;
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
        public static int argPos = 0;

        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        public CommandHandler(DiscordSocketClient client, IServiceProvider services)
        {
            _client = client;
            _services = services;

            CommandServiceConfig config = new CommandServiceConfig()
            {
                DefaultRunMode = RunMode.Async
            };
            _commands = new CommandService(config);
        }

        public async Task InitCommandsAsync()
        {
            _client.Connected += SendConnectMessage;
            _client.Disconnected += SendDisconnectError;
            _client.JoinedGuild += SendJoinMessage;
            _client.MessageReceived += HandleCommandAsync;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            _commands.CommandExecuted += SendErrorAsync;
        }

        private async Task SendErrorAsync(Optional<CommandInfo> info, ICommandContext context, IResult result)
        {
            if (!result.IsSuccess && info.Value.RunMode == RunMode.Async && result.Error != CommandError.UnknownCommand && result.Error != CommandError.UnmetPrecondition)
            {
                await context.Channel.SendMessageAsync($"Error: {result.ErrorReason}");
            }
        }

        private async Task SendConnectMessage()
        {
            if (Program.isConsole)
            {
                await Console.Out.WriteLineAsync($"{SecurityInfo.botName} is online");
            }
        }

        private async Task SendDisconnectError(Exception e)
        {
            if (Program.isConsole)
            {
                await Console.Out.WriteLineAsync(e.Message);
            }
        }

        private async Task SendJoinMessage(SocketGuild g) => await g.DefaultChannel.SendMessageAsync("Death to [insert ethnicity/social class here]");

        private async Task HandleCommandAsync(SocketMessage m)
        {
            if (!(m is SocketUserMessage msg) || (msg.Author.IsBot && msg.Author.Id != _client.CurrentUser.Id))
            {
                return;
            }

            SocketCommandContext Context = new SocketCommandContext(_client, msg);

            bool isCommand = msg.HasMentionPrefix(_client.CurrentUser, ref argPos) || msg.HasStringPrefix(prefix, ref argPos);
            if (isCommand)
            {
                var result = await _commands.ExecuteAsync(Context, argPos, _services);

                List<Task> cmds = new List<Task>();
                if (!result.IsSuccess && result.Error == CommandError.UnmetPrecondition)
                {
                    cmds.Add(Context.Channel.SendMessageAsync(result.ErrorReason));
                }

                if (msg.Author.IsBot)
                {
                    cmds.Add(msg.DeleteAsync());
                }

                await Task.WhenAll(cmds);
            }
        }
    }
}