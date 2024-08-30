using System.Reflection;
using Discord.WebSocket;
using DiscordBot.Bot.SlashCommands.Commands;
using Fergun.Interactive;
using Microsoft.Extensions.Logging;
using Serilog;

namespace DiscordBot.Bot.SlashCommands;

public class CommandHandler
{
    
    private static Dictionary<string, ISlashCommand> _commands = new();

    public CommandHandler()
    {
        _commands.Add(Avatar.Name, new Avatar());
        _commands.Add(Ping.Name, new Ping());
        _commands.Add(Purge.Name, new Purge());
        _commands.Add(Rank.Name, new Rank());
        _commands.Add(Starcheck.Name, new Starcheck());
        _commands.Add(Whois.Name, new Whois());
        _commands.Add(Restart.Name, new Restart());
        _commands.Add(EightBall.Name, new EightBall());
        _commands.Add(Define.Name, new Define());
        _commands.Add(Warn.Name, new Warn());
        _commands.Add(AuditUser.Name, new AuditUser());
        _commands.Add(AuditCase.Name, new AuditCase());
    }
    
    public static async Task SlashCommandHandler(SocketSlashCommand command)
    {
        try
        {
            string commandName = command.Data.Name.ToLower();

            if (_commands.ContainsKey(commandName))
            {
               await _commands[commandName].Run(command);
            }
        }
        catch (Exception ex)
        {
            Log.Logger.Error($"[CommandHandler] {ex}");
        }
    }
}

/* Automated slow command handler

        string CommandName = char.ToUpper(command.Data.Name[0]) + command.Data.Name.Substring(1);
        List<Type> CommandList = Assembly.GetExecutingAssembly().GetTypes()
            .Where(x => x.Namespace == "DiscordBot.Bot.SlashCommands.Commands").ToList();

        var classExec = CommandList.First(x => x.Name == CommandName);
        classExec.GetConstructor(new[] { typeof(SocketSlashCommand) })!.Invoke(new object[] { command });
        return Task.CompletedTask;

*/