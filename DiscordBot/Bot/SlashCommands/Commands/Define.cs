using System.Net;
using System.Net.Http.Json;
using Discord;
using Discord.WebSocket;
using DiscordBot.Data;
using Fergun.Interactive;
using Fergun.Interactive.Pagination;
using Newtonsoft.Json;
using static DiscordBot.Data.Models.Dictionary;
using static Program;

namespace DiscordBot.Bot.SlashCommands.Commands;

public class Define : ISlashCommand
{
    public static string Name = "define";
    public async Task Run(SocketSlashCommand command)
    {
        try
        {
            string word = command.Data.Options.FirstOrDefault(o => o.Name == "word")!.Value.ToString()!;
            string uri = $"https://www.dictionaryapi.com/api/v3/references/collegiate/json/{word.Replace(" ", "")}?key={Environment.GetEnvironmentVariable("MARRIAM_KEY")}";
            HttpClient httpClient = new HttpClient();
            List<Definition> definitions = await httpClient.GetFromJsonAsAsyncEnumerable<Definition>(uri).ToListAsync();
            List<Definition> sanitizedDefinitions = definitions.Where(x => x.shortdef.Count() != 0).ToList();
        
            if (!sanitizedDefinitions.Any())
            {
                await command.RespondAsync("No definitions found", ephemeral:true);
                return;
            }

            List<IPageBuilder> pages = new List<IPageBuilder>();
            foreach (var definition in sanitizedDefinitions)
            {
                PageBuilder pageBuilder = new PageBuilder();
                pageBuilder.Title = word;
                pageBuilder.AddField(new EmbedFieldBuilder()
                {
                    Name = definition.fl,
                    Value = "• " + String.Join("\n• ", definition.shortdef)
                });
                pageBuilder.WithFooter(
                    $"Page {sanitizedDefinitions.IndexOf(definition) + 1}/{sanitizedDefinitions.Count} | Requested By: {command.User.Username ?? command.User.GlobalName}");
                pages.Add(pageBuilder);
            }

            var paginator = new StaticPaginatorBuilder()
                .AddUser(command.User)
                .WithPages(pages.ToArray())
                .WithOptions(new Dictionary<IEmote, PaginatorAction>()
                {
                    { new Emoji("\u23ea"), PaginatorAction.SkipToStart },
                    { new Emoji("\u2b05"), PaginatorAction.Backward },
                    { new Emoji("\u27a1"), PaginatorAction.Forward },
                    { new Emoji("\u23e9"), PaginatorAction.SkipToEnd },
                })
                .WithFooter(PaginatorFooter.None)
                .WithActionOnTimeout(ActionOnStop.DisableInput)
                .Build();
            var result = await GetInteractiveService().SendPaginatorAsync(paginator, command);
        }
        catch (Exception e)
        {
            await command.RespondAsync($"An Error Has Occured", ephemeral: true);
            Console.WriteLine(e);
        }
    }

    public CommandInfo CommandInfo()
    {
        CommandInfo commandInfo = new CommandInfo
        {
            CommandName = Name,
            CommandDescription = "Define a word"
        };
        commandInfo.CommandOption.Add(
            new SlashCommandOptionBuilder()
            {
                Name = "word",
                Type = ApplicationCommandOptionType.String,
                Description = "Word to Define",
                IsRequired = true
            });
        return commandInfo;
    }
}