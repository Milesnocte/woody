using System.Runtime.InteropServices.JavaScript;
using Dapper;
using Discord;
using Discord.WebSocket;
using DiscordBot.Data;
using DiscordBot.Data.Models;
using static Program;

namespace DiscordBot.Bot.SlashCommands.Commands;

public class Warn : ISlashCommand
{
    public static string Name = "warn";
    
    public async Task Run(SocketSlashCommand command)
    {
        var context = new AppDBContext();
        var user = (SocketGuildUser)command.Data.Options.First(x => x.Name == "user").Value;
        
        Server server = await context.Connection()
            .QuerySingleAsync<Server>($"SELECT * FROM servers WHERE guild_id = '{command.GuildId.ToString()}' LIMIT 1");
        
        if (!GetClient().GetGuild(command.GuildId.Value).GetUser(command.User.Id).GuildPermissions.ManageMessages)
        {
           await command.RespondAsync($"You do not have permission to use this command!", ephemeral: true);
           return;
        }

        if (user.IsBot)
        {
            await command.RespondAsync("You can not warn a bot!", ephemeral: true);
            return;
        }
        
        int? lastId = await context.Connection().QuerySingleOrDefaultAsync<int?>("SELECT get_last_action_id(@Guild)", new { Guild = command.GuildId.ToString() });

        if (lastId == null)
        {
            lastId = 1;
        }
        else
        {
            lastId++;
        }
        
        
        string id = Guid.NewGuid().ToString();
        long mod = (long)command.User.Id;
        string reason = command.Data.Options.First(x => x.Name == "reason").Value.ToString();
        await context.Connection()
            .ExecuteAsync(
                $"INSERT INTO mod_actions VALUES('{id}','{user.Id}','{mod}','warn','{reason}', '{DateTime.Now}', null, '{server.guild_id}', '{lastId}')");

        EmbedBuilder embedBuilder = new EmbedBuilder();
        embedBuilder.Title = $"Warned in {GetClient().GetGuild((ulong)command.GuildId).Name}";
        embedBuilder.Description = $"**Reason:** {reason}";
        embedBuilder.Color = Color.Orange;

        if (server.log_channel != null)
        {
            EmbedBuilder logEmbed = new EmbedBuilder();
            logEmbed.Title = $"User Warned";
            logEmbed.ThumbnailUrl = user.GetDisplayAvatarUrl();
            logEmbed.Color = Color.Orange;
            
            List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();
            fields.Add(new EmbedFieldBuilder()
            {
                Name = "User",
                Value = user.Mention,
                IsInline = true
            });
            fields.Add(new EmbedFieldBuilder()
            {
                Name = "Mod",
                Value = command.User.Mention,
                IsInline = true
            });
            fields.Add(new EmbedFieldBuilder()
            {
                Name = "Reason",
                Value = reason,
                IsInline = false
            });
            
            logEmbed.Fields = fields;

            await GetClient().GetGuild(Convert.ToUInt64(server.guild_id))
                .GetTextChannel(Convert.ToUInt64(server.log_channel))
                .SendMessageAsync(embed: logEmbed.Build());
        }

        await user.SendMessageAsync(embed: embedBuilder.Build());
        
        await command.RespondAsync($"\u2705 Warned {user.Username}!", ephemeral: true);
    }

    public CommandInfo CommandInfo()
    {
        CommandInfo commandInfo = new CommandInfo
        {
            CommandName = Name,
            CommandDescription = "Warn a user"
        };
        commandInfo.CommandOption.Add(
            new SlashCommandOptionBuilder()
            {
                Name = "user",
                Type = ApplicationCommandOptionType.User,
                Description = "User to warn",
                IsRequired = true
            });
        commandInfo.CommandOption.Add(
            new SlashCommandOptionBuilder()
            {
                Name = "reason",
                Type = ApplicationCommandOptionType.String,
                Description = "reason",
                MaxLength = 250,
                IsRequired = true
            });
        return commandInfo;
    }
}