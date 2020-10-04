using DSharpPlus;
using DSharpPlus.CommandsNext;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using EasyManager.Data;
using System;
using DSharpPlus.EventArgs;
using System.Collections.Generic;
using EasyManager.Commands;
using DSharpPlus.Entities;
using EasyManager.Events;

namespace EasyManager {

	public class Bot {

		IReadOnlyDictionary<int, CommandsNextExtension> Commands;
		public static DiscordShardedClient SharedClient;

		public async Task StartBot() {
			var json = string.Empty;
			using (var fs = File.OpenRead("config.json"))
			using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
				json = await sr.ReadToEndAsync().ConfigureAwait(false);

			var configJson = JsonConvert.DeserializeObject<Data_Config>(json);

			var config = new DiscordConfiguration {
				Token = Data_Config.Token,
				TokenType = TokenType.Bot,
				AutoReconnect = true,
				LogLevel = LogLevel.Debug,
				UseInternalLogHandler = true,
			};

			SharedClient = new DiscordShardedClient(config);
			SharedClient.Ready += ClientReady;

			CommandsNextConfiguration commandsConfig = new CommandsNextConfiguration {
				StringPrefixes = new[] { "!" },
				EnableDms = false,
				EnableMentionPrefix = true,
				EnableDefaultHelp = false
			};

			await SharedClient.StartAsync();

			Commands = await SharedClient.UseCommandsNextAsync(commandsConfig);

			foreach (var command in Commands) {
				command.Value.RegisterCommands<Cmd_Ping>();
				command.Value.RegisterCommands<Cmd_ServerStats>();
				command.Value.RegisterCommands<Cmd_Help>();
				command.Value.RegisterCommands<Cmd_PinMessage>();
				command.Value.RegisterCommands<Cmd_InviteManager>();
			}

			// Events handlers
			SharedClient.GuildMemberAdded   /**/ += Events_GuildMemberAdded.Main;
			SharedClient.GuildMemberRemoved /**/ += Events_GuildMemberRemoved.Main;
			SharedClient.GuildCreated       /**/ += Events_GuildCreated.Main;
			SharedClient.MessageCreated     /**/ += Events_MessageCreated.Main;
			SharedClient.GuildAvailable     /**/ += Events_GuildAvailable.Main;
			SharedClient.InviteCreated      /**/ += Events_InviteCreated.Main;

			await Database.LoadServers();
			await Task.Delay(-1);
		}

		private async Task ClientReady(ReadyEventArgs e) {
			Console.WriteLine("Connected!");

			try {
				DiscordActivity activity = new DiscordActivity();
				activity.Name = "easymanager.xyz | !help";
				await e.Client.UpdateStatusAsync(activity);
			} catch (System.Exception ex) {
				Console.WriteLine(ex.ToString());
			}

			await Task.Delay(0);
		}

	}
}
