using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace fuzzcore_bot
{
    public class helpcommand : ModuleBase<SocketCommandContext>
    {
        private string embedImgLink = "https://i.imgur.com/XEnPLkm.gif";
        [Command("help"), Alias("h")]
        public async Task helpCommandList()
        { 
            var helpEmbed = new EmbedBuilder();
            helpEmbed.WithAuthor("Fuzzcore");
            helpEmbed.WithColor(73, 221, 255);
            helpEmbed.WithFooter("Fuzzcore Bot", embedImgLink);
            helpEmbed.WithCurrentTimestamp();
            helpEmbed.WithTitle("Commands List");
            helpEmbed.AddField("'.fc' keyword",
                "By calling the '.fc' before a command lets the bot know you are talking with it. Use '.fc' if you want a response from the bot.");
            helpEmbed.AddField(".fc bespoofer",
                "This command reveals all the prices and information about the called spoofer. You can also call this command alternatively by using '.fc be', '.fc r6s', '.fc battleye'. ");
            helpEmbed.AddField(".fc medal @user", "Ever get annoyed of someone? Use this command to tell them to be quiet in style :sunglasses: :sunglasses:");
            helpEmbed.AddField(".fc info @user", "Grabs and displays generic information about the user.");
            helpEmbed.WithDescription(
                "Click on the arrows to browse the next help page. Only the message author can call this.");


            var helpMessage = await Context.Channel.SendMessageAsync("", false, helpEmbed.Build());
            await helpMessage.AddReactionAsync(new Emoji("U+2B05"));
            await helpMessage.AddReactionAsync(new Emoji("U+27A1"));
        }

        public async Task adminHelpCommandList() //NEEDS TO BE WORKED ON
        {
        }
    }
}
