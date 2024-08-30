using Dapper;
using Discord;
using Discord.WebSocket;
using DiscordBot.Bot.Interactions;
using DiscordBot.Data;
using DiscordBot.Data.Models;
using Models;

namespace DiscordBot.Bot.SlashCommands.Commands;

public class Starcheck : ISlashCommand
{
    
    public static string Name = "starcheck";

    public async Task Run(SocketSlashCommand command)
    {
        var context = new AppDBContext();
        Server server = await context.Connection()
            .QuerySingleAsync<Server>($"SELECT * FROM servers WHERE guild_id = '{command.GuildId.ToString()}' LIMIT 1");
        SocketGuildUser user;
        
        if (command.Data.Options.Count != 0)
        {
            user = (SocketGuildUser) command.Data.Options.First().Value;
        }
        else
        {
            user = (SocketGuildUser) command.User;
        }
        
        int stars = await context.Connection().QuerySingleAsync<int>("SELECT get_user_stars(@Message, @Guild)", new { Message = user.Mention, Guild = command.GuildId.ToString() });

        await command.RespondAsync($"{user.Mention} has {string.Format($"{stars:n0}")}{server.star_emote}", allowedMentions: AllowedMentions.None);
    }

    public CommandInfo CommandInfo()
    {
        CommandInfo commandInfo = new CommandInfo
        {
            CommandName = Name,
            CommandDescription = "Get a user star count"
        };
        commandInfo.CommandOption.Add(
            new SlashCommandOptionBuilder()
            {
                Name = "user",
                Type = ApplicationCommandOptionType.User,
                Description = "User to get",
                IsRequired = false
            });
        return commandInfo;
    }
}