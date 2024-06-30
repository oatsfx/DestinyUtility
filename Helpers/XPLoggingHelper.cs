﻿using Discord;
using Levante.Configs;
using System;

namespace Levante.Helpers
{
    public class XPLoggingHelper
    {
        public static ComponentBuilder GenerateChannelButtons(ulong DiscordID)
        {
            Emoji deleteEmote = new Emoji("⛔");
            Emoji restartEmote = new Emoji("😴");

            var buttonBuilder = new ComponentBuilder()
                .WithButton("Delete Log Channel", customId: $"deleteChannel", ButtonStyle.Secondary, deleteEmote, row: 0)
                .WithButton("Restart Logging", customId: $"restartLogging:{DiscordID}", ButtonStyle.Secondary, restartEmote, row: 0);

            return buttonBuilder;
        }

        public static EmbedBuilder GenerateSessionSummary(ActiveConfig.ActiveAFKUser aau)
        {
            var auth = new EmbedAuthorBuilder()
            {
                Name = $"Session Summary: {aau.UniqueBungieName}",
                IconUrl = BotConfig.BotAvatarUrl,
            };
            var foot = new EmbedFooterBuilder()
            {
                Text = $"XP Logging Session Summary"
            };
            var embed = new EmbedBuilder()
            {
                Color = new Discord.Color(BotConfig.EmbedColor.R, BotConfig.EmbedColor.G, BotConfig.EmbedColor.B),
                Author = auth,
                Footer = foot,
            };
            int powerBonusGained = aau.Last.PowerBonus - aau.Start.PowerBonus;
            var levelCap = ManifestHelper.CurrentLevelCap;
            var nextLevelAt = aau.Last.NextLevelAt;

            int levelsGained = (aau.Last.Level - aau.Start.Level) + (aau.Last.ExtraLevel - aau.Start.ExtraLevel);
            long xpGained = 0;
            // If the user hit the cap during the session, calculate gained XP differently.
            if (aau.Start.Level < levelCap && aau.Last.Level >= levelCap)
            {
                int levelsToCap = levelCap - aau.Start.Level;
                int levelsPastCap = levelsGained - levelsToCap;
                // StartNextLevelAt should be the 100,000 or whatever Bungie decides to change it to later on.
                xpGained = ((levelsToCap) * aau.Start.NextLevelAt) + (levelsPastCap * nextLevelAt) - aau.Start.LevelProgress + aau.Last.LevelProgress;
            }
            else // The nextLevelAt stays the same because it did not change mid-logging session.
            {
                xpGained = (levelsGained * nextLevelAt) - aau.Start.LevelProgress + aau.Last.LevelProgress;
            }
            
            var timeSpan = DateTime.Now - aau.Start.Timestamp;
            string timeString = $"{(Math.Floor(timeSpan.TotalHours) > 0 ? $"{Math.Floor(timeSpan.TotalHours)}h " : "")}" +
                    $"{(timeSpan.Minutes > 0 ? $"{timeSpan.Minutes:00}m " : "")}" +
                    $"{timeSpan.Seconds:00}s";
            int xpPerHour = (int)Math.Floor(xpGained / (DateTime.Now - aau.Start.Timestamp).TotalHours);
            embed.WithCurrentTimestamp();
            embed.Description = $"Time Logged: {timeString}\n";

            embed.AddField(x =>
            {
                x.Name = "Level Information";
                x.Value = $"Start: {aau.Start.Level}{(aau.Start.ExtraLevel > 0 ? $" (+{aau.Start.ExtraLevel})" : "")} ({aau.Start.LevelProgress:n0}/{aau.Start.NextLevelAt:n0})\n" +
                    $"Now: {aau.Last.Level}{(aau.Last.ExtraLevel > 0 ? $" (+{aau.Last.ExtraLevel})" : "")} ({aau.Last.LevelProgress:n0}/{aau.Last.NextLevelAt:n0})\n" +
                    $"Gained: {levelsGained}\n";
                x.IsInline = true;
            }).AddField(x =>
            {
                x.Name = "XP Information";
                x.Value = $"Gained: {xpGained:n0}\n" +
                    $"XP Per Hour: {xpPerHour:n0}";
                x.IsInline = true;
            }).AddField(x =>
            {
                x.Name = "Power Bonus Information";
                x.Value = $"Start: {aau.Start.PowerBonus}\n" +
                    $"Now: {aau.Last.PowerBonus}\n" +
                    $"Gained: {powerBonusGained}\n";
                x.IsInline = true;
            });

            return embed;
        }
    }
}
