﻿using Discord;
using Levante.Configs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BungieSharper.Entities.Forum;
using Newtonsoft.Json.Linq;
using Serilog;
using Discord.WebSocket;

namespace Levante.Services
{
    public class CreationsService
    {
        private readonly Timer _timer;
        private readonly DiscordShardedClient _client;

        public readonly string FilePath = @"Configs/creationsConfig.json";

        private List<string> CreationsPosts = new();

        public CreationsService(DiscordShardedClient client)
        {
            if (!File.Exists(FilePath))
            {
                File.WriteAllText(FilePath, JsonConvert.SerializeObject(CreationsPosts, Formatting.Indented));
                Log.Information("[{Type}] No creationsConfig.json file detected. A new one has been created, no action needed.", "Creations");
            }

            string json = File.ReadAllText(FilePath);
            CreationsPosts = JsonConvert.DeserializeObject<List<string>>(json);

            _timer = new Timer(CheckCommunityCreationsCallback, null, 5000, 60000 * 2);
            _client = client;

            Log.Information("[{Type}] Creations Service Loaded.", "Creations");
        }

        public async void CheckCommunityCreationsCallback(object o) => await CheckCreations().ConfigureAwait(false);

        private async Task CheckCreations()
        {
            Log.Information("[{Type}] Pulling recent Creations...", "Creations");
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-API-Key", AppConfig.Credentials.BungieApiKey);

                    var response = client.GetAsync($"https://www.bungie.net/Platform/CommunityContent/Get/1/0/0/").Result;
                    var content = response.Content.ReadAsStringAsync().Result;
                    dynamic item = JsonConvert.DeserializeObject(content);

                    bool isNewCreations = false;
                    List<string> newCreations = new();
                    var results = ((JArray)item.Response.results).Reverse();
                    foreach (var creation in (dynamic)results)
                    {
                        // Find Author
                        newCreations.Add($"{creation.postId}");
                        string authorName = "";
                        string authorAvatarUrl = "https://bungie.net";
                        foreach (var author in item.Response.authors)
                        {
                            if ($"{author.membershipId}".Equals($"{creation.authorMembershipId}"))
                            {
                                authorAvatarUrl += $"{author.profilePicturePath}";
                                authorName = $"{author.displayName}";
                                break;
                            }
                        }
                        // Find Editor
                        string editorName = "";
                        foreach (var author in item.Response.authors)
                        {
                            if ($"{author.membershipId}".Equals($"{creation.editorMembershipId}"))
                            {
                                editorName = $"{author.displayName}";
                                break;
                            }
                        }
                        // Build Embed
                        if (!CreationsPosts.Contains($"{creation.postId}"))
                        {
                            var auth = new EmbedAuthorBuilder()
                            {
                                Name = $"A new Community Creation has been approved!",
                                IconUrl = authorAvatarUrl,
                                Url = $"https://www.bungie.net/en/Community/Detail?itemId={creation.postId}"
                            };
                            var foot = new EmbedFooterBuilder()
                            {
                                Text = $"Powered by the Bungie API"
                            };
                            var embed = new EmbedBuilder()
                            {
                                Color = new Color(AppConfig.Discord.EmbedColor.R, AppConfig.Discord.EmbedColor.G, AppConfig.Discord.EmbedColor.B),
                                Author = auth,
                                Footer = foot,
                            };
                            embed.Description = $"[{System.Web.HttpUtility.HtmlDecode($"{creation.subject}")}](https://www.bungie.net/en/Community/Detail?itemId={creation.postId})";
                            if (creation.urlMediaType == ForumMediaType.Image)
                                embed.ImageUrl = $"{creation.urlLinkOrImage}";
                            else if (creation.urlMediaType == ForumMediaType.Video || creation.urlMediaType == ForumMediaType.Youtube)
                                embed.Description += $"\n> Video Link: {creation.urlLinkOrImage}";

                            embed.ThumbnailUrl = authorAvatarUrl;
                            embed.AddField(x =>
                            {
                                x.Name = "Submitted";
                                x.Value = $"{TimestampTag.FromDateTime(DateTime.SpecifyKind(DateTime.Parse($"{creation.creationDate}"), DateTimeKind.Utc))}\n" +
                                    $"by {authorName}";
                                x.IsInline = true;
                            }).AddField(x =>
                            {
                                x.Name = "Approved";
                                x.Value = $"{TimestampTag.FromDateTime(DateTime.SpecifyKind(DateTime.Parse($"{creation.lastModified}"), DateTimeKind.Utc))}\n" +
                                    $"by {editorName}";
                                x.IsInline = true;
                            });

                            var msg = await AppConfig.Discord.CreationsLogChannel.SendMessageAsync(embed: embed.Build());
                            //if (BotConfig.CreationsLogChannel is SocketNewsChannel)
                            //await msg.CrosspostAsync();

                            isNewCreations = true;
                        }
                    }

                    if (isNewCreations)
                    {
                        CreationsPosts = newCreations;
                        File.WriteAllText(FilePath, JsonConvert.SerializeObject(CreationsPosts, Formatting.Indented));
                        Log.Information("[{Type}] Creations successfully handled.", "Creations");
                    }
                    else
                    {
                        Log.Information("[{Type}] No new Creations found.", "Creations");
                    }
                }
            }
            catch (Exception x)
            {
                Log.Warning("[{Type}] Creations request failed. {Exception}", "Creations", x);
            }
        }
    }
}
