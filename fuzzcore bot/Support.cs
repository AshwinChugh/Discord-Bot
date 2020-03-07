using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;


namespace fuzzcore_bot
{
    public class Support : ModuleBase<SocketCommandContext>
    {
        private string textTic = @"C:\Program Files (x86)\blacklist";
        private string embedImgLink = "https://i.imgur.com/XEnPLkm.gif";
        [Command("new", RunMode = RunMode.Async)]
        public async Task createNewSupportChannel(SocketGuildUser userIn, [Remainder] string issueIn)
        {
            SocketGuildUser user = userIn;

            bool channelExist = false;

            ulong ticketCategoryId = 0;

            ticketCategoryId = (Context.Guild.CategoryChannels.FirstOrDefault(x => x.Name.ToLower().Contains("tickets"))).Id;
            if (ticketCategoryId == 0)
            { await Context.Channel.SendMessageAsync("Unable to find ticket category."); return;}

            foreach (var channel in Context.Guild.TextChannels)
            {
                if (channel.CategoryId == ticketCategoryId)
                {
                    if (channel.Name == "ticket-" + user.Username.ToLower())
                    {
                        await Context.Channel.SendMessageAsync("You already have a support channel: " +
                                                               channel.Mention +
                                                               ". Please state your issue there, or close it and create a new one again.");
                        channelExist = true;
                        break;
                    }
                }
            }

            if (!channelExist)
            {
                var authorPerms = new OverwritePermissions(PermValue.Deny, PermValue.Deny, PermValue.Allow,
                    PermValue.Allow, PermValue.Allow,
                    PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Deny,
                    PermValue.Deny);

                var ownerRole = ((ITextChannel)Context.Channel).Guild.Roles.FirstOrDefault(x => x.Name.ToLower().Contains("owner"));
                var headStaff =
                    ((ITextChannel)Context.Channel).Guild.Roles.FirstOrDefault(x => x.Name.ToLower().Contains("head staff"));
                var staffRole =
                    ((ITextChannel) Context.Channel).Guild.Roles.FirstOrDefault(x => x.Name.ToLower().Equals("staff"));
                var trialStaff = ((ITextChannel)Context.Channel).Guild.Roles.FirstOrDefault(x => x.Name.ToLower().Contains("trial staff"));
                var everyoneRole = Context.Guild.EveryoneRole;

                var supportChannel =
                    ((ITextChannel) Context.Guild.Channels.FirstOrDefault(x => x.Name.ToLower().Contains("support")));
                if (supportChannel == null)//check that the channel exists
                {
                    await Context.Channel.SendMessageAsync(
                        "Hmm. Im not able to find a dedicated support channel to call from. Please make sure the support channel is called either 'tickets' or 'support' without the apostrophes.");
                    return;
                }

                if (Context.Message.Channel == supportChannel) //only make the command callable in the support channel
                {
                    var ticketChannel = await Context.Guild.CreateTextChannelAsync("ticket-" + user.Username, properties =>
                    {
                        properties.CategoryId = ticketCategoryId;//ticket category id
                        properties.Topic = "ticket";
                    });

                    //setting the perms for the channel itself
                    await ticketChannel.AddPermissionOverwriteAsync(user, authorPerms);//for the user that needs the help

                    await ticketChannel.AddPermissionOverwriteAsync(ownerRole,//all for the admins to help the respective user
                        OverwritePermissions.AllowAll(ticketChannel));
                    await ticketChannel.AddPermissionOverwriteAsync(headStaff,
                        OverwritePermissions.AllowAll(ticketChannel));
                    await ticketChannel.AddPermissionOverwriteAsync(staffRole,
                        OverwritePermissions.AllowAll(ticketChannel));
                    await ticketChannel.AddPermissionOverwriteAsync(trialStaff,
                        OverwritePermissions.AllowAll(ticketChannel));

                    await ticketChannel.AddPermissionOverwriteAsync(everyoneRole,
                        OverwritePermissions.DenyAll(ticketChannel));//deny everyone else access to this channel
                    try
                    {
                        await ticketChannel.SendMessageAsync(staffRole.Mention); //notify the admins for the new channel
                    }
                    catch (Exception e)
                    {
                        await ticketChannel.SendMessageAsync(
                            "Cant mention staff. Was there a role name change?");
                    }

                    await newTicketEmbed(ticketChannel, user, issueIn);

                    await userEmbedBuild(ticketChannel, supportChannel as SocketTextChannel, userIn);

                }
                else
                {
                    await Context.Channel.SendMessageAsync("Oops. You can't call this command here. Please try the " +
                                                           supportChannel.Mention + " to make a ticket.");
                }
            }
        }

        [Command("new", RunMode = RunMode.Async)]
        public async Task createNewSupportChannel(SocketGuildUser userIn)//overload without the issue reason
        {
            SocketGuildUser user = userIn;
            bool channelExist = false;

            ulong ticketCategoryId = 0;
            ticketCategoryId = (Context.Guild.CategoryChannels.FirstOrDefault(x => x.Name.ToLower().Contains("tickets"))).Id;
            if (ticketCategoryId == 0) { await Context.Channel.SendMessageAsync("Unable to find ticket category."); return; }//simple check

            foreach (var channel in Context.Guild.TextChannels)
            {
                if (channel.CategoryId == ticketCategoryId)
                {
                    if (channel.Name == "ticket-" + user.Username.ToLower())
                    {
                        await Context.Channel.SendMessageAsync("You already have a support channel: " + channel.Mention +
                                                               ". Please state your issue there, or close it and create a new one again.");
                        channelExist = true;
                        break;
                    }
                }
            }

            if (!channelExist)
            {
                var authorPerms = new OverwritePermissions(PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Allow, PermValue.Allow,
                PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Deny, PermValue.Deny);

                var ownerRole = ((ITextChannel)Context.Channel).Guild.Roles.FirstOrDefault(x => x.Name.ToLower().Contains("owner"));
                var headStaff =
                    ((ITextChannel)Context.Channel).Guild.Roles.FirstOrDefault(x => x.Name.ToLower().Contains("head staff"));
                var staffRole =
                    ((ITextChannel)Context.Channel).Guild.Roles.FirstOrDefault(x => x.Name.ToLower().Equals("staff"));
                var trialStaff = ((ITextChannel)Context.Channel).Guild.Roles.FirstOrDefault(x => x.Name.ToLower().Contains("trial staff"));
                var everyoneRole = Context.Guild.EveryoneRole;

                var supportChannel =
                    ((ITextChannel)Context.Guild.Channels.FirstOrDefault(x => x.Name.ToLower().Contains("support")));
                if (supportChannel == null)//check that the channel exists
                {
                    await Context.Channel.SendMessageAsync(
                        "Hmm. Im not able to find a dedicated support channel to call from. Please make sure the support channel is called either 'tickets' or 'support' without the apostrophes.");
                    return;
                }

                if (Context.Message.Channel == supportChannel) //only make the command callable in the support channel
                {
                    var ticketChannel = await Context.Guild.CreateTextChannelAsync("ticket-" + user.Username, properties =>
                    {
                        properties.CategoryId = ticketCategoryId;//ticket category id
                        properties.Topic = "ticket";
                    });

                    //setting the perms for the channel itself
                    await ticketChannel.AddPermissionOverwriteAsync(user, authorPerms);//for the user that needs the help

                    await ticketChannel.AddPermissionOverwriteAsync(ownerRole,//all for the admins to help the respective user
                        OverwritePermissions.AllowAll(ticketChannel));
                    await ticketChannel.AddPermissionOverwriteAsync(headStaff,
                        OverwritePermissions.AllowAll(ticketChannel));
                    await ticketChannel.AddPermissionOverwriteAsync(staffRole,
                        OverwritePermissions.AllowAll(ticketChannel));
                    await ticketChannel.AddPermissionOverwriteAsync(trialStaff,
                        OverwritePermissions.AllowAll(ticketChannel));

                    await ticketChannel.AddPermissionOverwriteAsync(everyoneRole,
                        OverwritePermissions.DenyAll(ticketChannel));//deny everyone else access to this channel
                    try
                    {
                        await ticketChannel.SendMessageAsync(staffRole.Mention); //notify the admins for the new channel
                    }
                    catch (Exception e)
                    {
                        await ticketChannel.SendMessageAsync(
                            "Unable to mention staff. Was there a role name change?");
                    }

                    await newTicketEmbed(ticketChannel, user);

                    await userEmbedBuild(ticketChannel, supportChannel as SocketTextChannel, userIn);

                }
                else
                {
                    await Context.Channel.SendMessageAsync("Oops. You can't call this command here. Please try the " +
                                                           supportChannel.Mention + " to make a ticket.");
                }
            }
        }

        [Command("new", RunMode = RunMode.Async)]
        public async Task createNewSupportChannel()//overload without the issue reason or the user
        {
            bool channelExist = false;
            SocketGuildUser user = Context.Message.Author as SocketGuildUser;
            ulong ticketCategoryId = 0;

            ticketCategoryId =
                ((ICategoryChannel) Context.Guild.CategoryChannels.FirstOrDefault(x =>
                    x.Name.ToLower().Equals("tickets"))).Id;
            if (ticketCategoryId == 0) { await Context.Channel.SendMessageAsync("Unable to find tickets category."); return;}

            foreach (var channel in Context.Guild.TextChannels)
            {
                if (channel.CategoryId == ticketCategoryId)
                {
                    if (channel.Name == "ticket-" + user.Username.ToLower())
                    {
                        await Context.Channel.SendMessageAsync("You already have a support channel: " + channel.Mention +
                                                               ". Please state your issue there, or close it and create a new one again.");
                        channelExist = true;
                        break;
                        
                    }
                }
            }

            if (!channelExist)
            {
                var authorPerms = new OverwritePermissions(PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Allow, PermValue.Allow,
                PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Deny, PermValue.Deny);

                var ownerRole = ((ITextChannel)Context.Channel).Guild.Roles.FirstOrDefault(x => x.Name.ToLower().Contains("owner"));
                var headStaff =
                    ((ITextChannel)Context.Channel).Guild.Roles.FirstOrDefault(x => x.Name.ToLower().Contains("head staff"));
                var staffRole =
                    ((ITextChannel)Context.Channel).Guild.Roles.FirstOrDefault(x => x.Name.ToLower().Equals("staff"));
                var trialStaff = ((ITextChannel)Context.Channel).Guild.Roles.FirstOrDefault(x => x.Name.ToLower().Contains("trial staff"));
                var everyoneRole = Context.Guild.EveryoneRole;

                var supportChannel =
                    ((ITextChannel)Context.Guild.Channels.FirstOrDefault(x => x.Name.ToLower().Contains("support")));
                if (supportChannel == null)//check that the channel exists
                {
                    await Context.Channel.SendMessageAsync(
                        "Hmm. Im not able to find a dedicated support channel to call from. Please make sure the support channel is called either 'tickets' or 'support' without the apostrophes.");
                    return;
                }

                if (Context.Message.Channel == supportChannel) //only make the command callable in the support channel
                {
                    var ticketChannel = await Context.Guild.CreateTextChannelAsync("ticket-" + user.Username, properties =>
                    {
                        properties.CategoryId = ticketCategoryId;//ticket category id
                        properties.Topic = "ticket";
                    });

                    //setting the perms for the channel itself
                    await ticketChannel.AddPermissionOverwriteAsync(user, authorPerms);//for the user that needs the help

                    await ticketChannel.AddPermissionOverwriteAsync(ownerRole,//all for the admins to help the respective user
                        OverwritePermissions.AllowAll(ticketChannel));
                    await ticketChannel.AddPermissionOverwriteAsync(headStaff,
                        OverwritePermissions.AllowAll(ticketChannel));
                    await ticketChannel.AddPermissionOverwriteAsync(staffRole,
                        OverwritePermissions.AllowAll(ticketChannel));
                    await ticketChannel.AddPermissionOverwriteAsync(trialStaff,
                        OverwritePermissions.AllowAll(ticketChannel));

                    await ticketChannel.AddPermissionOverwriteAsync(everyoneRole,
                        OverwritePermissions.DenyAll(ticketChannel));//deny everyone else access to this channel
                    try
                    {
                        await ticketChannel.SendMessageAsync(staffRole.Mention); //notify the admins for the new channel
                    }
                    catch (Exception e)
                    {
                        await ticketChannel.SendMessageAsync("Unable to mention staff. Was there a role name change?");
                    }
                    

                    await newTicketEmbed(ticketChannel, user);
                    await embedBuild(ticketChannel, supportChannel as SocketTextChannel);
                }
                else
                {
                    await Context.Channel.SendMessageAsync("Oops. You can't call this command here. Please try the " +
                                                           supportChannel.Mention + " to make a ticket.");
                }
            }
        }

        [Command("new", RunMode = RunMode.Async)]
        public async Task createNewSupportChannel([Remainder] string issueIn)//overload without the user in
        {
            ulong ticketCategoryId = 0;
            ticketCategoryId = ((ICategoryChannel) Context.Guild.CategoryChannels.FirstOrDefault(x =>
                    x.Name.ToLower().Equals("tickets"))).Id;

            if (ticketCategoryId == 0) { await Context.Channel.SendMessageAsync("Unable to find ticket category."); return;}

            bool channelExist = false;
            
            var user = Context.Message.Author as SocketGuildUser;

            foreach (var channel in Context.Guild.TextChannels)
            {
                if (channel.CategoryId == ticketCategoryId)
                {
                    if (channel.Name == "ticket-" + user.Username.ToLower())
                    {
                        await Context.Channel.SendMessageAsync("You already have a support channel: " + channel.Mention +
                                                         ". Please state your issue there, or close it and create a new one again.");
                        channelExist = true;
                        break;
                    }
                }
            }

            if (!channelExist)
            {
                var authorPerms = new OverwritePermissions(PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Allow, PermValue.Allow,
                PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Deny, PermValue.Deny);

                var ownerRole = ((ITextChannel)Context.Channel).Guild.Roles.FirstOrDefault(x => x.Name.ToLower().Contains("owner"));
                var headStaff =
                    ((ITextChannel)Context.Channel).Guild.Roles.FirstOrDefault(x => x.Name.ToLower().Contains("head staff"));
                var staffRole =
                    ((ITextChannel)Context.Channel).Guild.Roles.FirstOrDefault(x => x.Name.ToLower().Equals("staff"));
                var trialStaff = ((ITextChannel)Context.Channel).Guild.Roles.FirstOrDefault(x => x.Name.ToLower().Contains("trial staff"));
                var everyoneRole = Context.Guild.EveryoneRole;

                var supportChannel =
                    ((ITextChannel)Context.Guild.Channels.FirstOrDefault(x => x.Name.ToLower().Contains("support")));
                if (supportChannel == null)//check that the channel exists
                {
                    await Context.Channel.SendMessageAsync(
                        "Hmm. Im not able to find a dedicated support channel to call from. Please make sure the support channel is called either 'tickets' or 'support' without the apostrophes.");
                    return;
                }

                if (Context.Message.Channel == supportChannel) //only make the command callable in the support channel
                {
                    var ticketChannel = await Context.Guild.CreateTextChannelAsync("ticket-" + user.Username, properties =>
                    {
                        properties.CategoryId = ticketCategoryId;//ticket category id
                        properties.Topic = "ticket";
                    });

                    //setting the perms for the channel itself
                    await ticketChannel.AddPermissionOverwriteAsync(user, authorPerms);//for the user that needs the help

                    await ticketChannel.AddPermissionOverwriteAsync(ownerRole,//all for the admins to help the respective user
                        OverwritePermissions.AllowAll(ticketChannel));
                    await ticketChannel.AddPermissionOverwriteAsync(headStaff,
                        OverwritePermissions.AllowAll(ticketChannel));
                    await ticketChannel.AddPermissionOverwriteAsync(trialStaff,
                        OverwritePermissions.AllowAll(ticketChannel));
                    await ticketChannel.AddPermissionOverwriteAsync(staffRole,
                        OverwritePermissions.AllowAll(ticketChannel));

                    await ticketChannel.AddPermissionOverwriteAsync(everyoneRole,
                        OverwritePermissions.DenyAll(ticketChannel));//deny everyone else access to this channel
                    try
                    {
                        await ticketChannel.SendMessageAsync(staffRole.Mention); //notify the admins for the new channel
                    }
                    catch (Exception e)
                    {
                        await ticketChannel.SendMessageAsync(
                            "Unable to mention staff. Was there a role name change?");
                    }

                    await newTicketEmbed(ticketChannel, user, issueIn);

                    await embedBuild(ticketChannel, supportChannel as SocketTextChannel);

                }
                else
                {
                    await Context.Channel.SendMessageAsync("Oops. You can't call this command here. Please try the " +
                                                           supportChannel.Mention + " to make a ticket.");
                }
            }
        }

        private async Task embedBuild(RestTextChannel channelIn, SocketTextChannel contextChannelIn)
        {
            var ticketCreatedEmbed = new EmbedBuilder();
            ticketCreatedEmbed.WithAuthor("Fuzzcore Ticket System", embedImgLink);
            ticketCreatedEmbed.WithDescription("Hello " + Context.Message.Author.Username +
                                               ". I have created a support channel/ticket for you in " + channelIn.Mention);
            ticketCreatedEmbed.WithFooter("Fuzzcore Bot", embedImgLink);
            ticketCreatedEmbed.WithColor(color: Color.Blue);
            ticketCreatedEmbed.WithCurrentTimestamp();
            await contextChannelIn.SendMessageAsync("", false, ticketCreatedEmbed.Build());
        }

        private async Task userEmbedBuild(RestTextChannel channelIn, SocketTextChannel conTextChannelIn, SocketGuildUser userIn)
        {
            var ticketCreatedEmbed = new EmbedBuilder();
            ticketCreatedEmbed.WithAuthor("Fuzzcore Ticket System", embedImgLink);
            ticketCreatedEmbed.WithDescription("Hello " + Context.Message.Author.Username +
                                               ". I have created a support channel/ticket for " + userIn.Username +
                                               " in " + channelIn.Mention);
            ticketCreatedEmbed.WithFooter("Fuzzcore Bot", embedImgLink);
            ticketCreatedEmbed.WithCurrentTimestamp();
            ticketCreatedEmbed.WithColor(Color.Blue);
            await conTextChannelIn.SendMessageAsync("", false, ticketCreatedEmbed.Build());
        }

        private async Task newTicketEmbed(RestTextChannel channelIn, SocketGuildUser userIn, string subject)
        {
            var welcomeEmbed = new EmbedBuilder();
            welcomeEmbed.WithTitle(userIn.Username + "'s Ticket");
            welcomeEmbed.WithDescription("Dear " + userIn.Mention + "," + Environment.NewLine +
                                         "Here is your personal ticket channel. The admins will respond as soon as they can. " +
                                         "If your issue has been resolved, you can close your ticket by typing `fc close`. Thank you for choosing Fuzzcore.");
            welcomeEmbed.AddField("Issue", subject);
            welcomeEmbed.WithColor(Color.Blue);
            await channelIn.SendMessageAsync("", false, welcomeEmbed.Build());
        }

        private async Task newTicketEmbed(RestTextChannel channelIn, SocketGuildUser userIn)
        {
            var welcomeEmbed = new EmbedBuilder();
            welcomeEmbed.WithTitle(userIn.Username + "'s Ticket");
            welcomeEmbed.WithDescription("Dear " + userIn.Mention + "," + Environment.NewLine +
                                         "Here is your personal ticket channel. The admins will respond as soon as they can.");
            welcomeEmbed.AddField("Issue", "You haven't stated an issue when you called the command. Please state your issue now so that the admins can get right to " +
                                           "helping you. If your issue has been resolved, you can close the ticket by typing ` .fc close `. Thank you for choosing Fuzzcore.");
            welcomeEmbed.WithColor(Color.Blue);
            await channelIn.SendMessageAsync("", false, welcomeEmbed.Build());
        }

        [Command("close", RunMode = RunMode.Async)]
        public async Task closeChannel()
        {
            var ticketCategory =
                ((ICategoryChannel) Context.Guild.CategoryChannels.FirstOrDefault(x =>
                    x.Name.ToLower().Equals("tickets")));

            var ticketLogChannel =
                ((ITextChannel) Context.Guild.Channels.FirstOrDefault(x => x.Name.ToLower().Equals("ticket-logs")));

            var deletedTicketEmbed = new EmbedBuilder();
            deletedTicketEmbed.WithTitle("Ticket Deleted");
            deletedTicketEmbed.WithDescription("Ticket channel: " + Context.Channel.Name + " has been deleted by " +
                                               Context.Message.Author.Username + ".");
            deletedTicketEmbed.WithCurrentTimestamp();
            deletedTicketEmbed.WithFooter("Fuzzcore Bot", embedImgLink);
            
            SocketTextChannel channel = Context.Channel as SocketTextChannel;
            if (channel.Topic == "ticket")
            {
                StreamWriter logFile;
                var messages = channel.GetCachedMessages(1000).ToString();

                await Context.Channel.SendMessageAsync("Support channel will be closed in 5 seconds.");
                try
                {
                    await ticketLogChannel.SendMessageAsync("", false, deletedTicketEmbed.Build());
                }
                catch (Exception e)
                {
                    await Context.Channel.SendMessageAsync("Unable to log ticket closing. Will close channel anyway.");
                }

                await Task.Delay(5000);
                await channel.DeleteAsync();
            }
            else
            {
                await Context.Channel.SendMessageAsync(
                    "[ERROR] Channel is not a ticket/support channel. If you think this was an issue, please contact Auschw1n");
            }
        }

        [Command("add", RunMode = RunMode.Async)]
        public async Task addUser(SocketGuildUser userIn)
        {
            var channel = Context.Channel as SocketTextChannel;

            if (channel.Topic.ToLower().Contains("ticket"))
            {
                await channel.AddPermissionOverwriteAsync(userIn,
                    new OverwritePermissions(PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Allow,
                        PermValue.Allow, PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Allow, PermValue.Allow,
                        PermValue.Deny));
                await Context.Channel.SendMessageAsync("Added user to ticket: " + userIn.Username);
            }
            else
            {
                await Context.Channel.SendMessageAsync(
                    "Hmmm. I don't think this is a ticket. If this was a mistake, please contact Auschw1n.");
            }
        }

        [Command("summon", RunMode = RunMode.Async)]
        public async Task summon()
        {
            SocketTextChannel channel = Context.Channel as SocketTextChannel;
            if (channel.Topic == "ticket")
            {
                var fizzy = (IUser)Context.Guild.Users.FirstOrDefault(x => x.Username.ToLower().Contains("fizzy"));
                try
                {
                    await fizzy.SendMessageAsync("You have been summoned in " + Context.Channel.Name);
                }
                catch (Exception e)
                {
                    await Context.Channel.SendMessageAsync(
                        "Unable to summon Fizzy. It seems that pinging him is our only option now.");
                }

                await Context.Channel.SendMessageAsync("The lord has been summoned. If he is asleep, it might take him longer to wake up and bring about true justice.");
            }
            else
            {
                await Context.Channel.SendMessageAsync(
                    "I can only summon the lord for when times are truly desperate.");
            }
            
        }
    }
}
