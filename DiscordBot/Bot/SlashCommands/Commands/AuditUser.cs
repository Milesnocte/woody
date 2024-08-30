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

public class AuditUser  : ISlashCommand
{
    public static string Name = "audituser";
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
            await command.RespondAsync("Bots cant be warned!", ephemeral: true);
            return;
        }

        int counter = 0;
        List<IPageBuilder> pages = new List<IPageBuilder>();
        List<ModActions> actions = (await context.Connection().QueryAsync<ModActions>(
            "SELECT * FROM get_user_moderations(@User, @Guild)",
            new { User = user.Id.ToString(), Guild = command.GuildId.ToString() })).AsList().OrderByDescending(d => d.Date).ToList();
        
        if (actions.Count != 0)
        {
            for (var i = 0; i < actions.Count; i += 10)
            {
                counter++;
                PageBuilder pageBuilder = new PageBuilder();
                pageBuilder.Title = $"{user.Username}'s logs ({actions.Count})";
                pageBuilder.Color = Color.DarkBlue;
                string desc = "";
                foreach (var action in actions.Skip(i).Take(10).ToList())
                {
                    switch (action.Type.ToLower())
                    {
                        case "warn":
                            desc += $"<:warn:1279139392849580205> **Warn** **Id:**`{action.ActionId}` ";
                            break;
                        case "mute":
                            desc += $"<:mute:1279160832894177300> **Mute** **Id:**`{action.ActionId}` ";
                            break;
                        case "kick":
                            desc += $"<:kick:1279139392178356375> **Kick** **Id:**`{action.ActionId}` ";
                            break;
                        case "ban":
                            desc += $"<:ban:1279139389729017928> **Ban** **Id:**`{action.ActionId}` ";
                            break;
                    }

                    string actionReason;
                    if (action.Reason.Length > 200)
                    {
                        actionReason = action.Reason[..200] + "...";
                    }
                    else
                    {
                        actionReason = action.Reason;
                    }

                    desc +=
                        $"<t:{((DateTimeOffset)action.Date).ToUnixTimeSeconds()}:R> \n <:blank:1278796649317007442> \u2937 {actionReason}\n";
                }

                pageBuilder.Description = desc;
                pageBuilder.WithFooter(
                    $"Page {counter}/{Math.Ceiling(actions.Count / 10.0)} | Requested By: {command.User.GlobalName}");
                pages.Add(pageBuilder);
            }
        }
        else
        {
            PageBuilder pageBuilder = new PageBuilder();
            pageBuilder.Title = $"{user.Username}'s Mod Actions";
            pageBuilder.Description = ":ballot_box_with_check: No Actions!";
            pageBuilder.WithFooter(
                $"Page 1/1 | Requested By: {command.User.GlobalName}");
            pages.Add(pageBuilder);
        }

        var paginator = new StaticPaginatorBuilder()
            .AddUser(command.User)
            .WithPages(pages.ToArray())
            .WithOptions(new Dictionary<IEmote, PaginatorAction>()
            {
                { new Emoji("\u2b05"), PaginatorAction.Backward },
                { new Emoji("\u27a1"), PaginatorAction.Forward },
            })
            .WithFooter(PaginatorFooter.None)
            .WithActionOnTimeout(ActionOnStop.DisableInput)
            .Build();
        await GetInteractiveService().SendPaginatorAsync(paginator, command);
    }

    public CommandInfo CommandInfo()
    {
        CommandInfo commandInfo = new CommandInfo
        {
            CommandName = Name,
            CommandDescription = "See actions taken against a user"
        };
        commandInfo.CommandOption.Add(
            new SlashCommandOptionBuilder()
            {
                Name = "user",
                Type = ApplicationCommandOptionType.User,
                Description = "User to check",
                IsRequired = true
            });
        return commandInfo;
    }
}