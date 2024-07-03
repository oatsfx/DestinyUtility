﻿using BungieSharper.Entities.Destiny.Definitions;
using Levante.Configs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using BungieSharper.Entities.Destiny;
using BungieSharper.Entities.Destiny.Definitions.Records;
using Levante.Rotations;
using BungieSharper.Entities.Destiny.Definitions.ActivityModifiers;
using BungieSharper.Entities.Destiny.Definitions.Presentation;
using Levante.Util;
using System.IO;
using BungieSharper.Entities.Destiny.Definitions.Seasons;
using Serilog;
using System.Threading.Tasks;

namespace Levante.Helpers
{
    public class ManifestHelper
    {
        // Inv Hash, Collectible Hash
        public static Dictionary<long, uint> EmblemsCollectible = new();
        // Hash, Name
        public static Dictionary<long, string> Consumables = new();
        public static Dictionary<long, string> Emblems = new();
        public static Dictionary<long, string> Weapons = new();
        public static Dictionary<long, string> Seals = new();
        public static Dictionary<long, string> Ada1Items = new();
        public static Dictionary<long, string> Activities = new();
        // Seal Hash, Tracker Hash
        public static Dictionary<long, long> GildableSeals = new();

        public static Dictionary<long, DestinyInventoryItemDefinition> Fish = new();

        public static Dictionary<long, string> Perks = new();
        public static Dictionary<long, string> EnhancedPerks = new();

        public static Dictionary<long, string> ClarityDescriptions = new();

        // List per Week, <Hash, Record>.
        public static List<Dictionary<long, DestinyRecordDefinition>> SeasonalChallenges = new();
        public static DestinySeasonDefinition CurrentSeason = new();
        public static int CurrentLevelCap = 0;
        public static int BaseNextLevelAt = 0;
        public static int ExtraNextLevelAt = 0;
        public static Dictionary<long, string> SeasonalObjectives = new();
        public static Dictionary<string, int> StringVariables = new();

        public static Dictionary<long, string> GuardianRanks = new();

        private static Dictionary<string, int> SeasonIconURLs = new();

        private const string DIM_WATERMARK_TO_SEASON_LINK = "https://raw.githubusercontent.com/DestinyItemManager/d2-additional-info/master/output/watermark-to-season.json";
        private const string CLARITY_INFO_LINK = "https://raw.githubusercontent.com/Database-Clarity/Live-Clarity-Database/live/descriptions/crayon.json";

        public static string DestinyManifestVersion { get; internal set; } = "[VERSION]";

        public static bool IsNewManifest()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-API-Key", BotConfig.BungieApiKey);

                var response = client.GetAsync($"https://www.bungie.net/Platform/Destiny2/Manifest/").Result;
                var content = response.Content.ReadAsStringAsync().Result;
                dynamic item = JsonConvert.DeserializeObject(content);
                if (DestinyManifestVersion.Equals($"{item.Response.version}"))
                    return false;
                else
                    return true;
            }
        }

        public static void LoadManifestDictionaries()
        {
            if (!Directory.Exists("Data/Manifest/JSONs"))
                Directory.CreateDirectory("Data/Manifest/JSONs");

            var emoteCfg = JsonConvert.DeserializeObject<EmoteConfig>(File.ReadAllText(EmoteConfig.FilePath));

            Emblems.Clear();
            EmblemsCollectible.Clear();
            Weapons.Clear();
            Seals.Clear();
            Ada1Items.Clear();
            GildableSeals.Clear();
            Activities.Clear();
            SeasonIconURLs.Clear();
            Perks.Clear();
            EnhancedPerks.Clear();
            ClarityDescriptions.Clear();
            using (var client = new HttpClient())
            {
                // DIM Watermark to Season
                var dimAiResponse = client.GetAsync(DIM_WATERMARK_TO_SEASON_LINK).Result;
                SeasonIconURLs = JsonConvert.DeserializeObject<Dictionary<string, int>>(dimAiResponse.Content.ReadAsStringAsync().Result);

                // Clarity
                var clarityResponse = client.GetAsync(CLARITY_INFO_LINK).Result;
                Dictionary<long, Clarity> clarity = JsonConvert.DeserializeObject<Dictionary<long, Clarity>>(ClarityClean(clarityResponse.Content.ReadAsStringAsync().Result));

                client.DefaultRequestHeaders.Add("X-API-Key", BotConfig.BungieApiKey);

                var response = client.GetAsync($"https://www.bungie.net/Platform/Destiny2/Manifest/").Result;
                var content = response.Content.ReadAsStringAsync().Result;
                dynamic item = JsonConvert.DeserializeObject(content);

                // Update Manifest Version String
                DestinyManifestVersion = item.Response.version;
                if (!File.Exists($"Data/Manifest/JSONs/{DestinyManifestVersion}.json"))
                {
                    Log.Information("[{Type}] Found v.{ManifestVersion}. Downloading and storing locally...",
                        "Manifest", DestinyManifestVersion);
                    File.WriteAllText($"Data/Manifest/JSONs/{DestinyManifestVersion}.json", JsonConvert.SerializeObject(item, Formatting.Indented));
                }
                else
                {
                    Log.Information("[{Type}] Found v.{ManifestVersion}. No download needed.",
                       "Manifest", DestinyManifestVersion);
                }

                // Inventory Items
                string path = item.Response.jsonWorldComponentContentPaths.en["DestinyInventoryItemDefinition"];
                string fileName = path.Split('/').LastOrDefault();
                Dictionary<string, DestinyInventoryItemDefinition> invItemList = new();
                if (!Directory.Exists("Data/Manifest/JSONs/DestinyInventoryItemDefinition"))
                    Directory.CreateDirectory("Data/Manifest/JSONs/DestinyInventoryItemDefinition");
                if (!File.Exists($"Data/Manifest/JSONs/DestinyInventoryItemDefinition/{fileName}"))
                {
                    Log.Information("[{Type}] Storing {def} locally...",
                        "Manifest", "DestinyInventoryItemDefinition");
                    string invItemUrl = $"https://www.bungie.net{item.Response.jsonWorldComponentContentPaths.en["DestinyInventoryItemDefinition"]}";
                    response = client.GetAsync(invItemUrl).Result;
                    content = response.Content.ReadAsStringAsync().Result;
                    invItemList = JsonConvert.DeserializeObject<Dictionary<string, DestinyInventoryItemDefinition>>(content);
                    File.WriteAllText($"Data/Manifest/JSONs/DestinyInventoryItemDefinition/{fileName}", JsonConvert.SerializeObject(invItemList, Formatting.Indented));
                }
                else
                {
                    content = File.ReadAllText($"Data/Manifest/JSONs/DestinyInventoryItemDefinition/{fileName}");
                    invItemList = JsonConvert.DeserializeObject<Dictionary<string, DestinyInventoryItemDefinition>>(content);
                    Log.Information("[{Type}] Loaded {def} from local.",
                        "Manifest", "DestinyInventoryItemDefinition");
                }

                // Vendors
                path = item.Response.jsonWorldComponentContentPaths.en["DestinyVendorDefinition"];
                fileName = path.Split('/').LastOrDefault();
                Dictionary<string, DestinyVendorDefinition> vendorList = new();
                if (!Directory.Exists("Data/Manifest/JSONs/DestinyVendorDefinition"))
                    Directory.CreateDirectory("Data/Manifest/JSONs/DestinyVendorDefinition");
                if (!File.Exists($"Data/Manifest/JSONs/DestinyVendorDefinition/{fileName}"))
                {
                    Log.Information("[{Type}] Storing {def} locally...",
                        "Manifest", "DestinyVendorDefinition");
                    string vendorListUrl = $"https://www.bungie.net{item.Response.jsonWorldComponentContentPaths.en["DestinyVendorDefinition"]}";
                    response = client.GetAsync(vendorListUrl).Result;
                    content = response.Content.ReadAsStringAsync().Result;
                    vendorList = JsonConvert.DeserializeObject<Dictionary<string, DestinyVendorDefinition>>(content);
                    File.WriteAllText($"Data/Manifest/JSONs/DestinyVendorDefinition/{fileName}", JsonConvert.SerializeObject(vendorList, Formatting.Indented));
                }
                else
                {
                    content = File.ReadAllText($"Data/Manifest/JSONs/DestinyVendorDefinition/{fileName}");
                    vendorList = JsonConvert.DeserializeObject<Dictionary<string, DestinyVendorDefinition>>(content);
                    Log.Information("[{Type}] Loaded {def} from local.",
                        "Manifest", "DestinyVendorDefinition");
                }
                var ada1ItemList = vendorList["350061650"].ItemList.Select(x => x.ItemHash);

                // Activities
                path = item.Response.jsonWorldComponentContentPaths.en["DestinyActivityDefinition"];
                fileName = path.Split('/').LastOrDefault();
                Dictionary<string, DestinyActivityDefinition> activityList = new();
                if (!Directory.Exists("Data/Manifest/JSONs/DestinyActivityDefinition"))
                    Directory.CreateDirectory("Data/Manifest/JSONs/DestinyActivityDefinition");
                if (!File.Exists($"Data/Manifest/JSONs/DestinyActivityDefinition/{fileName}"))
                {
                    Log.Information("[{Type}] Storing {def} locally...",
                        "Manifest", "DestinyActivityDefinition");
                    string activityListUrl = $"https://www.bungie.net{item.Response.jsonWorldComponentContentPaths.en["DestinyActivityDefinition"]}";
                    response = client.GetAsync(activityListUrl).Result;
                    content = response.Content.ReadAsStringAsync().Result;
                    activityList = JsonConvert.DeserializeObject<Dictionary<string, DestinyActivityDefinition>>(content);
                    File.WriteAllText($"Data/Manifest/JSONs/DestinyActivityDefinition/{fileName}", JsonConvert.SerializeObject(activityList, Formatting.Indented));
                }
                else
                {
                    content = File.ReadAllText($"Data/Manifest/JSONs/DestinyActivityDefinition/{fileName}");
                    activityList = JsonConvert.DeserializeObject<Dictionary<string, DestinyActivityDefinition>>(content);
                    Log.Information("[{Type}] Loaded {def} from local.",
                        "Manifest", "DestinyActivityDefinition");
                }

                // Places
                path = item.Response.jsonWorldComponentContentPaths.en["DestinyPlaceDefinition"];
                fileName = path.Split('/').LastOrDefault();
                Dictionary<string, DestinyPlaceDefinition> placeList = new();
                if (!Directory.Exists("Data/Manifest/JSONs/DestinyPlaceDefinition"))
                    Directory.CreateDirectory("Data/Manifest/JSONs/DestinyPlaceDefinition");
                if (!File.Exists($"Data/Manifest/JSONs/DestinyPlaceDefinition/{fileName}"))
                {
                    Log.Information("[{Type}] Storing {def} locally...",
                        "Manifest", "DestinyPlaceDefinition");
                    string placeListUrl = $"https://www.bungie.net{item.Response.jsonWorldComponentContentPaths.en["DestinyPlaceDefinition"]}";
                    response = client.GetAsync(placeListUrl).Result;
                    content = response.Content.ReadAsStringAsync().Result;
                    placeList = JsonConvert.DeserializeObject<Dictionary<string, DestinyPlaceDefinition>>(content);
                    File.WriteAllText($"Data/Manifest/JSONs/DestinyPlaceDefinition/{fileName}", JsonConvert.SerializeObject(placeList, Formatting.Indented));
                }
                else
                {
                    content = File.ReadAllText($"Data/Manifest/JSONs/DestinyPlaceDefinition/{fileName}");
                    placeList = JsonConvert.DeserializeObject<Dictionary<string, DestinyPlaceDefinition>>(content);
                    Log.Information("[{Type}] Loaded {def} from local.",
                        "Manifest", "DestinyPlaceDefinition");
                }

                // Modifiers
                path = item.Response.jsonWorldComponentContentPaths.en["DestinyActivityModifierDefinition"];
                fileName = path.Split('/').LastOrDefault();
                Dictionary<string, DestinyActivityModifierDefinition> modifierList = new();
                if (!Directory.Exists("Data/Manifest/JSONs/DestinyActivityModifierDefinition"))
                    Directory.CreateDirectory("Data/Manifest/JSONs/DestinyActivityModifierDefinition");
                if (!File.Exists($"Data/Manifest/JSONs/DestinyActivityModifierDefinition/{fileName}"))
                {
                    Log.Information("[{Type}] Storing {def} locally...",
                        "Manifest", "DestinyActivityModifierDefinition");
                    string modifierListUrl = $"https://www.bungie.net{item.Response.jsonWorldComponentContentPaths.en["DestinyActivityModifierDefinition"]}";
                    response = client.GetAsync(modifierListUrl).Result;
                    content = response.Content.ReadAsStringAsync().Result;
                    modifierList = JsonConvert.DeserializeObject<Dictionary<string, DestinyActivityModifierDefinition>>(content);
                    File.WriteAllText($"Data/Manifest/JSONs/DestinyActivityModifierDefinition/{fileName}", JsonConvert.SerializeObject(modifierList, Formatting.Indented));
                }
                else
                {
                    content = File.ReadAllText($"Data/Manifest/JSONs/DestinyActivityModifierDefinition/{fileName}");
                    modifierList = JsonConvert.DeserializeObject<Dictionary<string, DestinyActivityModifierDefinition>>(content);
                    Log.Information("[{Type}] Loaded {def} from local.",
                        "Manifest", "DestinyActivityModifierDefinition");
                }

                // Records/Triumph
                path = item.Response.jsonWorldComponentContentPaths.en["DestinyRecordDefinition"];
                fileName = path.Split('/').LastOrDefault();
                Dictionary<string, DestinyRecordDefinition> recordList = new();
                if (!Directory.Exists("Data/Manifest/JSONs/DestinyRecordDefinition"))
                    Directory.CreateDirectory("Data/Manifest/JSONs/DestinyRecordDefinition");
                if (!File.Exists($"Data/Manifest/JSONs/DestinyRecordDefinition/{fileName}"))
                {
                    Log.Information("[{Type}] Storing {def} locally...",
                        "Manifest", "DestinyRecordDefinition");
                    string recordUrl = $"https://www.bungie.net{item.Response.jsonWorldComponentContentPaths.en["DestinyRecordDefinition"]}";
                    response = client.GetAsync(recordUrl).Result;
                    content = response.Content.ReadAsStringAsync().Result;
                    recordList = JsonConvert.DeserializeObject<Dictionary<string, DestinyRecordDefinition>>(content);
                    File.WriteAllText($"Data/Manifest/JSONs/DestinyRecordDefinition/{fileName}", JsonConvert.SerializeObject(recordList, Formatting.Indented));
                }
                else
                {
                    content = File.ReadAllText($"Data/Manifest/JSONs/DestinyRecordDefinition/{fileName}");
                    recordList = JsonConvert.DeserializeObject<Dictionary<string, DestinyRecordDefinition>>(content);
                    Log.Information("[{Type}] Loaded {def} from local.",
                        "Manifest", "DestinyRecordDefinition");
                }

                // Objectives
                path = item.Response.jsonWorldComponentContentPaths.en["DestinyObjectiveDefinition"];
                fileName = path.Split('/').LastOrDefault();
                Dictionary<string, DestinyObjectiveDefinition> objectiveList = new();
                if (!Directory.Exists("Data/Manifest/JSONs/DestinyObjectiveDefinition"))
                    Directory.CreateDirectory("Data/Manifest/JSONs/DestinyObjectiveDefinition");
                if (!File.Exists($"Data/Manifest/JSONs/DestinyObjectiveDefinition/{fileName}"))
                {
                    Log.Information("[{Type}] Storing {def} locally...",
                        "Manifest", "DestinyObjectiveDefinition");
                    string recordUrl = $"https://www.bungie.net{item.Response.jsonWorldComponentContentPaths.en["DestinyObjectiveDefinition"]}";
                    response = client.GetAsync(recordUrl).Result;
                    content = response.Content.ReadAsStringAsync().Result;
                    objectiveList = JsonConvert.DeserializeObject<Dictionary<string, DestinyObjectiveDefinition>>(content);
                    File.WriteAllText($"Data/Manifest/JSONs/DestinyObjectiveDefinition/{fileName}", JsonConvert.SerializeObject(objectiveList, Formatting.Indented));
                }
                else
                {
                    content = File.ReadAllText($"Data/Manifest/JSONs/DestinyObjectiveDefinition/{fileName}");
                    objectiveList = JsonConvert.DeserializeObject<Dictionary<string, DestinyObjectiveDefinition>>(content);
                    Log.Information("[{Type}] Loaded {def} from local.",
                        "Manifest", "DestinyObjectiveDefinition");
                }

                // Presentation Nodes
                path = item.Response.jsonWorldComponentContentPaths.en["DestinyPresentationNodeDefinition"];
                fileName = path.Split('/').LastOrDefault();
                Dictionary<string, DestinyPresentationNodeDefinition> presentNodeList = new();
                if (!Directory.Exists("Data/Manifest/JSONs/DestinyPresentationNodeDefinition"))
                    Directory.CreateDirectory("Data/Manifest/JSONs/DestinyPresentationNodeDefinition");
                if (!File.Exists($"Data/Manifest/JSONs/DestinyPresentationNodeDefinition/{fileName}"))
                {
                    Log.Information("[{Type}] Storing {def} locally...",
                        "Manifest", "DestinyPresentationNodeDefinition");
                    string presentNodeUrl = $"https://www.bungie.net{item.Response.jsonWorldComponentContentPaths.en["DestinyPresentationNodeDefinition"]}";
                    response = client.GetAsync(presentNodeUrl).Result;
                    content = response.Content.ReadAsStringAsync().Result;
                    presentNodeList = JsonConvert.DeserializeObject<Dictionary<string, DestinyPresentationNodeDefinition>>(content);
                    File.WriteAllText($"Data/Manifest/JSONs/DestinyPresentationNodeDefinition/{fileName}", JsonConvert.SerializeObject(presentNodeList, Formatting.Indented));
                }
                else
                {
                    content = File.ReadAllText($"Data/Manifest/JSONs/DestinyPresentationNodeDefinition/{fileName}");
                    presentNodeList = JsonConvert.DeserializeObject<Dictionary<string, DestinyPresentationNodeDefinition>>(content);
                    Log.Information("[{Type}] Loaded {def} from local.",
                        "Manifest", "DestinyPresentationNodeDefinition");
                }

                // Sandbox Perks
                path = item.Response.jsonWorldComponentContentPaths.en["DestinySandboxPerkDefinition"];
                fileName = path.Split('/').LastOrDefault();
                Dictionary<string, DestinySandboxPerkDefinition> sandboxPerkList = new();
                if (!Directory.Exists("Data/Manifest/JSONs/DestinySandboxPerkDefinition"))
                    Directory.CreateDirectory("Data/Manifest/JSONs/DestinySandboxPerkDefinition");
                if (!File.Exists($"Data/Manifest/JSONs/DestinySandboxPerkDefinition/{fileName}"))
                {
                    Log.Information("[{Type}] Storing {def} locally...",
                        "Manifest", "DestinySandboxPerkDefinition");
                    string sandboxPerkUrl = $"https://www.bungie.net{item.Response.jsonWorldComponentContentPaths.en["DestinySandboxPerkDefinition"]}";
                    response = client.GetAsync(sandboxPerkUrl).Result;
                    content = response.Content.ReadAsStringAsync().Result;
                    sandboxPerkList = JsonConvert.DeserializeObject<Dictionary<string, DestinySandboxPerkDefinition>>(content);
                    File.WriteAllText($"Data/Manifest/JSONs/DestinySandboxPerkDefinition/{fileName}", JsonConvert.SerializeObject(sandboxPerkList, Formatting.Indented));
                }
                else
                {
                    content = File.ReadAllText($"Data/Manifest/JSONs/DestinySandboxPerkDefinition/{fileName}");
                    sandboxPerkList = JsonConvert.DeserializeObject<Dictionary<string, DestinySandboxPerkDefinition>>(content);
                    Log.Information("[{Type}] Loaded {def} from local.",
                        "Manifest", "DestinySandboxPerkDefinition");
                }

                // Seasons
                path = item.Response.jsonWorldComponentContentPaths.en["DestinySeasonDefinition"];
                fileName = path.Split('/').LastOrDefault();
                Dictionary<long, DestinySeasonDefinition> seasonList = new();
                if (!Directory.Exists("Data/Manifest/JSONs/DestinySeasonDefinition"))
                    Directory.CreateDirectory("Data/Manifest/JSONs/DestinySeasonDefinition");
                if (!File.Exists($"Data/Manifest/JSONs/DestinySeasonDefinition/{fileName}"))
                {
                    Log.Information("[{Type}] Storing {def} locally...",
                        "Manifest", "DestinySeasonDefinition");
                    string seasonUrl = $"https://www.bungie.net{item.Response.jsonWorldComponentContentPaths.en["DestinySeasonDefinition"]}";
                    response = client.GetAsync(seasonUrl).Result;
                    content = response.Content.ReadAsStringAsync().Result;
                    seasonList = JsonConvert.DeserializeObject<Dictionary<long, DestinySeasonDefinition>>(content);

                    File.WriteAllText($"Data/Manifest/JSONs/DestinySeasonDefinition/{fileName}", JsonConvert.SerializeObject(JsonConvert.DeserializeObject(content), Formatting.Indented));
                }
                else
                {
                    content = File.ReadAllText($"Data/Manifest/JSONs/DestinySeasonDefinition/{fileName}");
                    seasonList = JsonConvert.DeserializeObject<Dictionary<long, DestinySeasonDefinition>>(content);

                    Log.Information("[{Type}] Loaded {def} from local.",
                        "Manifest", "DestinySeasonDefinition");
                }

                foreach (var season in seasonList)
                {
                    if (season.Value.StartDate <= DateTime.UtcNow && DateTime.UtcNow <= season.Value.EndDate)
                        CurrentSeason = season.Value;
                }

                dynamic raw = JsonConvert.DeserializeObject(content);
                dynamic rawSeason = raw[$"{CurrentSeason.Hash}"];
                if (rawSeason.acts != null)
                {
                    for (int i = 0; i < rawSeason.acts.Count; i++)
                    {
                        if (((DateTime)rawSeason.acts[i].startTime) < DateTime.Now)
                        {
                            CurrentLevelCap += (int)rawSeason.acts[i].rankCount;
                        }
                    }
                }

                string stringVarUrl = "https://bungie.net/Platform/Destiny2/3/Profile/4611686018471482002/?components=100,202,1200";
                response = client.GetAsync(stringVarUrl).Result;
                content = response.Content.ReadAsStringAsync().Result;
                item = JsonConvert.DeserializeObject(content);
                var baseProgression = item.Response.characterProgressions.data[$"{item.Response.profile.data.characterIds[0]}"].progressions[$"{BotConfig.Hashes.First100Ranks}"];
                var extraProgression = item.Response.characterProgressions.data[$"{item.Response.profile.data.characterIds[0]}"].progressions[$"{BotConfig.Hashes.Above100Ranks}"];
                StringVariables = item.Response.profileStringVariables.data.integerValuesByHash.ToObject<Dictionary<string, int>>();
                BaseNextLevelAt = baseProgression.nextLevelAt;
                ExtraNextLevelAt = extraProgression.nextLevelAt;

                Log.Information("[{Type}] Populating Dictionaries...",
                        "Manifest");
                try
                {
                    foreach (var record in recordList)
                    {
                        if (record.Value.TitleInfo == null) continue;
                        if (record.Value.TitleInfo.HasTitle)
                        {
                            Seals.Add(record.Value.Hash, $"{record.Value.TitleInfo.TitlesByGender.Values.FirstOrDefault()}");
                            if (record.Value.TitleInfo.GildingTrackingRecordHash != null)
                                GildableSeals.Add(record.Value.Hash, (long)record.Value.TitleInfo.GildingTrackingRecordHash);
                        }
                    }

                    foreach (var season in seasonList)
                    {
                        if (season.Value.StartDate <= DateTime.UtcNow && DateTime.UtcNow <= season.Value.EndDate)
                            CurrentSeason = season.Value;
                    }

                    foreach (var node in presentNodeList)
                    {
                        if (node.Value.DisplayProperties == null) continue;
                        if (node.Value.Children == null) continue;

                        if (node.Value.DisplayProperties.Name == "Weekly" && node.Value.Children.PresentationNodes.Any())
                        {
                            foreach (var child in node.Value.Children.PresentationNodes)
                            {
                                var childNode = presentNodeList[$"{child.PresentationNodeHash}"];
                                if (childNode.DisplayProperties.Name.Contains("Week"))
                                {
                                    var challenges = new Dictionary<long, DestinyRecordDefinition>();
                                    foreach (var record in childNode.Children.Records)
                                    {
                                        challenges.Add(record.RecordHash, recordList[$"{record.RecordHash}"]);
                                        if (recordList[$"{record.RecordHash}"].ObjectiveHashes != null)
                                            foreach (var obj in recordList[$"{record.RecordHash}"].ObjectiveHashes)
                                            {
                                                if (!String.IsNullOrEmpty(objectiveList[$"{obj}"].ProgressDescription))
                                                    SeasonalObjectives.Add(obj, DestinyEmote.ParseBungieText(objectiveList[$"{obj}"].ProgressDescription));
                                            }
                                        //Log.Debug("Added {Record} from {Week}.", recordList[$"{record.RecordHash}"].DisplayProperties.Name, childNode.DisplayProperties.Name);
                                    }
                                    SeasonalChallenges.Add(challenges);
                                }
                            }
                        }

                        if (node.Value.DisplayProperties.Name.Equals("Guardian Ranks"))
                            foreach (var rank in node.Value.Children.PresentationNodes)
                                GuardianRanks.Add(rank.PresentationNodeHash, presentNodeList[$"{rank.PresentationNodeHash}"].DisplayProperties.Name);

                        //if (!node.Value.Children.Records.Any()) continue;
                        //if (node.Value.CompletionRecordHash == null) continue;
                        //foreach (var child in node.Value.Children.Records)
                        //{
                        //    if (GildableSeals.ContainsKey((long)node.Value.CompletionRecordHash) && recordList.ContainsKey(child.RecordHash.ToString()))
                        //    {
                        //        string recordName = recordList[child.RecordHash.ToString()].DisplayProperties.Name;
                        //        if (recordName.Contains("Grandmaster:"))
                        //        {
                        //            CurrentRotations.Nightfall.Rotations.Add(new Nightfall { Name = recordName.Replace("Grandmaster: ", "") });
                        //        }
                        //    }
                        //}
                    }

                    foreach (var activity in activityList)
                    {
                        if (String.IsNullOrEmpty(activity.Value.DisplayProperties.Name))
                            if (placeList.ContainsKey($"{activity.Value.PlaceHash}"))
                            {
                                Activities.Add(activity.Value.Hash, placeList[$"{activity.Value.PlaceHash}"].DisplayProperties.Name);
                                continue;
                            }

                        Activities.Add(activity.Value.Hash, activity.Value.DisplayProperties.Name);
                        int index = CurrentRotations.LostSector.Rotations.FindIndex(x => activity.Key.Equals($"{x.LegendActivityHash}"));
                        if (index != -1)
                        {
                            CurrentRotations.LostSector.Rotations[index].Name = activity.Value.OriginalDisplayProperties.Name;
                            CurrentRotations.LostSector.Rotations[index].Location = placeList[$"{activity.Value.PlaceHash}"].DisplayProperties.Name;
                            foreach (var mod in activity.Value.Modifiers)
                            {
                                if (!modifierList.ContainsKey($"{mod.ActivityModifierHash}"))
                                    continue;

                                var modifer = modifierList[$"{mod.ActivityModifierHash}"];
                                if (String.IsNullOrEmpty(modifer.DisplayProperties.Name) ||
                                    modifer.DisplayProperties.Name.Contains("Champion") ||
                                    modifer.DisplayProperties.Name.Contains("Shielded") ||
                                    modifer.DisplayProperties.Name.Contains("Modifiers") ||
                                    modifer.DisplayProperties.Name.Contains("Threat") ||
                                    modifer.DisplayProperties.Name.Contains("Surge"))
                                {
                                    continue;
                                }
                                CurrentRotations.LostSector.Rotations[index].LegendModifiers.Add(modifer.DisplayProperties.Name);

                                if (String.IsNullOrEmpty(modifer.DisplayProperties.Icon))
                                    continue;

                                //if (!emoteCfg.Emotes.ContainsKey(modifer.DisplayProperties.Name.Replace(" ", "").Replace("-", "").Replace("'", "")))
                                //{
                                //    var byteArray = new HttpClient().GetByteArrayAsync($"https://bungie.net{modifer.DisplayProperties.Icon}").Result;
                                //    Task.Run(() => emoteCfg.AddEmote(modifer.DisplayProperties.Name.Replace(" ", "").Replace("-", "").Replace("'", ""), new Discord.Image(new MemoryStream(byteArray)))).Wait();
                                //    Log.Debug($"Made new Emote for {modifer.DisplayProperties.Name} with icon: {modifer.DisplayProperties.Icon}");
                                //}
                            }
                        }
                        else
                        {
                            index = CurrentRotations.LostSector.Rotations.FindIndex(x => activity.Key.Equals($"{x.MasterActivityHash}"));
                            if (index != -1)
                            {
                                CurrentRotations.LostSector.Rotations[index].Name = activity.Value.OriginalDisplayProperties.Name;
                                CurrentRotations.LostSector.Rotations[index].PGCRImage = "https://bungie.net" + activity.Value.PgcrImage;
                                CurrentRotations.LostSector.Rotations[index].Location = placeList[$"{activity.Value.PlaceHash}"].DisplayProperties.Name;
                                foreach (var mod in activity.Value.Modifiers)
                                {
                                    if (!modifierList.ContainsKey($"{mod.ActivityModifierHash}"))
                                        continue;

                                    var modifer = modifierList[$"{mod.ActivityModifierHash}"];
                                    // Don't want champion modifier(s) because we have those.
                                    if (String.IsNullOrEmpty(modifer.DisplayProperties.Name) ||
                                        modifer.DisplayProperties.Name.Contains("Champion") ||
                                        modifer.DisplayProperties.Name.Contains("Shielded") ||
                                        modifer.DisplayProperties.Name.Contains("Modifiers") ||
                                        modifer.DisplayProperties.Name.Contains("Threat") ||
                                        modifer.DisplayProperties.Name.Contains("Surge"))
                                    {
                                        continue;
                                    }
                                    CurrentRotations.LostSector.Rotations[index].MasterModifiers.Add(modifer.DisplayProperties.Name);

                                    if (String.IsNullOrEmpty(modifer.DisplayProperties.Icon))
                                        continue;

                                    //if (!emoteCfg.Emotes.ContainsKey(modifer.DisplayProperties.Name.Replace(" ", "").Replace("-", "").Replace("'", "")))
                                    //{
                                    //    var byteArray = new HttpClient().GetByteArrayAsync($"https://bungie.net{modifer.DisplayProperties.Icon}").Result;
                                    //    Task.Run(() => emoteCfg.AddEmote(modifer.DisplayProperties.Name.Replace(" ", "").Replace("-", "").Replace("'", ""), new Discord.Image(new MemoryStream(byteArray)))).Wait();
                                    //    Log.Debug($"Made new Emote for {modifer.DisplayProperties.Name} with icon: {modifer.DisplayProperties.Icon}");
                                    //}
                                }
                            }
                        }
                    }

                    foreach (var invItem in invItemList)
                    {
                        if (invItem.Value == null) continue;

                        if (invItem.Value.Inventory != null
                            && invItem.Value.Inventory.StackUniqueLabel != null
                            && invItem.Value.Inventory.StackUniqueLabel.Contains("fishing.proxies.fish"))
                        {
                            Fish.Add(long.Parse(invItem.Key), invItem.Value);
                        }

                        if (string.IsNullOrWhiteSpace(invItem.Value.DisplayProperties.Name) ||
                            string.IsNullOrWhiteSpace(invItem.Value.ItemTypeDisplayName)) continue;


                        //Console.WriteLine($"{invItem.Value.DisplayProperties.Name}");
                        if (invItem.Value.ItemType == DestinyItemType.Emblem && invItem.Value.BackgroundColor != null)
                        {
                            if (invItem.Value.Hash == 1968995963) // соняшник (Sunflower) Ukraine Relief Emblem
                                Emblems.Add(invItem.Value.Hash, $"{invItem.Value.DisplayProperties.Name} (Sunflower)");
                            else
                                Emblems.Add(invItem.Value.Hash, invItem.Value.DisplayProperties.Name);

                            if (invItem.Value.CollectibleHash != null)
                                EmblemsCollectible.Add(invItem.Value.Hash, (uint)invItem.Value.CollectibleHash);
                        }

                        if (invItem.Value.ItemType == DestinyItemType.Weapon)
                        {
                            if (invItem.Value.DisplayProperties.Name == null)
                                continue;

                            if (invItem.Value.Hash == 417164956) // Jötunn
                                Weapons.Add(invItem.Value.Hash, $"{invItem.Value.DisplayProperties.Name} (Jotunn)");
                            else
                            {
                                var dupeWeapons = Weapons.Where(x => x.Value.Split(" [")[0].Equals(invItem.Value.DisplayProperties.Name));
                                if (dupeWeapons.Any())
                                {
                                    int dupeCount = dupeWeapons.Count();
                                    foreach (var weapon in dupeWeapons.ToList())
                                    {
                                        if (!weapon.Value.Contains("[S") && !weapon.Value.Contains("[V"))
                                        {
                                            Weapons.Remove(weapon.Key);
                                            if (invItemList[$"{weapon.Key}"].IconWatermark == null)
                                                Weapons.Add(weapon.Key, $"{weapon.Value} [S01]");
                                            else if (SeasonIconURLs.ContainsKey(invItemList[$"{weapon.Key}"].IconWatermark))
                                                Weapons.Add(weapon.Key, $"{weapon.Value} [S{SeasonIconURLs[$"{invItemList[$"{weapon.Key}"].IconWatermark}"]:00}]");
                                            else // This handles event weapons because their icon does not have the season one. Lazy implementation that does not take into account whether or not the weapon is the more recent version.
                                            {
                                                //Log.Debug("Added: {Name} [V{Count}]", weapon.Value, dupeCount + 1);
                                                Weapons.Add(weapon.Key, $"{weapon.Value} [V{(dupeCount + 1):00}]");
                                            }
                                        }
                                    }
                                    if (invItem.Value.IconWatermark == null)
                                        Weapons.Add(invItem.Value.Hash, $"{invItem.Value.DisplayProperties.Name} [S01]");
                                    else if (SeasonIconURLs.ContainsKey(invItem.Value.IconWatermark))
                                        Weapons.Add(invItem.Value.Hash, $"{invItem.Value.DisplayProperties.Name} [S{SeasonIconURLs[$"{invItem.Value.IconWatermark}"]:00}]");
                                    else
                                    {
                                        //Log.Debug("Added: {Name} [V{Count}]", invItem.Value.DisplayProperties.Name, 1);
                                        Weapons.Add(invItem.Value.Hash, $"{invItem.Value.DisplayProperties.Name} [V01]");
                                    }
                                }
                                else
                                    Weapons.Add(invItem.Value.Hash, $"{invItem.Value.DisplayProperties.Name}");
                            }

                            int index = CurrentRotations.Nightfall.WeaponRotations.FindIndex(x => x.Hash == invItem.Value.Hash);
                            if (index != -1)
                            {
                                CurrentRotations.Nightfall.WeaponRotations[index].Name = invItem.Value.DisplayProperties.Name;
                                bool isHeavyGL = invItem.Value.ItemSubType == DestinyItemSubType.GrenadeLauncher && invItem.Value.ItemCategoryHashes.Contains<uint>(4);
                                CurrentRotations.Nightfall.WeaponRotations[index].Emote = DestinyEmote.MatchWeaponItemSubtypeToEmote(invItem.Value.ItemSubType, isHeavyGL);
                            }
                        }

                        if (invItem.Value.ItemType == DestinyItemType.Mod)
                        {
                            //if (invItem.Value.DisplayProperties.Name == null || 
                            //    invItem.Value.DisplayProperties.Name.Equals("Empty Mod Socket") || 
                            //    invItem.Value.DisplayProperties.Name.Equals("Deprecated Armor Mod"))
                            //    continue;

                            //if (!invItem.Value.ItemCategoryHashes.Contains(4104513227))
                            //    continue;

                            if (ada1ItemList.Contains(invItem.Value.Hash) && invItem.Value.ItemSubType == DestinyItemSubType.Shader)
                                Ada1Items.Add(invItem.Value.Hash, $"{invItem.Value.DisplayProperties.Name}");


                            if (invItem.Value.ItemSubType == 0 && invItem.Value.ItemTypeDisplayName.Contains("Trait") && !invItem.Value.ItemCategoryHashes.Contains(4104513227) /*Exclude Armor Mods*/)
                            {
                                //Log.Debug("Perk: {PerkName} {IsEnhanced}", invItem.Value.DisplayProperties.Name, invItem.Value.ItemTypeDisplayName.Contains("Enhanced"));
                                if (invItem.Value.ItemTypeDisplayName.Contains("Enhanced"))
                                {
                                    if (EnhancedPerks.ContainsValue(invItem.Value.DisplayProperties.Name)) continue;
                                    EnhancedPerks.Add(invItem.Value.Hash, invItem.Value.DisplayProperties.Name.Replace('ä', 'a').Replace("Enhanced", "") /*Looking at you Perpetual Motion and Golden Tricorn*/);

                                    if (clarity.ContainsKey(invItem.Value.Hash) && clarity[invItem.Value.Hash].Descriptions != null && clarity[invItem.Value.Hash].Descriptions.ContainsKey("en"))
                                        ClarityDescriptions.Add(invItem.Value.Hash, clarity[invItem.Value.Hash].Descriptions["en"]);
                                }
                                else
                                {
                                    if (Perks.ContainsValue(invItem.Value.DisplayProperties.Name)) continue;
                                    Perks.Add(invItem.Value.Hash, invItem.Value.DisplayProperties.Name.Replace('ä', 'a').Replace("Enhanced", "") /*Looking at you Perpetual Motion and Golden Tricorn*/);

                                    if (clarity.ContainsKey(invItem.Value.Hash) && clarity[invItem.Value.Hash].Descriptions != null && clarity[invItem.Value.Hash].Descriptions.ContainsKey("en"))
                                        ClarityDescriptions.Add(invItem.Value.Hash, clarity[invItem.Value.Hash].Descriptions["en"]);
                                }

                            }
                        }

                        // invItem.Value.Inventory.BucketTypeHash == 1469714392 /* Consumables */ && invItem.Value.TraitHashes != null && invItem.Value.TraitHashes.Contains(3906525419)
                        if (invItem.Value.ItemType == DestinyItemType.Consumable)
                        {
                            //Log.Debug($"{invItem.Value.DisplayProperties.Name}");
                            Consumables.Add(invItem.Value.Hash, invItem.Value.DisplayProperties.Name);
                        }

                        if (invItem.Value.ItemType == DestinyItemType.None)
                        {
                            // Find those Enhanced Perks that aren't labeled as mods!
                            if (invItem.Value.ItemTypeDisplayName.Contains("Enhanced Trait"))
                            {
                                //Log.Debug("Perk: {PerkName} {IsEnhanced}", invItem.Value.DisplayProperties.Name, invItem.Value.ItemTypeDisplayName.Contains("Enhanced"));
                                EnhancedPerks.Add(invItem.Value.Hash, invItem.Value.DisplayProperties.Name.Replace("Enhanced", "") /*Looking at you Perpetual Motion and Golden Tricorn*/);

                                if (clarity.ContainsKey(invItem.Value.Hash) && clarity[invItem.Value.Hash].Descriptions != null)
                                    ClarityDescriptions.Add(invItem.Value.Hash, clarity[invItem.Value.Hash].Descriptions["en"]);
                            }

                            if (invItem.Value.ItemTypeDisplayName.Contains("Currency") && invItem.Value.Inventory.BucketTypeHash == 1469714392)
                            {
                                //Log.Debug($"{invItem.Value.DisplayProperties.Name}");
                                Consumables.Add(invItem.Value.Hash, invItem.Value.DisplayProperties.Name);
                            }

                            if (invItem.Value.ItemTypeDisplayName.Contains("Material"))
                            {
                                //Log.Debug($"{invItem.Value.DisplayProperties.Name}");
                                Consumables.Add(invItem.Value.Hash, invItem.Value.DisplayProperties.Name);
                            }

                            if (invItem.Value.ItemTypeDisplayName.Contains("Memento") && invItem.Value.Inventory.BucketTypeHash == 1469714392)
                            {
                                //Log.Debug($"{invItem.Value.DisplayProperties.Name}");
                                Consumables.Add(invItem.Value.Hash, invItem.Value.DisplayProperties.Name);
                            }

                            // LETS GO!!! SHADERS AREN'T SHADERS ANY MORE! (This is a joke because this sucks)
                            //if (ada1ItemList.Contains(invItem.Value.Hash) && invItem.Value.ItemTypeDisplayName.Contains("Shader"))
                            //{
                            //    Log.Debug($"{invItem.Value.DisplayProperties.Name}");
                            //    Ada1Items.Add(invItem.Value.Hash, $"{invItem.Value.DisplayProperties.Name}");
                            //}
                        }
                    }
                }
                catch (Exception x)
                {
                    Log.Fatal("[{Type}] Population error. {exception}", "Manifest", x);
                }
            }

            Log.Information("[{Type}] Dictionary population complete.", "Manifest");
        }

        private static string ClarityClean(string input)
        {
            string output = input.Replace("🡅", DestinyEmote.Enhanced);

            return output;
        }
    }
}
