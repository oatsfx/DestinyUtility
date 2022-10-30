﻿using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static Levante.Configs.BotConfig;

namespace Levante.Configs
{
    public sealed class BotConfig : IConfig
    {
        public static string FilePath { get; } = @"Configs/botConfig.json";

        [JsonProperty("AppName")]
        public static string AppName { get; set; } = "Levante";

        [JsonProperty("Website")]
        public static string Website { get; set; } = "oatsfx.com";

        // Include the @ in this field as the string this is used in, does not include the @.         
        [JsonProperty("Twitter")]
        public static string Twitter { get; set; } = "@OatsFX";

        // Exclude the https://discord.gg/ and only include the ending.
        [JsonProperty("SupportServer")]
        public static string SupportServer { get; set; } = "Levante";

        [JsonProperty("DiscordToken")]
        public static string DiscordToken { get; set; } = "[YOUR TOKEN HERE]";

        [JsonProperty("LogChannel")]
        public static ulong LogChannel { get; set; } = 0;

        public static SocketTextChannel LoggingChannel { get; set; } = null;

        [JsonProperty("CommunityCreationsLogChannel")]
        public static ulong CommunityCreationsLogChannel { get; set; } = 0;

        public static SocketTextChannel CreationsLogChannel { get; set; } = null;

        [JsonProperty("BungieApiKey")]
        public static string BungieApiKey { get; set; } = "[YOUR API KEY HERE]";

        [JsonProperty("BungieClientID")]
        public static string BungieClientID { get; set; } = "[YOUR CLIENT ID HERE]";

        [JsonProperty("BungieClientSecret")]
        public static string BungieClientSecret { get; set; } = "[YOUR CLIENT SECRET HERE]";

        [JsonProperty("Version")]
        public static string Version { get; set; } = "1.0.0";

        [JsonProperty("DefaultCommandPrefix")]
        public static string DefaultCommandPrefix { get; set; } = "l!";

        [JsonProperty("Note")]
        public static List<string> Notes { get; set; } = new();

        [JsonProperty("DurationToWaitForNextMessage")]
        public static int DurationToWaitForNextMessage { get; set; } = 20;

        [JsonProperty("DevServerID")]
        public static ulong DevServerID { get; set; } = 0;

        [JsonProperty("BotStaff")]
        public static List<ulong> BotStaffDiscordIDs { get; set; } = new();

        [JsonProperty("BotSupporters")]
        public static List<ulong> BotSupportersDiscordIDs { get; set; } = new();

        [JsonProperty("EmbedColor")]
        public static EmbedColorGroup EmbedColor { get; set; } = new();

        [JsonProperty("UniversalCodes")]
        public static List<UniversalCode> UniversalCodes { get; set; } = new();

        [JsonProperty("Hashes")]
        public static HashesList Hashes { get; set; } = new();

        // <Hash, Name>
        [JsonProperty("SeasonalCurrencyHashes")]
        public static Dictionary<long, string> SeasonalCurrencyHashes { get; set; } = new();

        public static string BotLogoUrl = "https://www.levante.dev/images/Levante-Logo.png";
        public static string BotAvatarUrl = "https://www.levante.dev/images/Levante-Avatar.png";

        public class EmbedColorGroup
        {
            [JsonProperty("R")]
            public static int R { get; set; } = 0;

            [JsonProperty("G")]
            public static int G { get; set; } = 0;

            [JsonProperty("B")]
            public static int B { get; set; } = 0;
        }

        public class UniversalCode
        {
            [JsonProperty("Name")]
            public string Name { get; set; } = "[EMBLEM NAME]";

            [JsonProperty("ImageUrl")]
            public string ImageUrl { get; set; } = "[IMAGE URL]";

            [JsonProperty("Code")]
            public string Code { get; set; } = "[EMBLEM CODE]";
        }

        public class HashesList
        {
            [JsonProperty("First100Ranks")]
            public string First100Ranks { get; set; } = "[HASH]";

            [JsonProperty("Above100Ranks")]
            public string Above100Ranks { get; set; } = "[HASH]";
        }

        public static bool IsSupporter(ulong DiscordID) => BotSupportersDiscordIDs.Contains(DiscordID);
    }
}
