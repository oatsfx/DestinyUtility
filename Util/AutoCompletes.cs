﻿using Discord;
using Discord.Interactions;
using Levante.Commands;
using Levante.Configs;
using Levante.Helpers;
using Levante.Rotations;
using Levante.Rotations.Abstracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Threading.Tasks;

namespace Levante.Util
{
    public class WeaponAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            await Task.Delay(0);
            var random = new Random();
            // Create a collection with suggestions for autocomplete
            List<AutocompleteResult> results = new();
            string SearchQuery = autocompleteInteraction.Data.Current.Value.ToString();
            if (String.IsNullOrWhiteSpace(SearchQuery))
            {
                while (results.Count < 7)
                {
                    var weapon = ManifestHelper.Weapons.ElementAt(random.Next(0, ManifestHelper.Weapons.Count));
                    if (!results.Exists(x => x.Name.Equals($"{weapon.Value}")))
                        results.Add(new AutocompleteResult(weapon.Value, $"{weapon.Key}"));
                }
            }

            else
                foreach (var Weapon in ManifestHelper.Weapons)
                    if (Weapon.Value.Split('[')[0].ToLower().Contains(SearchQuery.ToLower()))
                        results.Add(new AutocompleteResult(Weapon.Value, $"{Weapon.Key}"));


            results = results.OrderBy(x => x.Name).ToList();

            // max - 25 suggestions at a time (API limit)
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }

    public class Ada1ItemsAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            await Task.Delay(0);
            var random = new Random();
            // Create a collection with suggestions for autocomplete
            List<AutocompleteResult> results = new();
            string SearchQuery = autocompleteInteraction.Data.Current.Value.ToString();
            if (String.IsNullOrWhiteSpace(SearchQuery))
                while (results.Count < 7)
                {
                    var item = ManifestHelper.Ada1Items.ElementAt(random.Next(0, ManifestHelper.Ada1Items.Count));
                    if (!results.Exists(x => x.Name.Equals($"{item.Value}")))
                        results.Add(new AutocompleteResult(item.Value, $"{item.Key}"));
                }
            else
                foreach (var Item in ManifestHelper.Ada1Items)
                    if (Item.Value.ToLower().Contains(SearchQuery.ToLower()))
                        results.Add(new AutocompleteResult(Item.Value, $"{Item.Key}"));


            results = results.OrderBy(x => x.Name).ToList();

            // max - 25 suggestions at a time (API limit)
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }

    public class CurrentOfferAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            await Task.Delay(0);
            // Create a collection with suggestions for autocomplete
            List<AutocompleteResult> results = new();
            string SearchQuery = autocompleteInteraction.Data.Current.Value.ToString();

            if (String.IsNullOrWhiteSpace(SearchQuery))
                foreach (var Offer in EmblemOffer.CurrentOffers)
                    results.Add(new AutocompleteResult(Offer.OfferedEmblem.GetName(), $"{Offer.EmblemHashCode}"));
            else
                foreach (var Offer in EmblemOffer.CurrentOffers)
                    if (Offer.OfferedEmblem.GetName().ToLower().Contains(SearchQuery.ToLower()))
                        results.Add(new AutocompleteResult(Offer.OfferedEmblem.GetName(), $"{Offer.EmblemHashCode}"));

            results = results.OrderBy(x => x.Name).ToList();

            // max - 25 suggestions at a time (API limit)
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }

    public class CountdownAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            await Task.Delay(0);
            // Create a collection with suggestions for autocomplete
            List<AutocompleteResult> results = new();
            string SearchQuery = autocompleteInteraction.Data.Current.Value.ToString();

            if (String.IsNullOrWhiteSpace(SearchQuery))
                foreach (var Countdown in CountdownConfig.Countdowns)
                    results.Add(new AutocompleteResult(Countdown.Key, Countdown.Key));
            else
                foreach (var Countdown in CountdownConfig.Countdowns)
                    if (Countdown.Key.ToLower().Contains(SearchQuery.ToLower()))
                        results.Add(new AutocompleteResult(Countdown.Key, Countdown.Key));

            results = results.OrderBy(x => x.Name).ToList();

            // max - 25 suggestions at a time (API limit)
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }

    public class BungieTagAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            await Task.Delay(0);
            // Create a collection with suggestions for autocomplete
            List<AutocompleteResult> results = new();
            string SearchQuery = autocompleteInteraction.Data.Current.Value.ToString();
            if (String.IsNullOrWhiteSpace(SearchQuery))
                return AutocompletionResult.FromSuccess();
            else if (SearchQuery.Contains('#'))
            {
                foreach (var linkUser in DataConfig.DiscordIDLinks)
                    if (linkUser.UniqueBungieName.ToLower().Contains(SearchQuery.ToLower()))
                        results.Add(new AutocompleteResult($"{linkUser.UniqueBungieName}", $"{linkUser.UniqueBungieName}"));
            }
            else
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-API-Key", BotConfig.BungieApiKey);
                    //Attempt to use post, but results in Error Code 30.
                    //var values = new Dictionary<string, string>
                    //{
                    //    { "displayNamePrefix", $"{SearchQuery}" }
                    //};
                    //var postContent = new FormUrlEncodedContent(values);

                    //var response = client.PostAsync("https://www.bungie.net/Platform/User/Search/GlobalName/0/", postContent).Result;
                    var response = client.GetAsync($"https://www.bungie.net/Platform/User/Search/Prefix/{SearchQuery}/0/").Result;
                    var content = response.Content.ReadAsStringAsync().Result;
                    dynamic item = JsonConvert.DeserializeObject(content);

                    foreach (var result in item.Response.searchResults)
                        results.Add(new AutocompleteResult($"{result.bungieGlobalDisplayName}#{$"{result.bungieGlobalDisplayNameCode}".PadLeft(4, '0')}",
                            $"{result.bungieGlobalDisplayName}#{$"{result.bungieGlobalDisplayNameCode}".PadLeft(4, '0')}"));
                }
            }

            results = results.OrderBy(x => x.Name).ToList();

            // max - 25 suggestions at a time (API limit)
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }

    public class EmblemAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            await Task.Delay(0);
            var random = new Random();
            // Create a collection with suggestions for autocomplete
            List<AutocompleteResult> results = new();
            string SearchQuery = autocompleteInteraction.Data.Current.Value.ToString();
            if (String.IsNullOrWhiteSpace(SearchQuery))
                while (results.Count < 7)
                {
                    var emblem = ManifestHelper.Emblems.ElementAt(random.Next(0, ManifestHelper.Emblems.Count));
                    if (!results.Exists(x => x.Name.Equals($"{emblem.Value}")))
                        results.Add(new AutocompleteResult(emblem.Value, $"{emblem.Key}"));
                }
            else
                foreach (var Emblem in ManifestHelper.Emblems)
                    if (Emblem.Value.ToLower().Contains(SearchQuery.ToLower()))
                        results.Add(new AutocompleteResult(Emblem.Value, $"{Emblem.Key}"));

            results = results.OrderBy(x => x.Name).ToList();

            // max - 25 suggestions at a time (API limit)
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }

    public class WeaponPerkAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            await Task.Delay(0);
            var random = new Random();
            // Create a collection with suggestions for autocomplete
            List<AutocompleteResult> results = new();
            string SearchQuery = autocompleteInteraction.Data.Current.Value.ToString();
            if (String.IsNullOrWhiteSpace(SearchQuery))
                while (results.Count < 7)
                {
                    var perk = ManifestHelper.Perks.ElementAt(random.Next(0, ManifestHelper.Perks.Count));
                    if (!results.Exists(x => x.Name.Equals($"{perk.Value}")))
                        results.Add(new AutocompleteResult(perk.Value, $"{perk.Key}"));
                }
            else
                foreach (var Perk in ManifestHelper.Perks)
                    if (Perk.Value.ToLower().Contains(SearchQuery.ToLower()))
                        results.Add(new AutocompleteResult(Perk.Value, $"{Perk.Key}"));

            results = results.OrderBy(x => x.Name).ToList();

            // max - 25 suggestions at a time (API limit)
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }

    public class AltarsOfSorrowAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            await Task.Delay(0);
            // Create a collection with suggestions for autocomplete
            List<AutocompleteResult> results = new();
            var searchList = CurrentRotations.AltarsOfSorrow.Rotations;
            string SearchQuery = autocompleteInteraction.Data.Current.Value.ToString();
            if (String.IsNullOrWhiteSpace(SearchQuery))
                for (int i = 0; i < searchList.Count; i++)
                    results.Add(new AutocompleteResult($"{searchList[i]}", i));
            else
                for (int i = 0; i < searchList.Count; i++)
                    if ($"{searchList[i]}".ToLower().Contains(SearchQuery.ToLower()))
                        results.Add(new AutocompleteResult($"{searchList[i]}", i));

            // max - 25 suggestions at a time (API limit)
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }

    public class AscendantChallengeAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            await Task.Delay(0);
            // Create a collection with suggestions for autocomplete
            List<AutocompleteResult> results = new();
            var searchList = CurrentRotations.AscendantChallenge.Rotations;
            string SearchQuery = autocompleteInteraction.Data.Current.Value.ToString();
            if (String.IsNullOrWhiteSpace(SearchQuery))
                for (int i = 0; i < searchList.Count; i++)
                    results.Add(new AutocompleteResult($"{searchList[i]}", i));
            else
                for (int i = 0; i < searchList.Count; i++)
                    if ($"{searchList[i]}".ToLower().Contains(SearchQuery.ToLower()))
                        results.Add(new AutocompleteResult($"{searchList[i]}", i));

            // max - 25 suggestions at a time (API limit)
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }

    public class CurseWeekAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            await Task.Delay(0);
            // Create a collection with suggestions for autocomplete
            List<AutocompleteResult> results = new();
            var searchList = CurrentRotations.CurseWeek.Rotations;
            string SearchQuery = autocompleteInteraction.Data.Current.Value.ToString();
            if (String.IsNullOrWhiteSpace(SearchQuery))
                for (int i = 0; i < searchList.Count; i++)
                    results.Add(new AutocompleteResult($"{searchList[i]}", i));
            else
                for (int i = 0; i < searchList.Count; i++)
                    if ($"{searchList[i]}".ToLower().Contains(SearchQuery.ToLower()))
                        results.Add(new AutocompleteResult($"{searchList[i]}", i));

            // max - 25 suggestions at a time (API limit)
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }

    public class DeepStoneCryptAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            await Task.Delay(0);
            // Create a collection with suggestions for autocomplete
            List<AutocompleteResult> results = new();
            var searchList = CurrentRotations.DeepStoneCrypt.Rotations;
            string SearchQuery = autocompleteInteraction.Data.Current.Value.ToString();
            if (String.IsNullOrWhiteSpace(SearchQuery))
                for (int i = 0; i < searchList.Count; i++)
                    results.Add(new AutocompleteResult($"{searchList[i]}", i));
            else
                for (int i = 0; i < searchList.Count; i++)
                    if ($"{searchList[i]}".ToLower().Contains(SearchQuery.ToLower()))
                        results.Add(new AutocompleteResult($"{searchList[i]}", i));

            // max - 25 suggestions at a time (API limit)
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }

    public class EmpireHuntAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            await Task.Delay(0);
            // Create a collection with suggestions for autocomplete
            List<AutocompleteResult> results = new();
            var searchList = CurrentRotations.EmpireHunt.Rotations;
            string SearchQuery = autocompleteInteraction.Data.Current.Value.ToString();
            if (String.IsNullOrWhiteSpace(SearchQuery))
                for (int i = 0; i < searchList.Count; i++)
                    results.Add(new AutocompleteResult($"{searchList[i]}", i));
            else
                for (int i = 0; i < searchList.Count; i++)
                    if ($"{searchList[i]}".ToLower().Contains(SearchQuery.ToLower()))
                        results.Add(new AutocompleteResult($"{searchList[i]}", i));

            // max - 25 suggestions at a time (API limit)
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }

    public class FeaturedRaidAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            await Task.Delay(0);
            // Create a collection with suggestions for autocomplete
            List<AutocompleteResult> results = new();
            var searchList = CurrentRotations.FeaturedRaid.Rotations;
            string SearchQuery = autocompleteInteraction.Data.Current.Value.ToString();
            if (String.IsNullOrWhiteSpace(SearchQuery))
                for (int i = 0; i < searchList.Count; i++)
                    results.Add(new AutocompleteResult($"{searchList[i]}", i));
            else
                for (int i = 0; i < searchList.Count; i++)
                    if ($"{searchList[i]}".ToLower().Contains(SearchQuery.ToLower()))
                        results.Add(new AutocompleteResult($"{searchList[i]}", i));

            // max - 25 suggestions at a time (API limit)
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }

    public class FeaturedDungeonAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            await Task.Delay(0);
            // Create a collection with suggestions for autocomplete
            List<AutocompleteResult> results = new();
            var searchList = CurrentRotations.FeaturedDungeon.Rotations;
            string SearchQuery = autocompleteInteraction.Data.Current.Value.ToString();
            if (String.IsNullOrWhiteSpace(SearchQuery))
                for (int i = 0; i < searchList.Count; i++)
                    results.Add(new AutocompleteResult($"{searchList[i]}", i));
            else
                for (int i = 0; i < searchList.Count; i++)
                    if ($"{searchList[i]}".ToLower().Contains(SearchQuery.ToLower()))
                        results.Add(new AutocompleteResult($"{searchList[i]}", i));

            // max - 25 suggestions at a time (API limit)
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }

    public class GardenOfSalvationAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            await Task.Delay(0);
            // Create a collection with suggestions for autocomplete
            List<AutocompleteResult> results = new();
            var searchList = CurrentRotations.GardenOfSalvation.Rotations;
            string SearchQuery = autocompleteInteraction.Data.Current.Value.ToString();
            if (String.IsNullOrWhiteSpace(SearchQuery))
                for (int i = 0; i < searchList.Count; i++)
                    results.Add(new AutocompleteResult($"{searchList[i]}", i));
            else
                for (int i = 0; i < searchList.Count; i++)
                    if ($"{searchList[i]}".ToLower().Contains(SearchQuery.ToLower()))
                        results.Add(new AutocompleteResult($"{searchList[i]}", i));

            // max - 25 suggestions at a time (API limit)
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }

    public class KingsFallAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            await Task.Delay(0);
            // Create a collection with suggestions for autocomplete
            List<AutocompleteResult> results = new();
            var searchList = CurrentRotations.KingsFall.Rotations;
            string SearchQuery = autocompleteInteraction.Data.Current.Value.ToString();
            if (String.IsNullOrWhiteSpace(SearchQuery))
                for (int i = 0; i < searchList.Count; i++)
                    results.Add(new AutocompleteResult($"{searchList[i]}", i));
            else
                for (int i = 0; i < searchList.Count; i++)
                    if ($"{searchList[i]}".ToLower().Contains(SearchQuery.ToLower()))
                        results.Add(new AutocompleteResult($"{searchList[i]}", i));

            // max - 25 suggestions at a time (API limit)
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }

    public class LastWishAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            await Task.Delay(0);
            // Create a collection with suggestions for autocomplete
            List<AutocompleteResult> results = new();
            var searchList = CurrentRotations.LastWish.Rotations;
            string SearchQuery = autocompleteInteraction.Data.Current.Value.ToString();
            if (String.IsNullOrWhiteSpace(SearchQuery))
                for (int i = 0; i < searchList.Count; i++)
                    results.Add(new AutocompleteResult($"{searchList[i]}", i));
            else
                for (int i = 0; i < searchList.Count; i++)
                    if ($"{searchList[i]}".ToLower().Contains(SearchQuery.ToLower()))
                        results.Add(new AutocompleteResult($"{searchList[i]}", i));

            // max - 25 suggestions at a time (API limit)
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }

    public class LightfallMissionAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            await Task.Delay(0);
            // Create a collection with suggestions for autocomplete
            List<AutocompleteResult> results = new();
            var searchList = CurrentRotations.LightfallMission.Rotations;
            string SearchQuery = autocompleteInteraction.Data.Current.Value.ToString();
            if (String.IsNullOrWhiteSpace(SearchQuery))
                for (int i = 0; i < searchList.Count; i++)
                    results.Add(new AutocompleteResult($"{searchList[i]}", i));
            else
                for (int i = 0; i < searchList.Count; i++)
                    if ($"{searchList[i]}".ToLower().Contains(SearchQuery.ToLower()))
                        results.Add(new AutocompleteResult($"{searchList[i]}", i));

            // max - 25 suggestions at a time (API limit)
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }

    public class LostSectorAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            await Task.Delay(0);
            // Create a collection with suggestions for autocomplete
            List<AutocompleteResult> results = new();
            var searchList = CurrentRotations.LostSector.Rotations;
            string SearchQuery = autocompleteInteraction.Data.Current.Value.ToString();
            if (String.IsNullOrWhiteSpace(SearchQuery))
                for (int i = 0; i < searchList.Count; i++)
                    results.Add(new AutocompleteResult($"{searchList[i]}", i));
            else
                for (int i = 0; i < searchList.Count; i++)
                    if ($"{searchList[i]}".ToLower().Contains(SearchQuery.ToLower()))
                        results.Add(new AutocompleteResult($"{searchList[i]}", i));

            // max - 25 suggestions at a time (API limit)
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }

    public class ExoticArmorAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            await Task.Delay(0);
            // Create a collection with suggestions for autocomplete
            List<AutocompleteResult> results = new();
            var searchList = CurrentRotations.LostSector.ArmorRotations;
            string SearchQuery = autocompleteInteraction.Data.Current.Value.ToString();
            if (String.IsNullOrWhiteSpace(SearchQuery))
                for (int i = 0; i < searchList.Count; i++)
                    results.Add(new AutocompleteResult($"{searchList[i]}", i));
            else
                for (int i = 0; i < searchList.Count; i++)
                    if ($"{searchList[i]}".ToLower().Contains(SearchQuery.ToLower()))
                        results.Add(new AutocompleteResult($"{searchList[i]}", i));

            // max - 25 suggestions at a time (API limit)
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }

    public class NightfallAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            await Task.Delay(0);
            // Create a collection with suggestions for autocomplete
            List<AutocompleteResult> results = new();
            var searchList = CurrentRotations.Nightfall.Rotations;
            string SearchQuery = autocompleteInteraction.Data.Current.Value.ToString();
            if (String.IsNullOrWhiteSpace(SearchQuery))
                for (int i = 0; i < searchList.Count; i++)
                    results.Add(new AutocompleteResult($"{searchList[i]}", i));
            else
                for (int i = 0; i < searchList.Count; i++)
                    if ($"{searchList[i]}".ToLower().Contains(SearchQuery.ToLower()))
                        results.Add(new AutocompleteResult($"{searchList[i]}", i));

            // max - 25 suggestions at a time (API limit)
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }

    public class NightfallWeaponAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            await Task.Delay(0);
            // Create a collection with suggestions for autocomplete
            List<AutocompleteResult> results = new();
            var searchList = CurrentRotations.Nightfall.WeaponRotations;
            string SearchQuery = autocompleteInteraction.Data.Current.Value.ToString();
            if (String.IsNullOrWhiteSpace(SearchQuery))
                for (int i = 0; i < searchList.Count; i++)
                    results.Add(new AutocompleteResult($"{searchList[i]}", i));
            else
                for (int i = 0; i < searchList.Count; i++)
                    if ($"{searchList[i]}".ToLower().Contains(SearchQuery.ToLower()))
                        results.Add(new AutocompleteResult($"{searchList[i]}", i));

            // max - 25 suggestions at a time (API limit)
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }

    public class NightmareHuntAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            await Task.Delay(0);
            // Create a collection with suggestions for autocomplete
            List<AutocompleteResult> results = new();
            var searchList = CurrentRotations.NightmareHunt.Rotations;
            string SearchQuery = autocompleteInteraction.Data.Current.Value.ToString();
            if (String.IsNullOrWhiteSpace(SearchQuery))
                for (int i = 0; i < searchList.Count; i++)
                    results.Add(new AutocompleteResult($"{searchList[i]}", i));
            else
                for (int i = 0; i < searchList.Count; i++)
                    if ($"{searchList[i]}".ToLower().Contains(SearchQuery.ToLower()))
                        results.Add(new AutocompleteResult($"{searchList[i]}", i));

            // max - 25 suggestions at a time (API limit)
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }

    public class RootOfNightmaresAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            await Task.Delay(0);
            List<AutocompleteResult> results = new();
            var searchList = CurrentRotations.RootOfNightmares.Rotations;
            string SearchQuery = autocompleteInteraction.Data.Current.Value.ToString();
            if (String.IsNullOrWhiteSpace(SearchQuery))
                for (int i = 0; i < searchList.Count; i++)
                    results.Add(new AutocompleteResult($"{searchList[i]}", i));
            else
                for (int i = 0; i < searchList.Count; i++)
                    if ($"{searchList[i]}".ToLower().Contains(SearchQuery.ToLower()))
                        results.Add(new AutocompleteResult($"{searchList[i]}", i));

            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }

    public class ShadowkeepMissionAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            await Task.Delay(0);
            // Create a collection with suggestions for autocomplete
            List<AutocompleteResult> results = new();
            var searchList = CurrentRotations.ShadowkeepMission.Rotations;
            string SearchQuery = autocompleteInteraction.Data.Current.Value.ToString();
            if (String.IsNullOrWhiteSpace(SearchQuery))
                for (int i = 0; i < searchList.Count; i++)
                    results.Add(new AutocompleteResult($"{searchList[i]}", i));
            else
                for (int i = 0; i < searchList.Count; i++)
                    if ($"{searchList[i]}".ToLower().Contains(SearchQuery.ToLower()))
                        results.Add(new AutocompleteResult($"{searchList[i]}", i));

            // max - 25 suggestions at a time (API limit)
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }

    public class TerminalOverloadAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            await Task.Delay(0);
            List<AutocompleteResult> results = new();
            var searchList = CurrentRotations.TerminalOverload.Rotations;
            string SearchQuery = autocompleteInteraction.Data.Current.Value.ToString();
            if (String.IsNullOrWhiteSpace(SearchQuery))
                for (int i = 0; i < searchList.Count; i++)
                    results.Add(new AutocompleteResult($"{searchList[i]}", i));
            else
                for (int i = 0; i < searchList.Count; i++)
                    if ($"{searchList[i]}".ToLower().Contains(SearchQuery.ToLower()))
                        results.Add(new AutocompleteResult($"{searchList[i]}", i));

            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }

    public class VaultOfGlassAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            await Task.Delay(0);
            List<AutocompleteResult> results = new();
            var searchList = CurrentRotations.VaultOfGlass.Rotations;
            string SearchQuery = autocompleteInteraction.Data.Current.Value.ToString();
            if (String.IsNullOrWhiteSpace(SearchQuery))
                for (int i = 0; i < searchList.Count; i++)
                    results.Add(new AutocompleteResult($"{searchList[i]}", i));
            else
                for (int i = 0; i < searchList.Count; i++)
                    if ($"{searchList[i]}".ToLower().Contains(SearchQuery.ToLower()))
                        results.Add(new AutocompleteResult($"{searchList[i]}", i));

            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }

    public class VowOfTheDiscipleAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            await Task.Delay(0);
            List<AutocompleteResult> results = new();
            var searchList = CurrentRotations.VowOfTheDisciple.Rotations;
            string SearchQuery = autocompleteInteraction.Data.Current.Value.ToString();
            if (String.IsNullOrWhiteSpace(SearchQuery))
                for (int i = 0; i < searchList.Count; i++)
                    results.Add(new AutocompleteResult($"{searchList[i]}", i));
            else
                for (int i = 0; i < searchList.Count; i++)
                    if ($"{searchList[i]}".ToLower().Contains(SearchQuery.ToLower()))
                        results.Add(new AutocompleteResult($"{searchList[i]}", i));

            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }

    public class WellspringAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            await Task.Delay(0);
            List<AutocompleteResult> results = new();
            var searchList = CurrentRotations.Wellspring.Rotations;
            string SearchQuery = autocompleteInteraction.Data.Current.Value.ToString();
            if (String.IsNullOrWhiteSpace(SearchQuery))
                for (int i = 0; i < searchList.Count; i++)
                    results.Add(new AutocompleteResult($"{searchList[i]}", i));
            else
                for (int i = 0; i < searchList.Count; i++)
                    if ($"{searchList[i]}".ToLower().Contains(SearchQuery.ToLower()))
                        results.Add(new AutocompleteResult($"{searchList[i]}", i));

            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }

    public class WitchQueenMissionAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            await Task.Delay(0);
            // Create a collection with suggestions for autocomplete
            List<AutocompleteResult> results = new();
            var searchList = CurrentRotations.WitchQueenMission.Rotations;
            string SearchQuery = autocompleteInteraction.Data.Current.Value.ToString();
            if (String.IsNullOrWhiteSpace(SearchQuery))
                for (int i = 0; i < searchList.Count; i++)
                    results.Add(new AutocompleteResult($"{searchList[i]}", i));
            else
                for (int i = 0; i < searchList.Count; i++)
                    if ($"{searchList[i]}".ToLower().Contains(SearchQuery.ToLower()))
                        results.Add(new AutocompleteResult($"{searchList[i]}", i));

            // max - 25 suggestions at a time (API limit)
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }
}