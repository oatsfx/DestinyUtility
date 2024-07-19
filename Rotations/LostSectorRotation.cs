﻿using Discord;
using Levante.Configs;
using Levante.Rotations.Abstracts;
using Levante.Rotations.Interfaces;
using Levante.Util;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Levante.Rotations
{
    public class LostSectorRotation : SetRotation<LostSector, LostSectorLink, LostSectorPrediction>
    {
        private string ArmorRotationFilePath;

        public List<ExoticArmor> ArmorRotations = new();

        public LostSectorRotation()
        {
            FilePath = @"Trackers/lostSector.json";
            RotationFilePath = @"Rotations/lostSector.json";
            ArmorRotationFilePath = @"Rotations/lostSectorArmor.json";

            IsDaily = true;

            GetRotationJSON();
            GetTrackerJSON();
        }

        public new void GetRotationJSON()
        {
            if (File.Exists(RotationFilePath))
            {
                string json = File.ReadAllText(RotationFilePath);
                Rotations = JsonConvert.DeserializeObject<List<LostSector>>(json);
            }
            else
            {
                File.WriteAllText(RotationFilePath, JsonConvert.SerializeObject(Rotations, Formatting.Indented));
                Log.Warning("No {RotationFilePath} file detected; it has been created for you. No action is needed.", RotationFilePath);
            }

            if (File.Exists(ArmorRotationFilePath))
            {
                string json = File.ReadAllText(ArmorRotationFilePath);
                ArmorRotations = JsonConvert.DeserializeObject<List<ExoticArmor>>(json);
            }
            else
            {
                File.WriteAllText(ArmorRotationFilePath, JsonConvert.SerializeObject(ArmorRotations, Formatting.Indented));
                Log.Warning("No {ArmorRotationFilePath} file detected; it has been created for you. No action is needed.", ArmorRotationFilePath);
            }
        }

        public EmbedBuilder GetLostSectorEmbed(int LS, LostSectorDifficulty LSD)
        {
            var LostSector = Rotations[LS];

            var auth = new EmbedAuthorBuilder()
            {
                Name = $"{(LSD == LostSectorDifficulty.Legend ? "Legend" : "Master")} Lost Sector",
            };
            var foot = new EmbedFooterBuilder()
            {
                Text = $"{LostSector.Location}"
            };
            var embed = new EmbedBuilder()
            {
                Color = new Discord.Color(AppConfig.Discord.EmbedColor.R, AppConfig.Discord.EmbedColor.G, AppConfig.Discord.EmbedColor.B),
                Author = auth,
                Footer = foot,
            };
            embed.AddField(y =>
            {
                y.Name = LSD == LostSectorDifficulty.Legend ? "Legend" : "Master";
                y.Value = $"Recommended Power: {DestinyEmote.Light}{GetLostSectorDifficultyLight(LSD)}\n" +
                    $"Threat: {DestinyEmote.MatchEmote(LostSector.Burn)}{LostSector.Burn}";
                y.IsInline = false;
            })
            .AddField(y =>
            {
                y.Name = "Champions";
                y.Value = LostSector.GetChampions(LSD);
                y.IsInline = true;
            })
            .AddField(y =>
            {
                y.Name = "Modifiers";
                y.Value = LostSector.GetModifiers(LSD);
                y.IsInline = true;
            })
            .AddField(y =>
            {
                y.Name = "Shields";
                y.Value = LostSector.GetShields(LSD);
                y.IsInline = true;
            });

            embed.Title = $"{LostSector.Name}";
            embed.ImageUrl = LostSector.PGCRImage;
            embed.ThumbnailUrl = "https://www.bungie.net/common/destiny2_content/icons/6a2761d2475623125d896d1a424a91f9.png";

            return embed;
        }

        public static string GetLostSectorDifficultyLight(LostSectorDifficulty lsd)
        {
            return lsd switch
            {
                LostSectorDifficulty.Legend => "1830",
                LostSectorDifficulty.Master => "1840",
                _ => "",
            };
        }

        public LostSectorPrediction DatePrediction(int LS, int ArmorType, int Skip)
        {
            int iterationEAT = CurrentRotations.Actives.LostSectorArmorDrop;
            int iterationLS = CurrentRotations.Actives.LostSector;
            int correctIterations = -1;
            int DaysUntil = 0;

            if (LS == -1 && ArmorType != -1)
            {
                do
                {
                    iterationEAT = iterationEAT == ArmorRotations.Count - 1 ? 0 : iterationEAT + 1;
                    iterationLS = iterationLS == Rotations.Count - 1 ? 0 : iterationLS + 1;
                    DaysUntil++;
                    if (iterationEAT == ArmorType)
                        correctIterations++;

                } while (Skip != correctIterations);
            }
            else if (ArmorType == -1 && LS != -1)
            {
                do
                {
                    iterationEAT = iterationEAT == ArmorRotations.Count - 1 ? 0 : iterationEAT + 1;
                    iterationLS = iterationLS == Rotations.Count - 1 ? 0 : iterationLS + 1;
                    DaysUntil++;
                    if (iterationLS == LS)
                        correctIterations++;

                } while (Skip != correctIterations);
            }
            else if (ArmorType != -1 && LS != -1)
            {
                do
                {
                    iterationEAT = iterationEAT == ArmorRotations.Count - 1 ? 0 : iterationEAT + 1;
                    iterationLS = iterationLS == Rotations.Count - 1 ? 0 : iterationLS + 1;
                    DaysUntil++;
                    if (iterationEAT == ArmorType && iterationLS == LS)
                        correctIterations++;

                } while (Skip != correctIterations);
            }
            return new LostSectorPrediction { LostSector = Rotations[iterationLS], ExoticArmor = ArmorRotations[iterationEAT], Date = CurrentRotations.Actives.DailyResetTimestamp.AddDays(DaysUntil) };
        }

        public override bool IsTrackerInRotation(LostSectorLink Tracker)
        {
            if (Tracker.LostSector == -1)
                return Tracker.ArmorDrop == CurrentRotations.Actives.LostSectorArmorDrop;
            else if (Tracker.ArmorDrop == -1)
                return Tracker.LostSector == CurrentRotations.Actives.LostSector;
            else
                return Tracker.LostSector == CurrentRotations.Actives.LostSector && Tracker.ArmorDrop == CurrentRotations.Actives.LostSectorArmorDrop;
        }

        public override LostSectorPrediction DatePrediction(int Rotation, int Skip)
        {
            throw new NotImplementedException();
        }

        public override string ToString() => "Lost Sector";
    }

    public class LostSector
    {
        public string Name;
        public string Location;
        public string PGCRImage;
        [JsonProperty("LegendActivityHash")]
        public long LegendActivityHash;
        [JsonProperty("MasterActivityHash")]
        public long MasterActivityHash;
        [JsonProperty("Burn")]
        public string Burn;
        // <Champion Type, Count>
        [JsonProperty("LegendChampions")]
        public Dictionary<string, int> LegendChampions;
        [JsonProperty("MasterChampions")]
        public Dictionary<string, int> MasterChampions;
        // <Shield Type, Count>
        [JsonProperty("LegendShields")]
        public Dictionary<string, int> LegendShields;
        [JsonProperty("MasterShields")]
        public Dictionary<string, int> MasterShields;

        public List<string> LegendModifiers = new();
        public List<string> MasterModifiers = new();

        public string GetModifiers(LostSectorDifficulty Difficulty)
        {
            string json = File.ReadAllText(EmoteConfig.FilePath);
            var emoteCfg = JsonConvert.DeserializeObject<EmoteConfig>(json);
            string result = "";

            var modifers = Difficulty == LostSectorDifficulty.Legend ? LegendModifiers : MasterModifiers;
            foreach (var modifier in modifers)
                result += $"{emoteCfg.GetEmote(modifier.Replace(" ", "").Replace("-", "").Replace("'", ""))}{modifier}\n";
            return result;
        }

        public string GetChampions(LostSectorDifficulty Difficulty)
        {
            string result = "";
            if (Difficulty == LostSectorDifficulty.Legend)
                foreach (var champion in LegendChampions)
                    result += $"{DestinyEmote.MatchEmote(champion.Key)}{champion.Value} ({champion.Key})\n";
            else
                foreach (var champion in MasterChampions)
                    result += $"{DestinyEmote.MatchEmote(champion.Key)}{champion.Value} ({champion.Key})\n";

            return result;
        }

        public string GetShields(LostSectorDifficulty Difficulty)
        {
            string result = "";
            if (Difficulty == LostSectorDifficulty.Legend)
                foreach (var shield in LegendShields)
                    result += $"{DestinyEmote.MatchEmote(shield.Key)}{shield.Value}\n";
            else
                foreach (var shield in MasterShields)
                    result += $"{DestinyEmote.MatchEmote(shield.Key)}{shield.Value}\n";

            return result;
        }

        public override string ToString() => $"{Name} ({Location})";
    }

    public enum LostSectorDifficulty
    {
        Legend,
        Master
    }

    public class ExoticArmor
    {
        [JsonProperty("Type")]
        public readonly string Type;

        [JsonProperty("ArmorEmote")]
        public readonly string ArmorEmote;

        public override string ToString() => $"{Type}";
    }

    public class LostSectorLink : IRotationTracker
    {
        [JsonProperty("DiscordID")]
        public ulong DiscordID { get; set; } = 0;

        [JsonProperty("Encounter")]
        public int LostSector { get; set; } = 0;

        [JsonProperty("ArmorDrop")]
        public int ArmorDrop { get; set; } = 0;

        public override string ToString()
        {
            string result = "Lost Sector";
            if (LostSector >= 0)
                result = $"{CurrentRotations.LostSector.Rotations[LostSector]}";

            if (ArmorDrop >= 0)
                result += $" dropping {CurrentRotations.LostSector.ArmorRotations[ArmorDrop]}";

            return result;
        }
    }

    public class LostSectorPrediction : IRotationPrediction
    {
        public DateTime Date { get; set; }
        public LostSector LostSector { get; set; }
        public ExoticArmor ExoticArmor { get; set; }
    }
}
