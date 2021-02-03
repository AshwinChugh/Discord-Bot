using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

namespace fuzzcore_bot 
{
    public class Admin : ModuleBase<SocketCommandContext>
    {
        private string embedImgLink = "https://i.imgur.com/XEnPLkm.gif";
        //private string textDB = @"C:\Users\chugh\source\repos\fuzzcore bot\fuzzcore bot\bin\x64\Release\db.txt";
        private string textDB = @"C:\Program Files (x86)\blacklist\text.txt";

        [Command("assignrole")]
        public async Task assignRole(SocketGuildUser userin, SocketRole roleIn)
        {
            //if(true)
            //    return;
            var author = Context.Message.Author as SocketGuildUser;
            if (author.GuildPermissions.Administrator || admin(author))
            {
                await userin.AddRoleAsync(roleIn);
                await Context.Channel.SendMessageAsync("Added role " + roleIn.Name + " to " + userin.Username);
            }
            else
            {
                await NoPermissions();
            }
            
        }

        [Command("blacklist")]
        public async Task addBlacklist(ulong idIn)
        {
            var user = Context.Message.Author as SocketGuildUser;
            if (user.GuildPermissions.Administrator || user.GuildPermissions.ManageGuild || user.GuildPermissions.BanMembers)
            {
                foreach (var id in Program.blackList)
                {
                    if (idIn == id)
                    {
                        await Context.Channel.SendMessageAsync("This id is already blacklisted.");
                        return;
                    }
                }
                Program.blackList.Add(idIn);
                await Context.Channel.SendMessageAsync("Added to active blacklist");
                try
                {
                    using (StreamWriter BLFile = new StreamWriter(textDB, true))
                    {
                        BLFile.WriteLine(idIn.ToString());
                        BLFile.Close();
                        BLFile.Dispose();
                    }
                    await Context.Channel.SendMessageAsync("Added to blacklist database");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                //debugging purposes
                StreamReader blFileReader = new StreamReader(textDB);
                var lineCount = File.ReadAllLines(textDB).Count();
                await Context.Channel.SendMessageAsync("There are currently " + lineCount + " ids that are blacklisted.");
                //            Console.WriteLine("Lines: " + lineCount);

                //            foreach (var userUlong in Program.WhiteList)
                //            {
                //                Console.WriteLine(userUlong);
                //            }

            }
            else
            {
                await NoPermissions();
            }
        }

        [Command("blusers")]
        public async Task showBlUsers()
        {
            var user = Context.Message.Author as SocketGuildUser;
            if (user.GuildPermissions.Administrator || user.GuildPermissions.ManageGuild ||
                user.GuildPermissions.BanMembers)
            {
                await Context.Channel.SendMessageAsync("Currently blacklisted ids: ");
                foreach (var id in Program.blackList)
                {
                    await Context.Channel.SendMessageAsync(id.ToString());
                }
            }
        }

        [Command("remove bluser"), Alias("unblacklist")]
        public async Task removeBLUser(ulong idIn)
        {
            var userId = idIn.ToString();
            Program.blackList.Remove(idIn);
            await Context.Channel.SendMessageAsync("ID: " + userId + " has been removed from active blacklist.");
            var fileData = File.ReadAllLines(textDB).ToList();
            File.WriteAllText(textDB, string.Empty);
            foreach (var id in fileData)
            {
                if (id == userId)
                {
                    fileData.Remove(id);
                    break;
                }
            }
            File.WriteAllLines(textDB, fileData.ToArray());
            await Context.Channel.SendMessageAsync("ID: " + userId + " has been removed from the blacklist database.");
            fileData.Clear();//release memory
        }

        [Command("whitelist")]
        public async Task addWhitelist(ulong idIn)
        {
            var user = Context.Message.Author as SocketGuildUser;
            if (user.GuildPermissions.Administrator || user.GuildPermissions.ManageGuild ||
                user.GuildPermissions.BanMembers || admin(user))
            {
                foreach (var id in Program.whitelist)
                {
                    if (idIn == id)
                    {
                        await Context.Channel.SendMessageAsync("This id is already whitelisted.");
                        return;
                    }
                }
                Program.whitelist.Add(idIn);
                await Context.Channel.SendMessageAsync("Added to active whitelist");
                await Context.Channel.SendMessageAsync(
                    "Whitelist database is currently under development and thus this id has not been added. As a result, if and when the bot gets updated, any users add through this command will not be initialized again. It is strongly recommended that you call `.fc wlusers` to get the ids of all users in the whitelist before the bot updates.");
            }
            else
            {
                await NoPermissions();
            }
        }

        [Command("wlUsers")]
        public async Task displayWLUsers()
        {
            var user = Context.Message.Author as SocketGuildUser;
            if (user.GuildPermissions.Administrator || user.GuildPermissions.ManageGuild ||
                user.GuildPermissions.BanMembers)
            {
                await Context.Channel.SendMessageAsync("ID's currently whitelist:");
                foreach (var userId in Program.whitelist)
                {
                    await Context.Channel.SendMessageAsync(userId.ToString());
                }
            }
            else
            {
                await NoPermissions();
            }
                
        }

        [Command("info", RunMode = RunMode.Async)]
        public async Task getUserInfo(SocketGuildUser userIn)
        {
            var user = userIn as SocketUser;
            var author = Context.Message.Author as SocketGuildUser;
            if (admin(Context.Message.Author as SocketGuildUser) || author.GuildPermissions.KickMembers)
            {
                var userInfoEmbed = new EmbedBuilder();
                userInfoEmbed.WithAuthor("Fuzzcore Bot User Info", embedImgLink);
                userInfoEmbed.AddField("Username", user.Username);
                userInfoEmbed.AddField("Date of Creation", userIn.CreatedAt.ToLocalTime());
                userInfoEmbed.AddField("User ID", userIn.Id);
                userInfoEmbed.AddField("Profile Picture", userIn.GetAvatarUrl());
                userInfoEmbed.WithImageUrl(userIn.GetAvatarUrl());
                await Context.Channel.SendMessageAsync("", false, userInfoEmbed.Build());

            }
            else
            {
                await NoPermissions();
            }
        }


    [Command("ban", RunMode = RunMode.Async)]
        public async Task guildBan(SocketGuildUser userIn, string banMessage = null)
        {
            var user = Context.User as SocketGuildUser;
            //var role = (user as IGuildUser).Guild.Ro
            if (!user.GuildPermissions.BanMembers)
            {
                await NoPermissions();
            }
            else
            {
                EmbedBuilder banConfirmedEmbed = new EmbedBuilder();
                banConfirmedEmbed.AddField(":white_check_mark: Ban successful", userIn.Username + " has been banned.");
                banConfirmedEmbed.WithFooter("This message will delete in 15 seconds • Fuzzcore Bot", embedImgLink);
                banConfirmedEmbed.WithColor(73, 221, 255);
                try
                {
                    await sendBanMessage(userIn, banMessage);
                }
                catch (Exception e)
                {
                    await Context.Channel.SendMessageAsync("Unable to send ban message. User Banned anyway.");
                }
                await userIn.BanAsync(0, banMessage);
                await Context.Channel.SendMessageAsync("Success!");
                var response = await Context.Channel.SendMessageAsync("", false, banConfirmedEmbed.Build());
                await Task.Delay(15000);
                await response.DeleteAsync();
            }           

        }

        [Command("kick", RunMode = RunMode.Async)]
        public async Task guildKick(SocketGuildUser userIn,[Remainder] string kickMessage = null)
        {
            var user = Context.User as SocketGuildUser;
            if (!user.GuildPermissions.KickMembers)
            {
                await NoPermissions();
            }
            else
            {
                EmbedBuilder kickConfirmedEmbed = new EmbedBuilder();
                kickConfirmedEmbed.AddField(":white_check_mark: Kick successful", userIn.Username + " has been kicked.");
                kickConfirmedEmbed.WithFooter("This message will delete in 15 seconds • Fuzzcore Bot", embedImgLink);
                kickConfirmedEmbed.WithColor(73, 221, 255);
                try
                {
                    await sendKickMessage(userIn, kickMessage);
                }
                catch (Exception e)
                {
                    await Context.Channel.SendMessageAsync("Unable to send kick message. User kicked anyway.");
                }
                await Context.Channel.SendMessageAsync("Success!");
                await userIn.KickAsync(kickMessage);
                var confirmMessage = await Context.Channel.SendMessageAsync("", false, kickConfirmedEmbed.Build());
                await Task.Delay(15000);
                await confirmMessage.DeleteAsync();
            }
            
        }

        [Command("say")]
        public async Task AnnouncementTask(SocketRole roleIn = null, [Remainder] string announcementIn = null)
        {
            //await Context.Channel.SendMessageAsync("This command has been temporarily disabled due to security reasons.",
            //false);
            //ulong testChnlID = 550760472455151626;
            //ulong chnlID = 550381025298219018; //fake announcement channel id
            ulong realChnldId = 576831024659693578; //real announcement channel id
            var user = Context.Message.Author as SocketGuildUser;
            //testing shit

            //ulong testRoleId = 550760542575263764; //--> test user role ID(not really needed)
            //ulong testAdminId = 544985698449620993; // --> test admin role ID
            //user.Roles.Contains(Context.Guild.GetRole(testAdminId)) --> for testing



            if (user.GuildPermissions.Administrator || admin(user))
            {
                if (string.IsNullOrWhiteSpace(announcementIn))
                {
                    await Context.Channel.SendMessageAsync("Please enter something to announce.");
                }
                else
                {
                    var announcementArray = new string[2];
                    var announcementEmbed = new EmbedBuilder();
                    announcementArray = announcementIn.Split(new[] {','}, 2);
                    if (announcementArray.Length >= 2)
                    {
                        announcementEmbed.WithTitle(announcementArray[0]);
                        announcementEmbed.WithDescription(announcementArray[1]);
                    }
                    else if (announcementArray.Length == 1)
                    {
                        announcementEmbed.WithTitle("Announcement");
                        announcementEmbed.WithDescription(announcementArray[0]);
                    }

                    announcementEmbed.WithColor(73, 221, 255);
                    announcementEmbed.WithFooter("Fuzzcore Bot", embedImgLink);
                    announcementEmbed.WithCurrentTimestamp();
                    var chnl = Program.client.GetChannel(realChnldId) as IMessageChannel;
                    await chnl.SendMessageAsync(roleIn.Mention, false, announcementEmbed.Build());
                }
            }
            else
            {
                await NoPermissions();
            }
        }

        [Command("mute")]
        public async Task mute(SocketGuildUser userIn)
        {
            var user = Context.Message.Author as SocketGuildUser;
            
            var denyPerms = new OverwritePermissions(PermValue.Inherit, PermValue.Inherit, PermValue.Inherit, PermValue.Inherit, PermValue.Deny);
            var mutedRole = (IRole)Context.Guild.Roles.FirstOrDefault(x => x.Name.ToLower().Contains("muted")); //real muted role
            if (userIn.Roles.Contains(mutedRole))
            {
                await Context.Channel.SendMessageAsync(userIn.Username +
                                                 " has already been muted. You can't mute the same user twice.");
                return;//stop execution
            }
            if (user.GuildPermissions.BanMembers || admin(user))
            {
                await Context.Channel.SendMessageAsync(
                    "Unlike MEE6, I actually mute the user. As a result, that takes me a bit longer to mute and unmute. Please be patient and wait for the confirmation message.");
                await userIn.AddRoleAsync(mutedRole);
                if (userIn.Username.Length > 25)
                {
                    await userIn.ModifyAsync(x => x.Nickname = "[MUTED]");
                }
                else
                {
                    await userIn.ModifyAsync(x => { x.Nickname = userIn.Username + "[MUTED]"; });
                }
                foreach (var channel in Context.Guild.Channels)
                {
                    if (userIn.GetPermissions(channel).ViewChannel && userIn.GetPermissions(channel).SendMessages)
                    {
                        await channel.AddPermissionOverwriteAsync(userIn, denyPerms);
                    }
                }

                await Context.Channel.SendMessageAsync(userIn.Username + " has been muted.");
            }
            else
            {
                await NoPermissions();
            }
        }

        [Command("unmute")]
        public async Task unmute(SocketGuildUser userIn)
        {
            var user = Context.Message.Author as SocketGuildUser;
            var mutedRole = (IRole)Context.Guild.Roles.FirstOrDefault(x => x.Name.ToLower().Contains("muted")); //real muted role

            if (userIn.Roles.Contains(mutedRole) || userIn.Nickname.Contains("[MUTED]"))
            {
                if (user.GuildPermissions.BanMembers || admin(user))
                {
                    await Context.Channel.SendMessageAsync(
                        "Unlike MEE6, I actually mute the user. As a result, that takes me a bit longer to mute and unmute. Please be patient and wait for the confirmation message.");
                    await userIn.RemoveRoleAsync((mutedRole));
                    foreach (var channel in Context.Guild.Channels)
                    {
                        if (!userIn.GetPermissions(channel).SendMessages && userIn.GetPermissions(channel).ViewChannel)
                        {
                            await channel.RemovePermissionOverwriteAsync(userIn);
                            await userIn.ModifyAsync(x => { x.Nickname = null; });
                        }
                    }

                    await Context.Channel.SendMessageAsync(userIn.Username + " has been unmuted.");
                }
                else
                {
                    await NoPermissions();
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync(userIn.Username + " was never muted.");
            }
        }

        [Command("clear")]
        //[RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task bulkPurge(int amount)
        {

            var user = Context.Message.Author as SocketGuildUser;

            if (user.GuildPermissions.ManageMessages || admin(user))
            {
                IEnumerable<IMessage> messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();
                var filteredMessages = messages.Where(x => (DateTimeOffset.UtcNow - x.Timestamp).TotalDays <= 14);

                var count = filteredMessages.Count();
                if (count == 0)
                {
                    await Context.Channel.SendMessageAsync("Unable to delete anything or nothing to delete.");
                }
                else
                {
                    await ((ITextChannel)Context.Channel).DeleteMessagesAsync(filteredMessages);
                    const int delay = 3000;
                    IUserMessage m = await ReplyAsync($"I have deleted {amount} messages for ya. :)");
                    await Task.Delay(delay);
                    await m.DeleteAsync();
                }
            }
            else
            {
                await NoPermissions();
            }   
        }

        [Command("purge", RunMode = RunMode.Async)]
        //[RequireUserPermission(GuildPermission.ManageChannels)]
        public async Task bulkPurge()
        {
            //Channel Settings

            var user = Context.Message.Author as SocketGuildUser;
            var channel = Context.Channel as SocketTextChannel;
            

            if (user.GuildPermissions.ManageChannels || admin(user))
            {
                await Context.Channel.SendMessageAsync("Replicating channel...");
                //await Context.Channel.SendMessageAsync("Cached messages found: " + channel.CachedMessages.Count);
                var name = channel.Name;
                var isNSFW = channel.IsNsfw;
                var position = channel.Position;
                var category = channel.CategoryId;
                var topic = channel.Topic;

                try
                {
                    var newChannel = await Context.Guild.CreateTextChannelAsync(name, x =>
                    {
                        try
                        {
                            x.IsNsfw = isNSFW;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("IsNSFW");
                        }

                        try
                        {
                            x.Position = position;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("position");
                            throw;
                        }

                        try
                        {
                            x.CategoryId = category;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("category");
                        }

                        try
                        {
                            x.Topic = topic;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("topic");
                        }

                    });

                    foreach (var role in Context.Guild.Roles)
                    {
                        var perms = channel.GetPermissionOverwrite(role);
                        try
                        {
                            await newChannel.AddPermissionOverwriteAsync(role, perms.Value);
                        }
                        catch (Exception e)
                        {
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                await Context.Channel.SendMessageAsync("Purging...");
                await Task.Delay(1000);

                await channel.DeleteAsync();
            }
            else
            {
                await NoPermissions();
            }
        }

        //        [Command("lock")]
        //        public async Task lockChannel()
        //        {
        //            var channel = Context.Channel as SocketGuildChannel;
        //            var user = Context.Message.Author as SocketGuildUser;
        //            if (admin(user))
        //            {
        //                foreach (var role in Context.Guild.Roles)
        //                {
        //                    if (!roleCheck(role))
        //                    {
        //                        await channel.AddPermissionOverwriteAsync(role,
        //                            new OverwritePermissions(PermValue.Inherit, PermValue.Inherit, PermValue.Deny,
        //                                PermValue.Inherit, PermValue.Deny));
        //                    }
        //
        //                    var completedEmbed = new EmbedBuilder();
        //                    completedEmbed.WithColor(Color.Blue);
        //                    completedEmbed.WithDescription(
        //                        "This channel has been muted. Everyone aside from the admins will be unable to send messages until `.fc unlock` has been called.");
        //                    completedEmbed.WithAuthor("Channel Unlocked", embedImgLink);
        //                    await Context.Channel.SendMessageAsync("", false, completedEmbed.Build());
        //                }
        //            }
        //            else
        //            {
        //                await NoPermissions();
        //            }
        //        }
        //
        //        [Command("unlock")]
        //        public async Task unlockChannel()
        //        {
        //            var channel = Context.Channel as SocketGuildChannel;
        //            var user = Context.Message.Author as SocketGuildUser;
        //            if (admin(user))
        //            {
        //                foreach (var role in Context.Guild.Roles)
        //                {
        //                    if (roleCheck(role))
        //                    {
        //                        await channel.RemovePermissionOverwriteAsync(role);
        //                    }
        //                    
        //                }
        //                var completedEmbed = new EmbedBuilder();
        //                completedEmbed.WithColor(Color.Blue);
        //                completedEmbed.WithDescription(
        //                    "This channel has been unmuted. Everyone who had access before to send messages will be able to send messages again.");
        //                completedEmbed.WithAuthor("Channel Unlocked", embedImgLink);
        //                await Context.Channel.SendMessageAsync("", false, completedEmbed.Build());
        //            }
        //            else
        //            {
        //                await NoPermissions();
        //            }
        //        }
        //
        //        [Command("ALock")]
        //        public async Task emergency()
        //        {
        //            //var channel = Context.Guild.GetChannel()
        //        }

//        private bool roleCheck(SocketRole roleIn)
//        {
//            bool returnVar;
//            ulong ownerRoleID = 546838690790637577;
//            ulong managerRoleID = 547025788944384011;
//            ulong adminRoleId = 546830805163442186;
//            ulong supportRoleID = 547011558891454474;
//            ulong testSupportRoleID = 554999718133104703;
//            if (roleIn.Id == ownerRoleID || roleIn.Id == managerRoleID || roleIn.Id == adminRoleId ||
//                roleIn.Id == supportRoleID || roleIn.Id == testSupportRoleID || roleIn.Id == 549729391651979267)
//            {
//                returnVar = true;
//            }
//            else
//            {
//                returnVar = false;
//            }
//
//            return returnVar;
//        }

//        private async Task startTimer(int minutes)
//        {
//            var _countdown = minutes;
//            var _timer = new Timer();
//            _timer.Tick += new EventHandler(timer_Tick);
//            _timer.Interval = 60000;
//            _timer.Start();
//        }

//        private void timer_Tick(object sender, EventArgs e)
//        {
//            _countDown--;
//            if (_countDown <= 0)
//            {
//                timerDone = true;
//            }
//        }

        private async Task NoPermissions()
        {
            EmbedBuilder permissionDeniedEmbed = new EmbedBuilder();
            permissionDeniedEmbed.AddField(":x: Command failed", "You don't have permission to use this command. Nice try tard.");
            permissionDeniedEmbed.WithFooter("This message will be deleted in 15 seconds • Fuzzcore Bot", embedImgLink);
            permissionDeniedEmbed.WithColor(73, 221, 255);
            var response = await Context.Channel.SendMessageAsync("", false, permissionDeniedEmbed.Build());
            await Task.Delay(15000);
            await response.DeleteAsync();
        }
    
        private async Task sendBanMessage(SocketGuildUser userIn, string banMessage = null)
        {
            if (banMessage != null)
            {
                try
                {
                    await userIn.SendMessageAsync("You have been from Fuzzcore. Ban message: " + banMessage);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            else
            {
                try
                {
                    await userIn.SendMessageAsync("You have been banned from Fuzzcore.");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        private async Task sendKickMessage(SocketGuildUser userIn, string kickMessage = null)
        {
            if (kickMessage != null)
            {
                try
                {
                    await userIn.SendMessageAsync("You have been kicked from Fuzzcore. Admin message: " + kickMessage);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            else
            {
                try
                {
                    await userIn.SendMessageAsync("You have been kicked from Fuzzcore.");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        public bool admin(SocketGuildUser user)
        {
            bool returnVar;
//            var ownerRole = ((ITextChannel)Context.Channel).Guild.Roles.FirstOrDefault(x => x.Name.ToLower().Contains("owner"));
//            var headStaff =
//                ((ITextChannel)Context.Channel).Guild.Roles.FirstOrDefault(x => x.Name.ToLower().Equals("head staff"));
//            var staffRole =
//                ((ITextChannel)Context.Channel).Guild.Roles.FirstOrDefault(x => x.Name.ToLower().Equals("staff"));
//            var trialStaff = ((ITextChannel)Context.Channel).Guild.Roles.FirstOrDefault(x => x.Name.ToLower().Equals("trial staff"));
            if (user.Id == 256609272694177793)
            {
                returnVar = true;
            }
            else
            {
                returnVar = false;
            }

            return returnVar;
        }
    }
}
