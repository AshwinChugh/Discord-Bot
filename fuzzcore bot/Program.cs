using System;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Discord.Rest;

namespace fuzzcore_bot
{
    public class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        //private string textDB = @"C:\Users\chugh\source\repos\fuzzcore bot\fuzzcore bot\bin\x64\Release\db.txt";
        private string textDB = @"C:\Program Files (x86)\blacklist\text.txt";
        //initilaize main discord bot components
        public static DiscordSocketClient client;
        public static CommandService Commands;
        public static List<ulong> whitelist = new List<ulong>();
        public static List<ulong> blackList = new List<ulong>();
        public struct captcha
        {
            public string code { get; set; }
            public string user { get; set; }
        }
        public static List<captcha> verifyList = new List<captcha>();
        private IServiceProvider Services;

        static void Main(string[] args)
            => new Program().Start().GetAwaiter().GetResult(); //program entry

        private async Task Start() //starts the bot
        {
            client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Info,
                AlwaysDownloadUsers = true,
                ConnectionTimeout = 5000,
                MessageCacheSize = 5000,
            });

        Commands = new CommandService();
            client.Log += Log; //begin logging for debugging during testing

            string token = "[DISCORD TOKEN HERE]"; //must be private

            Services = new ServiceCollection() //for command management and calling
                .BuildServiceProvider();

            await InstallCommands();
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync(); //login and connect to discord through the bot

            //set discord bot status
            await client.SetGameAsync("fuzzcore.xyz", null, ActivityType.Playing);
            await client.SetStatusAsync(UserStatus.Online);


            initializeBlacklist();
            initializeWhiteList();
            //banAll();
            await checkJoined();


            ////
            var handle = GetConsoleWindow();
            // Hide

            ShowWindow(handle, SW_HIDE);

//            // Show
//            ShowWindow(handle, SW_SHOW);
            //3600000
            await Task.Delay(-1); //keep bot running forever waiting for commands to pick up
        }

        public async Task InstallCommands()
        {
            //hook message recieved into our command handler
            //client.MessageReceived += testReaction;
            //client.MessageReceived += shutUp; --> really broken and doesnt work
            client.MessageReceived += HandleCommand;
            //discover all the commands and load them
            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        }


        public async Task testReaction(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            var context = new SocketCommandContext(client, message);
            var yesReaction = new Emoji("\u2705");
            var noReaction = new Emoji("\u274C");
            var channel = context.Guild.GetChannel(547018942175117332);

            //checks
            if (message == null || context.User.IsBot) return;//make sure it actually has a message or a command for the bot
            //547018942175117332 --> real
            //541746387134447618 --> test
            if (message.Channel.Id == 547018942175117332)
            {
                await message.AddReactionAsync(yesReaction);
                await message.AddReactionAsync(noReaction);
            }
        }


        public async Task HandleCommand(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            var context = new SocketCommandContext(client, message);

            int argPos = 0; //used to determine if the call is a command
            if (!(message.HasStringPrefix(".fc ", ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos))) return; //check if it is a command

            var result = await Commands.ExecuteAsync(context, argPos, Services);
            if (!result.IsSuccess)
                Console.WriteLine($"{DateTime.Now} at Commands] Something went wrong with executing a command. Text: {message.Content} | Error: {result.Error} | Error Reason: {result.ErrorReason}");
        }

        public async Task checkJoined()
        {
            client.UserJoined += Client_UserJoined;
        }

        public async Task checkLeave()
        {
            client.UserLeft += Client_UserLeft;
        }

        private void initializeBlacklist()//initializes from the file(works)
        {

            StreamReader BLRead = new StreamReader(textDB);
            var arrayRead = File.ReadAllLines(textDB);
            foreach (var line in arrayRead)
            {
                var id = UInt64.Parse(line);
                blackList.Add(id);
            }
        }

        void initializeWhiteList()//adds certain hardcoded userID values when the bot starts 
        {
            whitelist.Add(594953012737212466); //fizzy
            whitelist.Add(577617438347362339); //droes
            whitelist.Add(550071313528782867);//kuro
            whitelist.Add(594964741848563762); //greg
        }


        private async Task Client_UserJoined(SocketGuildUser userIn)
        {
            var fcGuild = client.Guilds.FirstOrDefault(x => x.Name.ToLower().Contains("fuzzcore"));
            var needsVerifyRole = fcGuild.Roles.FirstOrDefault(x => x.Name.ToLower().Equals("needs to verify"));
            var accountDateCreated = userIn.CreatedAt;
            var timePassed = (DateTimeOffset.Now - accountDateCreated).TotalHours;

            foreach (var banndUsr in blackList)
            {
                if (userIn.Id == banndUsr)
                {
                    await userIn.BanAsync(0, "Blacklisted User");
                    await fcGuild.TextChannels.FirstOrDefault(x => x.Name.ToLower().Equals("bot-logs"))
                        .SendMessageAsync(
                            "Banned blacklisted user: " + userIn.Username);
                    return;
                }
            }

            foreach (var id in whitelist)//let them join in whitelist
            {
                if (userIn.Id == id)
                {
                    try
                    {
                        await userIn.AddRoleAsync(needsVerifyRole);
                        await fcGuild.TextChannels.FirstOrDefault(x => x.Name.ToLower().Equals("bot-logs"))
                            .SendMessageAsync(
                                "Whitelisted User Joined: " + userIn.Username);
                        try
                        {
                            await userIn.SendMessageAsync(
                                "Welcome to FuzzCore! Please type ```.fc verify``` in the verify channel to ensure that you aren't a bot and to gain access to the server.");
                        }
                        catch (Exception e)
                        {
                            await fcGuild.TextChannels.FirstOrDefault(x => x.Name.ToLower().Equals("bot-logs"))
                                .SendMessageAsync(
                                    "Unable to send verification instructions in DMs to new user: " + userIn.Username + ". User has been given the needs to verify role anyways");
                        }
                    }
                    catch (Exception e)
                    {
                        await fcGuild.TextChannels.FirstOrDefault(x => x.Name.ToLower().Equals("bot-logs"))
                            .SendMessageAsync(
                                "Unable to add verified role to new users. Either I can not find the server or I can't find the role. Please contact the dev of this bot for a fix.");
                        Console.WriteLine(e + "\n source: " + e.Source);
                    }
                    return;
                }
            }

            #region Captcha

            string captchaCode = "";
            var user = new captcha();
            user.user = userIn.Username;
            captchaCode = genCaptcha();
            Console.WriteLine("Captcha code: " + captchaCode);
            user.code = captchaCode;
            verifyList.Add(user);
            #endregion

            if (timePassed < 100.0000000d)
            {
                await userIn.BanAsync();
                await fcGuild.TextChannels.FirstOrDefault(x => x.Name.ToLower().Equals("bot-logs"))
                    .SendMessageAsync(
                        "Banned new user: " + userIn.Username + ". That account is " + timePassed + " hours old. Very likely an alt and thus has been banned.");
                await fcGuild.TextChannels.FirstOrDefault(x => x.Name.ToLower().Equals("bot-logs"))
                    .SendMessageAsync(
                        "The account was created at: " + accountDateCreated.ToLocalTime() + " . If you think that was a mistake, unban and make sure the account is atleast 100 hours before being able to join the server again.");

                return;
            }
            else
            {
                await fcGuild.TextChannels.FirstOrDefault(x => x.Name.ToLower().Equals("bot-logs")).SendMessageAsync(
                    "User Joined: " + userIn.Username + "   . The account has existed for " + (int)timePassed + " hours.");
                await fcGuild.TextChannels.FirstOrDefault(x => x.Name.ToLower().Equals("bot-logs"))
                    .SendMessageAsync(userIn.Username + "'s Captcha Code is ```" + captchaCode + "```.");
                await fcGuild.TextChannels.FirstOrDefault(x => x.Name.ToLower().Equals("bot-logs"))
                    .SendMessageAsync("Total Users: " + fcGuild.MemberCount);
            }

            try
            {
                await userIn.AddRoleAsync(needsVerifyRole);
                
                try
                {
                    await userIn.SendMessageAsync(
                        "Welcome to FuzzCore! Please type ```.fc verify " + captchaCode + "``` in the verify channel to ensure that you aren't a bot and to gain access to the server.");
                }
                catch (Exception e)
                {
                    await fcGuild.TextChannels.FirstOrDefault(x => x.Name.ToLower().Equals("bot-logs"))
                        .SendMessageAsync(
                            "Unable to send verification instructions in DMs to new user: "+ userIn.Username + ". User has been given the needs to verify role anyways");
                }
            }
            catch (Exception e)
            {
                await fcGuild.TextChannels.FirstOrDefault(x => x.Name.ToLower().Equals("bot-logs"))
                    .SendMessageAsync(
                        "Unable to add verified role to new users. Either I can not find the server or I can't find the role. Please contact the dev of this bot for a fix.");
                Console.WriteLine(e + "\n source: " + e.Source);
            }
        }

        private async Task Client_UserLeft(SocketGuildUser userIn)//clear captcha registry if user leaves
        {
            foreach (var user in verifyList)
            {
                if (user.user == userIn.Username)
                    verifyList.Remove(user);
            }
        }

        string genCaptcha()
        {
            var randomChar = new Random();
            var resultString = new StringBuilder();
            for (int i=0; i<5; i++)
            {
                
                char letter = (char)randomChar.Next(65, 122);
                resultString.Append(letter.ToString());
            }
            for(int i=0; i<3; i++)
            {
                char number = (char)randomChar.Next(48, 57);
                resultString.Append(number.ToString());
            }
            return resultString.ToString();//generates first 5 random letters and then 3 random numbers
        }


        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }

}
