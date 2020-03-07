using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;


namespace fuzzcore_bot.Commands
{
    public class BespooferCommand : ModuleBase<SocketCommandContext>
    {
        private string embedImgLink = "https://i.imgur.com/XEnPLkm.gif";
        [Command("bespoofer"), Alias("be", "r6s", "battleye")]
        public async Task beSpooferInfo()
        {
            EmbedBuilder beSpooferInfoEmbed = new EmbedBuilder();
            beSpooferInfoEmbed.WithColor(73, 221, 255);
            beSpooferInfoEmbed.WithAuthor("BE Spoofer");
            beSpooferInfoEmbed.AddField("About", "Spoofs your HWID (Hardware ID)");
            beSpooferInfoEmbed.AddField("Day Price", "Not Available", true);
            beSpooferInfoEmbed.AddField("Week Price", "£5", true);
            beSpooferInfoEmbed.AddField("Month Price", "£20", true);
            beSpooferInfoEmbed.AddField("Lifetime", "£40", true);
            beSpooferInfoEmbed.WithFooter("Fuzzcore Bot", embedImgLink);
            beSpooferInfoEmbed.WithCurrentTimestamp();
            await Context.Channel.SendMessageAsync("", false, beSpooferInfoEmbed.Build());
        }
    }
}
