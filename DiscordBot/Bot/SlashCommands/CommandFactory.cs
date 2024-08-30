using System.Reflection;
using Discord;
using Discord.WebSocket;
using DiscordBot.Data;
using Serilog;

namespace DiscordBot.Bot.SlashCommands;

public class CommandFactory
{
    private List<SlashCommandBuilder> Commands = new();
    
    public CommandFactory(DiscordSocketClient client)
    {
        try
        {
            List<Type> CommandList = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.Namespace == "DiscordBot.Bot.SlashCommands.Commands")
                .Where(m => !m.Name.Contains("<")).ToList();

            foreach (var command in CommandList)
            {
                var commandInstance = Activator.CreateInstance(command);
                var method = command.GetMethod("CommandInfo");
                CommandInfo attributes = method.Invoke(commandInstance, null) as CommandInfo;
                SlashCommandBuilder slashCommand = new SlashCommandBuilder()
                {
                    Name = attributes.CommandName.ToLower(),
                    Description = attributes.CommandDescription,
                    Options = attributes.CommandOption
                };
                Commands.Add(slashCommand);
            }

            RegisterCommands(client);
        }
        catch (Exception e)
        {
            Log.Logger.Error($"[CommandFactory] {e}");
        }
    }

    private async void RegisterCommands(DiscordSocketClient client)
    {
        ApplicationCommandProperties[] commands = new ApplicationCommandProperties[Commands.Count-1];

        int counter = 0;
        foreach (var slashCommandBuilder in Commands)
        {
            if (slashCommandBuilder.Name == "restart")
            {
                continue;
            }
            commands[counter] = slashCommandBuilder.Build();
            counter++;
        }

        await client.BulkOverwriteGlobalApplicationCommandsAsync(commands);
    }
}