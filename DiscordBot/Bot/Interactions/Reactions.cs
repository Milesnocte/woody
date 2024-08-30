using Dapper;
using Discord;
using Discord.WebSocket;
using DiscordBot.Data.Models;
using Models;
using Serilog;

namespace DiscordBot.Bot.Interactions;

public class Reactions
{
    public static async Task ReactionAdd(Cacheable<IUserMessage, ulong> Message, Cacheable<IMessageChannel, ulong> Channel, SocketReaction Reaction)
    {
        try
        {
            var context = new AppDBContext();
            bool debugFroot = false;
            
            IUserMessage message = await Message.GetOrDownloadAsync();
            string user = message.Author.Mention;
            string messageId = message.Id.ToString();
            string guildId = message.GetJumpUrl().Split("/")[4];
            DateTime date = DateTime.Now.ToUniversalTime();

            Server server = await context.Connection()
                .QuerySingleAsync<Server>($"SELECT * FROM servers WHERE guild_id = '{guildId}' LIMIT 1");
            string serverStar = server.star_emote;
            
            if(server.star_channel == Reaction.Channel.Id.ToString()) return;
            
            if(server.star_channel == null) return; // If no channel is configured do nothing
            
            string emote = Reaction.Emote.ToString();
            int starsToAdd = -2;
            
            switch (emote)
            {
                case "<:debugfroot:936344152004755456>":
                    if (Reaction.User.Value.Id.ToString() == "225772174336720896")
                    {
                        starsToAdd = 0;
                    }
                    break;
                case var value when value == serverStar:
                    starsToAdd = 1;
                    break;
            }
            
            if (starsToAdd == -2) return;
            
            await context.Connection().ExecuteAsync("SELECT insert_or_update_stars(@Guild, @Message, @Member, @Stars, @Date)", 
                new {Guild = guildId, Message = messageId, Member = user, Stars = starsToAdd, Date = new DateTime().ToUniversalTime()});
            int currentStars = await context.Connection().QuerySingleAsync<int>("SELECT get_message_stars(@Message)", new { Message = messageId });
            
            if(await context.Connection().QuerySingleAsync<int>("SELECT get_starboard_posted(@Message)", new { Message = messageId }) == 1 && emote != "<:debugfroot:936344152004755456>") return;
            
            if (currentStars >= server.star_count || emote == "<:debugfroot:936344152004755456>")
            {
                await context.Connection().ExecuteAsync("SELECT set_starboard_posted(@Message)", new {Message = messageId});
                var channel = Program.GetClient().GetGuild(ulong.Parse(guildId)).TextChannels.FirstOrDefault(x => x.Id.ToString() == server.star_channel);
                
                if (channel == null) return;
                
                List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();

                if (message.ReferencedMessage != null)
                {
                    if (string.IsNullOrEmpty(message.ReferencedMessage.Content))
                    {
                        fields.Add(new EmbedFieldBuilder()
                        {
                            Name = "Reply to",
                            Value = "[Attachment]",
                            IsInline = false
                        });
                    }
                    else
                    {
                        fields.Add(new EmbedFieldBuilder()
                        {
                            Name = "Reply to",
                            Value = message.ReferencedMessage.Content,
                            IsInline = false
                        });
                    }
                }

                if (!string.IsNullOrEmpty(message.Content))
                {
                    fields.Add(new EmbedFieldBuilder()
                    {
                        Name = "Message",
                        Value = message.Content,
                        IsInline = false
                    });
                }

                EmbedAuthorBuilder authorBuilder = new EmbedAuthorBuilder();
                authorBuilder.Name = message.Author.GlobalName ?? message.Author.Username;
                authorBuilder.IconUrl = message.Author.GetAvatarUrl();

                if (message.Attachments.Count > 0)
                {
                    if (message.Attachments.First().ContentType.StartsWith("video/") ||
                        message.Attachments.First().ContentType.StartsWith("audio/"))
                    {
                        fields.Add(new EmbedFieldBuilder()
                        {
                            Name = "Attachment",
                            Value = "Could not put attachment in embed, use jump link",
                            IsInline = false
                        });
                    }
                }

                EmbedBuilder embedBuilder = new EmbedBuilder();
                embedBuilder.Author = authorBuilder;
                embedBuilder.Fields = fields;
                    
                if (message.Attachments.Count > 0)
                {
                    embedBuilder.ImageUrl = message.Attachments.First().Url;
                }

                if (message.Attachments.Count > 1)
                {
                    EmbedFooterBuilder footerBuilder = new EmbedFooterBuilder();
                    footerBuilder.Text = "More attachments in original message, use the jump to link to see them";
                    embedBuilder.Footer = footerBuilder;
                }
                

                ButtonBuilder buttonBuilder = new ButtonBuilder();
                buttonBuilder.Url = message.GetJumpUrl();
                buttonBuilder.Label = "Jump to Message";
                buttonBuilder.Style = ButtonStyle.Link;

                ComponentBuilder messageComponent = new ComponentBuilder();
                messageComponent.WithButton(buttonBuilder);
                await channel.SendMessageAsync(embed: embedBuilder.Build(), components: messageComponent.Build());
            }
        }
        catch (Exception e)
        {
            Log.Logger.Error($"[Messages] {e}");
        }
    }
    
    public static async Task ReactionRemove(Cacheable<IUserMessage, ulong> Message, Cacheable<IMessageChannel, ulong> Channel, SocketReaction Reaction)
    {
        try
        {
            IUserMessage message = Message.GetOrDownloadAsync().GetAwaiter().GetResult();
            
            var context = new AppDBContext();
            string user = message.Author.Mention;
            string messageId = message.Id.ToString();
            string guildId = message.GetJumpUrl().Split("/")[4];
            DateTime date = DateTime.Now.ToUniversalTime();
            
            Server server = await context.Connection()
                .QuerySingleAsync<Server>($"SELECT * FROM servers WHERE guild_id = '{guildId}' LIMIT 1");
            string serverStar = server.star_emote;
            
            if(server.star_channel == Reaction.Channel.Id.ToString()) return;
            
            if(server.star_channel == null) return; 

            string emote = Reaction.Emote.ToString();

            int starsToAdd = -2;
            
            switch (emote)
            {
                case var value when value == serverStar:
                    starsToAdd = -1;
                    break;
            }
            
            if (starsToAdd == -2) return;

            await context.Connection().ExecuteAsync("SELECT insert_or_update_stars(@Guild, @Message, @Member, @Stars, @Date)", 
                new {Guild = guildId, Message = messageId, Member = user, Stars = starsToAdd, Date = new DateTime().ToUniversalTime()});
            
        }
        catch (Exception e)
        {
            Log.Logger.Error($"[Messages] {e}");
            return;
        }
    }
}