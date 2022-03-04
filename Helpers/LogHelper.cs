﻿using System;
using System.Threading.Tasks;
using Discord;

namespace Levante.Helpers
{
    public static class LogHelper
    {
        public async static Task Log(IMessageChannel Channel, string Message)
        {
            if (Channel == null) return;
            try
            {
                await Channel.SendMessageAsync($"> [{GetTimePrefix()}]: {Message}");
            }
            catch
            {
                ConsoleLog("Unable to send message.");
            }
        }

        public async static Task Log(IMessageChannel Channel, string Message, ComponentBuilder CB)
        {
            if (Channel == null) return;
            try
            {
                await Channel.SendMessageAsync($"> [{GetTimePrefix()}]: {Message}", components: CB.Build());
            }
            catch
            {
                ConsoleLog("Unable to send message.");
            }
        }

        public async static Task Log(IMessageChannel Channel, string Message, EmbedBuilder Embed)
        {
            if (Channel == null) return;
            try
            {
                await Channel.SendMessageAsync($"> [{GetTimePrefix()}]: {Message}", embed: Embed.Build());
            }
            catch
            {
                ConsoleLog("Unable to send message.");
            }
        }

        public async static Task Log(IMessageChannel Channel, string Message, EmbedBuilder Embed, ComponentBuilder CB)
        {
            if (Channel == null) return;
            try
            {
                await Channel.SendMessageAsync($"> [{GetTimePrefix()}]: {Message}", embed: Embed.Build(), components: CB.Build());
            }
            catch
            {
                ConsoleLog("Unable to send message.");
            }
        }

        // This is here for the future when developer logs eventually make it to a specific Discord Channel.
        public static void ConsoleLog(string Message) =>
            Console.WriteLine($"[{String.Format("{0:00}", DateTime.Now.Hour)}:" +
            $"{String.Format("{0:00}", DateTime.Now.Minute)}:" +
            $"{String.Format("{0:00}", DateTime.Now.Second)}] {Message}");

        private static TimestampTag GetTimePrefix() => TimestampTag.FromDateTime(DateTime.Now, TimestampTagStyles.LongTime);
    }
}
