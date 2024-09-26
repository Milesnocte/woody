using System.Data;
using Dapper;
using Discord;
using Discord.WebSocket;
using DiscordBot.Data.Models;
using Models;
using Serilog;

namespace DiscordBot.Bot.Interactions;

public class Messages
{
    private static Dictionary<string, List<string>> _xpDictionary = new();
    private static DiscordSocketClient _socketClient;
    private List<string> _usersToUpdate = new();
    private AppDBContext _database = new();
    private Random _rng = new();
    private Timer _processMessages;
    private Timer _processRoles;
    
    public Messages(DiscordSocketClient client)
    {
        try
        {
            _socketClient = client;
            _processMessages = new Timer(ProcessMessages, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            _processRoles = new Timer(DistributeRoles, null, TimeSpan.FromSeconds(15), TimeSpan.FromMinutes(15));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Log.Error($"[Messages] {e}");
        }
    }
    
    public static Task MessageReceived(SocketMessage arg)
    {
        try
        {
            if (arg.Author.IsBot) return Task.CompletedTask;

            var guildId = arg.GetJumpUrl().Split("/")[4];

            if (!_xpDictionary.ContainsKey(guildId)) _xpDictionary[guildId] = new List<string>();
            if (_xpDictionary[guildId].Contains(arg.Author.Id.ToString())) return Task.CompletedTask;

            _xpDictionary[guildId].Add(arg.Author.Id.ToString());

            return Task.CompletedTask;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Log.Logger.Error($"[Messages] {e}");
            return Task.CompletedTask;
        }
    }

    private async void ProcessMessages(object? state)
    { 
        try
        {
            Dictionary<string, List<string>> processList = new(_xpDictionary);
            _xpDictionary.Clear();

            foreach (var guild in processList.Keys)
            {
                foreach (var user in processList[guild])
                {
                    int xp = _rng.Next(7, 30);
                    
                    await _database.Connection().ExecuteAsync("SELECT insert_or_update_xp(@Member, @Server, @Xp)", new {Member = user, Server = guild, Xp = xp});

                    if (guild == "776380239961260052")
                    {
                        if(!_usersToUpdate.Contains(user)) _usersToUpdate.Add(user);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private async void DistributeRoles(Object? state)
    {
        try
        {
            var beanzone = _socketClient.GetGuild(776380239961260052);
            var rwh = _socketClient.GetGuild(433825343485247499);
            if (beanzone == null) return;
            
            List<Levels> bzMembers =
                (await _database.Connection().QueryAsync<Levels>("SELECT * FROM get_guild_users_xp(@Server)",
                    new { Server = "776380239961260052" }, commandType: CommandType.Text))
                .Where(x => _usersToUpdate.Contains(x.Member)).Distinct().ToList();
            
            List<Levels> rwhMembers =
                (await _database.Connection().QueryAsync<Levels>("SELECT * FROM get_guild_users_xp(@Server)",
                    new { Server = "433825343485247499" }, commandType: CommandType.Text))
                .Where(x => _usersToUpdate.Contains(x.Member)).Distinct().ToList();
            
            _usersToUpdate.Clear();
            
            // RWH Ranks
            foreach (var rwhMember in rwhMembers)
            {
                var user = rwh.GetUser(Convert.ToUInt64(rwhMember.Member));
                if (user == null) continue;
                
                List<ulong> rolesToAdd = new();
                int level = GetLevel(rwhMember.Xp);
                
                if (level >= 5) rolesToAdd.Add(1288931087766261790);
                
                await user.ModifyAsync(x => x.RoleIds = user.Roles.Select(r => r.Id)
                    .Except(new[] { user.Guild.Id })
                    .Concat(rolesToAdd)
                    .Distinct()
                    .ToList());
            }
            
            
            // BZ Ranks
            ulong tadpole = 1114237294988230808;
            ulong froglet = 857136982692724756;
            ulong frog = 857137043355992106;
            ulong bullfrog = 857137261706346537;
            ulong suitedFrog = 857137344318668810;
            ulong rainbowFrog = 882488807776202762;

            foreach (var bzMember in bzMembers)
            {
                var user = beanzone.GetUser(Convert.ToUInt64(bzMember.Member));
                if (user == null) continue;
                
                List<ulong> rolesToAdd = new();
                int level = GetLevel(bzMember.Xp);

                rolesToAdd.Add(tadpole);
                if (level >= 5) rolesToAdd.Add(froglet);
                if (level >= 15) rolesToAdd.Add(frog);
                if (level >= 30) rolesToAdd.Add(bullfrog);
                if (level >= 50) rolesToAdd.Add(suitedFrog);
                if (level >= 75) rolesToAdd.Add(rainbowFrog);
                
                await user.ModifyAsync(x => x.RoleIds = user.Roles.Select(r => r.Id)
                    .Except(new[] { user.Guild.Id })
                    .Concat(rolesToAdd)
                    .Distinct()
                    .ToList());
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private int GetLevel(int xp)
    {
        try
        {
            int level = 0;
            int xpfornext = 0;

            for (int i = 0; i < 10000; i++)
            {
                xpfornext += (int)(5 * Math.Pow(i, 2) + 50 * i + 100);
                if (xpfornext > xp)
                {
                    level = i + 1;
                    break;
                }
            }

            return level;
        }
        catch (Exception e)
        {
            Log.Logger.Error($"[Messages] {e}");
            return 0;
        }
    }

    public static async Task MessageDeleted(Cacheable<IMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2)
    {
        try
        {
            var context = new AppDBContext();
            var guildId = arg1.Value.GetJumpUrl().Split("/")[4];
            var user = arg1.Value.Author;
            
            Server server = await context.Connection()
                .QuerySingleAsync<Server>($"SELECT * FROM servers WHERE guild_id = '{guildId}' LIMIT 1");
            
            if (server.chat_logs == null) return;

            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.Title = $"{user.GlobalName ?? user.Username} Deleted in <#{arg2.Value.Id}>";
            embedBuilder.ThumbnailUrl = user.GetDisplayAvatarUrl();
            
            List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();
            fields.Add(new EmbedFieldBuilder()
            {
                Name = "Message",
                Value = arg1.Value.Content,
                IsInline = false
            });
            embedBuilder.Fields = fields;

            await _socketClient.GetGuild(Convert.ToUInt64(server.guild_id))
                .GetTextChannel(Convert.ToUInt64(server.chat_logs))
                .SendMessageAsync(embed: embedBuilder.Build());
        }
        catch (Exception)
        {
            // Ignored
        }
    }

    public static async Task MessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
    {
        try
        {
            var context = new AppDBContext();
            var guildId = arg2.GetJumpUrl().Split("/")[4];
            var user = arg1.Value.Author;
            
            Server server = await context.Connection()
                .QuerySingleAsync<Server>($"SELECT * FROM servers WHERE guild_id = '{guildId}' LIMIT 1");
            
            if (server.chat_logs == null) return; 
            if(arg1.Value.Content == arg2.Content) return; // This event is triggered when a link embeds, so we have to check the content

            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.Title = $"{user.GlobalName ?? user.Username} Edited in <#{arg3.Id}>";
            embedBuilder.ThumbnailUrl = user.GetDisplayAvatarUrl();

            List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();
            fields.Add(new EmbedFieldBuilder()
            {
                Name = "Old Message",
                Value = arg1.Value.Content,
                IsInline = false
            });
            fields.Add(new EmbedFieldBuilder()
            {
                Name = "New Message",
                Value = arg2.Content,
                IsInline = false
            });
            embedBuilder.Fields = fields;

            await _socketClient.GetGuild(Convert.ToUInt64(server.guild_id))
                .GetTextChannel(Convert.ToUInt64(server.chat_logs))
                .SendMessageAsync(embed: embedBuilder.Build());
        }
        catch (Exception e)
        {
            // Ignored
        }
    }
}