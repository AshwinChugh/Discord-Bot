using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace fuzzcore_bot
{
    
    public class general : ModuleBase<SocketCommandContext>
    {
        private string embedImgLink = "https://i.imgur.com/XEnPLkm.gif";
        [Command("verify")]
        public async Task verifyUser([Remainder]string code)
        {
            var user = Context.Message.Author as SocketGuildUser;
            var verifyChannel =
                ((ITextChannel) Context.Guild.Channels.FirstOrDefault(x => x.Name.ToLower().Equals("verify")));
            var membersRole =
                ((IRole) Context.Guild.Roles.FirstOrDefault(x => x.Name.ToLower().Equals("members")));
            var needsVerifyRole =
                ((IRole) Context.Guild.Roles.FirstOrDefault(x => x.Name.ToLower().Equals("needs to verify")));
            if (Context.Channel == verifyChannel)
            {
                foreach (var userCaptcha in Program.verifyList)
                {
                    if (userCaptcha.code == code)
                    {
                        if (userCaptcha.user == user.Username)
                        {
                            await user.AddRoleAsync(membersRole);
                            await user.RemoveRoleAsync(needsVerifyRole);
                            await verifyChannel.SendMessageAsync("Correct Captcha! You have been verified.");
                            Program.verifyList.Remove(userCaptcha);//remove so the user can not use an old captcha again
                            return;
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync("Invalid Captcha! Try Again");
                        }
                    }
                }
                await verifyChannel.SendMessageAsync("Invalid Captcha!");
            }
        }

        [Command("avatar", RunMode = RunMode.Async)]
        public async Task avatarCheck()
        {
            if (Context.Message.Author.GetAvatarUrl() == null)
            {
                await Context.Channel.SendMessageAsync("No custom avatar detected.");
            }
            else
            {
                var avatarEmbed = new EmbedBuilder();
                avatarEmbed.WithImageUrl(Context.Message.Author.GetAvatarUrl());
                avatarEmbed.WithDescription("Avatar url: " + Context.Message.Author.GetDefaultAvatarUrl());
                avatarEmbed.WithAuthor("Fuzzcore", embedImgLink);
                avatarEmbed.WithCurrentTimestamp();
                avatarEmbed.WithFooter("Fuzzcore");
                await Context.Channel.SendMessageAsync("", false, avatarEmbed.Build());
            }
        }

        [Command("avatar", RunMode = RunMode.Async)]
        public async Task getAvatar(SocketGuildUser userIn)
        { 
            var user = userIn as SocketUser;

            if (user.GetDefaultAvatarUrl() == null)
            {
                await Context.Channel.SendMessageAsync("This user is using the default discord avatar.");
            }
            else
            {
                var avatarEmbed = new EmbedBuilder();
                avatarEmbed.WithImageUrl(user.GetAvatarUrl());
                avatarEmbed.WithDescription("Avatar url: " + user.GetDefaultAvatarUrl());
                avatarEmbed.WithAuthor("Fuzzcore", embedImgLink);
                avatarEmbed.WithCurrentTimestamp();
                avatarEmbed.WithFooter("Fuzzcore");
                await Context.Channel.SendMessageAsync("", false, avatarEmbed.Build());
            }
        }
    }
}
