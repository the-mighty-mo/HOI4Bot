﻿using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HOI4Bot
{
    public class Program
    {
        static void Main() => new Program().StartAsync().GetAwaiter().GetResult();

        private DiscordSocketConfig _config;
        private DiscordSocketClient _client;
        private CommandHandler _handler;

        public static readonly bool isConsole = Console.OpenStandardInput(1) != Stream.Null;

        public async Task StartAsync()
        {
            if (isConsole)
            {
                Console.Title = SecurityInfo.botName;
            }

            bool isRunning = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Count() > 1;
            if (isRunning)
            {
                await Task.Delay(1000);

                isRunning = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Count() > 1;
                if (isRunning)
                {
                    MessageBox.Show("Program is already running", SecurityInfo.botName);
                    return;
                }
            }

            List<Task> initSqlite = new List<Task>()
            {
                SQLCommands.InitUsersSqliteAsync()
            };

            _config = new DiscordSocketConfig
            {
                AlwaysDownloadUsers = false
            };
            _client = new DiscordSocketClient(_config);

            await _client.LoginAsync(TokenType.Bot, SecurityInfo.token);
            await _client.StartAsync();
            await _client.SetGameAsync($"@{SecurityInfo.botName} help", null, ActivityType.Listening);

            IServiceProvider _services = new ServiceCollection().BuildServiceProvider();
            _handler = new CommandHandler(_client, _services);
            Task initCmd = _handler.InitCommandsAsync();

            await Task.WhenAll(initSqlite);
            if (isConsole)
            {
                Console.WriteLine($"{SecurityInfo.botName} has finished loading");
            }

            await initCmd;
            await Task.Delay(-1);
        }
    }
}
