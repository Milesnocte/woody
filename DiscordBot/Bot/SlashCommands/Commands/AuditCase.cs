using Dapper;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Data.Models;
using Fergun.Interactive;
using Fergun.Interactive.Pagination;
using Models;
using static Program;
using CommandInfo = DiscordBot.Data.CommandInfo;

namespace DiscordBot.Bot.SlashCommands.Commands;

public class AuditCase : ISlashCommand
{
    public static string Name = "auditcase";
    public async Task Run(SocketSlashCommand command)
    {
        var context = new AppDBContext();
        var caseId = (string)command.Data.Options.First(x => x.Name == "case").Value;
        
        if (!GetClient().GetGuild(command.GuildId.Value).GetUser(command.User.Id).GuildPermissions.ManageMessages)
        {
            await command.RespondAsync($"You do not have permission to use this command!", ephemeral: true);
            return;
        }
        
        List<ModActions?> modCases = (await context.Connection().QueryAsync<ModActions?>("SELECT * FROM get_server_cases(@Guild)", new {Guild = command.GuildId.ToString()})).ToList();
        ModActions? modCase = modCases.FirstOrDefault(x => x?.ActionId == caseId);

        if (modCase == null)
        {
            await command.RespondAsync($"No case found with Id {caseId}", ephemeral: true);
            return;
        }
        
        long unixTimestamp = ((DateTimeOffset)modCase.Date).ToUnixTimeSeconds();

        SocketUser user = GetClient().GetGuild((ulong)command.GuildId).GetUser(ulong.Parse(modCase.UserId.ToString()));
        SocketUser mod = GetClient().GetGuild((ulong)command.GuildId).GetUser(ulong.Parse(modCase.ModId.ToString()));
        
        EmbedBuilder embed = new EmbedBuilder();
        embed.Title = $"Case {caseId} Details ";
        embed.Color = Color.Orange;
        embed.ThumbnailUrl = user.GetDisplayAvatarUrl();
        
        List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();
        fields.Add(new EmbedFieldBuilder()
        {
            Name = "Mod",
            Value = mod.Mention,
            IsInline = true
        });
        fields.Add(new EmbedFieldBuilder()
        {
            Name = "Type",
            Value = modCase.Type.ToUpper(),
            IsInline = true
        });
        fields.Add(new EmbedFieldBuilder()
        {
            Name = "User",
            Value = user.Mention,
            IsInline = false
        });
        fields.Add(new EmbedFieldBuilder()
        {
            Name = "Reason",
            Value = modCase.Reason,
            IsInline = false
        });
        fields.Add(new EmbedFieldBuilder()
        {
            Name = "Date",
            Value = "<t:" + unixTimestamp + ":f>",
            IsInline = false
        });
            
        embed.Fields = fields;

        await command.RespondAsync(embed:embed.Build());
    }

    public CommandInfo CommandInfo()
    {
        CommandInfo commandInfo = new CommandInfo
        {
            CommandName = Name,
            CommandDescription = "Details about a case"
        };
        commandInfo.CommandOption.Add(
            new SlashCommandOptionBuilder()
            {
                Name = "case",
                Type = ApplicationCommandOptionType.String,
                Description = "Case Id",
                IsRequired = true
            });
        return commandInfo;
    }
}