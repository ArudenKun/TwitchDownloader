﻿using NeoSmart.Unicode;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TwitchDownloaderCore.Chat;
using TwitchDownloaderCore.Tools;
using TwitchDownloaderCore.VideoPlatforms.Interfaces;
using TwitchDownloaderCore.VideoPlatforms.Twitch.Api;
using TwitchDownloaderCore.VideoPlatforms.Twitch.Gql;

namespace TwitchDownloaderCore.VideoPlatforms.Twitch
{
    public static class TwitchHelper
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private static readonly string[] bttvZeroWidth = { "SoSnowy", "IceCold", "SantaHat", "TopHat", "ReinDeer", "CandyCane", "cvMask", "cvHazmat" };

        public static async Task<GqlVideoResponse> GetVideoInfo(int videoId, string Oauth = null)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri("https://gql.twitch.tv/gql"),
                Method = HttpMethod.Post,
                Content = new StringContent("{\"query\":\"query{video(id:\\\"" + videoId + "\\\"){title,thumbnailURLs(height:180,width:320),createdAt,lengthSeconds,owner{id,displayName},viewCount,game{id,displayName}}}\",\"variables\":{}}", Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Client-ID", "kimne78kx3ncx6brgo4mv6wki5h1ko");
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            GqlVideoResponse res = await response.Content.ReadFromJsonAsync<GqlVideoResponse>();

            GqlVideoTokenResponse accessToken = await TwitchHelper.GetVideoToken(videoId, Oauth);
            if (accessToken is null)
            {
                throw new NullReferenceException("Invalid VOD, deleted/expired VOD possibly?");
            }

            string[] playlist = await TwitchHelper.GetVideoPlaylist(videoId, accessToken.data.videoPlaybackAccessToken.value, accessToken.data.videoPlaybackAccessToken.signature);
            if (playlist[0].Contains("vod_manifest_restricted"))
            {
                throw new NullReferenceException("Insufficient access. OAuth may be required.");
            }
            res.VideoQualities = new List<VideoQuality>();

            for (int i = 0; i < playlist.Length; i++)
            {
                if (playlist[i].Contains("#EXT-X-MEDIA"))
                {
                    string lastPart = playlist[i].Substring(playlist[i].IndexOf("NAME=\"") + 6);
                    string stringQuality = lastPart.Substring(0, lastPart.IndexOf('"'));

                    var bandwidthStartIndex = playlist[i + 1].IndexOf("BANDWIDTH=") + 10;
                    var bandwidthEndIndex = playlist[i + 1].IndexOf(',') - bandwidthStartIndex;
                    int.TryParse(playlist[i + 1].Substring(bandwidthStartIndex, bandwidthEndIndex), out var bandwidth);

                    res.VideoQualities.Add(new VideoQuality { Quality = stringQuality, SourceUrl = playlist[i + 2], Bandwidth = bandwidth });
                }
            }

            return res;
        }

        public static async Task<GqlVideoTokenResponse> GetVideoToken(int videoId, string authToken)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri("https://gql.twitch.tv/gql"),
                Method = HttpMethod.Post,
                Content = new StringContent("{\"operationName\":\"PlaybackAccessToken_Template\",\"query\":\"query PlaybackAccessToken_Template($login: String!, $isLive: Boolean!, $vodID: ID!, $isVod: Boolean!, $playerType: String!) {  streamPlaybackAccessToken(channelName: $login, params: {platform: \\\"web\\\", playerBackend: \\\"mediaplayer\\\", playerType: $playerType}) @include(if: $isLive) {    value    signature    __typename  }  videoPlaybackAccessToken(id: $vodID, params: {platform: \\\"web\\\", playerBackend: \\\"mediaplayer\\\", playerType: $playerType}) @include(if: $isVod) {    value    signature    __typename  }}\",\"variables\":{\"isLive\":false,\"login\":\"\",\"isVod\":true,\"vodID\":\"" + videoId + "\",\"playerType\":\"embed\"}}", Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Client-ID", "kimne78kx3ncx6brgo4mv6wki5h1ko");
            if (!string.IsNullOrWhiteSpace(authToken))
                request.Headers.Add("Authorization", $"OAuth {authToken}");
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<GqlVideoTokenResponse>();
        }

        public static async Task<string[]> GetVideoPlaylist(int videoId, string token, string sig)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"http://usher.ttvnw.net/vod/{videoId}?nauth={token}&nauthsig={sig}&allow_source=true&player=twitchweb"),
                Method = HttpMethod.Get
            };
            request.Headers.Add("Client-ID", "kimne78kx3ncx6brgo4mv6wki5h1ko");
            string playlist = await (await httpClient.SendAsync(request)).Content.ReadAsStringAsync();
            return playlist.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        }

        public static async Task<GqlClipResponse> GetClipInfo(object clipId)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri("https://gql.twitch.tv/gql"),
                Method = HttpMethod.Post,
                Content = new StringContent("{\"query\":\"query{clip(slug:\\\"" + clipId + "\\\"){title,thumbnailURL,createdAt,durationSeconds,broadcaster{id,displayName},videoOffsetSeconds,video{id},viewCount,game{id,displayName}}}\",\"variables\":{}}", Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Client-ID", "kimne78kx3ncx6brgo4mv6wki5h1ko");
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<GqlClipResponse>();
        }

        public static async Task<List<GqlClipTokenResponse>> GetClipLinks(string clipId)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri("https://gql.twitch.tv/gql"),
                Method = HttpMethod.Post,
                Content = new StringContent("[{\"operationName\":\"VideoAccessToken_Clip\",\"variables\":{\"slug\":\"" + clipId + "\"},\"extensions\":{\"persistedQuery\":{\"version\":1,\"sha256Hash\":\"36b89d2507fce29e5ca551df756d27c1cfe079e2609642b4390aa4c35796eb11\"}}}]", Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Client-ID", "kimne78kx3ncx6brgo4mv6wki5h1ko");
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<GqlClipTokenResponse>>();
        }

        public static async Task<GqlVideoSearchResponse> GetGqlVideos(string channelName, string cursor = "", int limit = 50)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri("https://gql.twitch.tv/gql"),
                Method = HttpMethod.Post,
                Content = new StringContent("{\"query\":\"query{user(login:\\\"" + channelName + "\\\"){videos(first: " + limit + "" + (cursor == "" ? "" : ",after:\\\"" + cursor + "\\\"") + ") { edges { node { title, id, lengthSeconds, previewThumbnailURL(height: 180, width: 320), createdAt, viewCount, game { id, displayName } }, cursor }, pageInfo { hasNextPage, hasPreviousPage }, totalCount }}}\",\"variables\":{}}", Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Client-ID", "kd1unb4b3q4t58fwlpcbzcbnm76a8fp");
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<GqlVideoSearchResponse>();
        }

        public static async Task<GqlClipSearchResponse> GetGqlClips(string channelName, string period = "LAST_WEEK", string cursor = "", int limit = 50)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri("https://gql.twitch.tv/gql"),
                Method = HttpMethod.Post,
                Content = new StringContent("{\"query\":\"query{user(login:\\\"" + channelName + "\\\"){clips(first: " + limit + (cursor == "" ? "" : ", after: \\\"" + cursor + "\\\"") + ", criteria: { period: " + period + " }) {  edges { cursor, node { id, slug, title, createdAt, durationSeconds, thumbnailURL, viewCount, game { id, displayName } } }, pageInfo { hasNextPage, hasPreviousPage } }}}\",\"variables\":{}}", Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Client-ID", "kd1unb4b3q4t58fwlpcbzcbnm76a8fp");
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<GqlClipSearchResponse>();
        }

        public static async Task<EmoteResponse> GetThirdPartyEmoteData(int streamerId, bool getBttv, bool getFfz, bool getStv, bool allowUnlistedEmotes, CancellationToken cancellationToken = new())
        {
            cancellationToken.ThrowIfCancellationRequested();

            EmoteResponse emoteReponse = new();

            if (getBttv)
            {
                await GetBttvEmoteData(streamerId, emoteReponse.BTTV);
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (getFfz)
            {
                await GetFfzEmoteData(streamerId, emoteReponse.FFZ);
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (getStv)
            {
                await PlatformHelper.GetStvEmoteData(streamerId, emoteReponse.STV, allowUnlistedEmotes, VideoPlatform.Twitch);
            }

            return emoteReponse;
        }

        private static async Task GetBttvEmoteData(int streamerId, List<EmoteResponseItem> bttvResponse)
        {
            var globalEmoteRequest = new HttpRequestMessage(HttpMethod.Get, new Uri("https://api.betterttv.net/3/cached/emotes/global", UriKind.Absolute));
            using var globalEmoteResponse = await httpClient.SendAsync(globalEmoteRequest, HttpCompletionOption.ResponseHeadersRead);
            globalEmoteResponse.EnsureSuccessStatusCode();
            var BTTV = await globalEmoteResponse.Content.ReadFromJsonAsync<List<BTTVEmote>>();

            //Channel might not have BTTV emotes
            try
            {
                var channelEmoteRequest = new HttpRequestMessage(HttpMethod.Get, new Uri($"https://api.betterttv.net/3/cached/users/twitch/{streamerId}", UriKind.Absolute));
                using var channelEmoteResponse = await httpClient.SendAsync(channelEmoteRequest, HttpCompletionOption.ResponseHeadersRead);
                channelEmoteResponse.EnsureSuccessStatusCode();

                var bttvChannel = await channelEmoteResponse.Content.ReadFromJsonAsync<BTTVChannelEmoteResponse>();
                BTTV.AddRange(bttvChannel.channelEmotes);
                BTTV.AddRange(bttvChannel.sharedEmotes);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound) { }

            foreach (var emote in BTTV)
            {
                string id = emote.id;
                string name = emote.code;
                string mime = emote.imageType;
                string url = $"https://cdn.betterttv.net/emote/{id}/[scale]x";
                bttvResponse.Add(new EmoteResponseItem() { Id = id, Code = name, ImageType = mime, ImageUrl = url, IsZeroWidth = bttvZeroWidth.Contains(name) });
            }
        }

        private static async Task GetFfzEmoteData(int streamerId, List<EmoteResponseItem> ffzResponse)
        {
            var globalEmoteRequest = new HttpRequestMessage(HttpMethod.Get, new Uri("https://api.betterttv.net/3/cached/frankerfacez/emotes/global", UriKind.Absolute));
            using var globalEmoteResponse = await httpClient.SendAsync(globalEmoteRequest, HttpCompletionOption.ResponseHeadersRead);
            globalEmoteResponse.EnsureSuccessStatusCode();
            var FFZ = await globalEmoteResponse.Content.ReadFromJsonAsync<List<FFZEmote>>();

            //Channel might not have FFZ emotes
            try
            {
                var channelEmoteRequest = new HttpRequestMessage(HttpMethod.Get, new Uri($"https://api.betterttv.net/3/cached/frankerfacez/users/twitch/{streamerId}", UriKind.Absolute));
                using var channelEmoteResponse = await httpClient.SendAsync(channelEmoteRequest, HttpCompletionOption.ResponseHeadersRead);
                channelEmoteResponse.EnsureSuccessStatusCode();

                var channelEmotes = await channelEmoteResponse.Content.ReadFromJsonAsync<List<FFZEmote>>();
                FFZ.AddRange(channelEmotes);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound) { }

            foreach (var emote in FFZ)
            {
                string id = emote.id.ToString();
                string name = emote.code;
                string mime = emote.imageType;
                string url = emote.animated
                    ? $"https://cdn.betterttv.net/frankerfacez_emote/{id}/animated/[scale]"
                    : $"https://cdn.betterttv.net/frankerfacez_emote/{id}/[scale]";
                ffzResponse.Add(new EmoteResponseItem() { Id = id, Code = name, ImageType = mime, ImageUrl = url });
            }
        }


        public static async Task<List<TwitchEmote>> GetThirdPartyEmotes(List<Comment> comments, int streamerId, string cacheFolder, EmbeddedData embeddedData = null, bool bttv = true, bool ffz = true, bool stv = true, bool allowUnlistedEmotes = true, bool offline = false, CancellationToken cancellationToken = new())
        {
            List<TwitchEmote> returnList = new List<TwitchEmote>();
            List<string> alreadyAdded = new List<string>();

            // No 3rd party emotes are wanted
            if (!bttv && !ffz && !stv)
            {
                return returnList;
            }

            // Load our embedded data from file
            if (embeddedData?.thirdParty != null)
            {
                foreach (EmbedEmoteData emoteData in embeddedData.thirdParty)
                {
                    try
                    {
                        TwitchEmote newEmote = new TwitchEmote(emoteData.data, EmoteProvider.TwitchThirdParty, emoteData.imageScale, emoteData.id, emoteData.name);
                        returnList.Add(newEmote);
                        alreadyAdded.Add(emoteData.name);
                    }
                    catch { }
                }
            }

            // Directly return if we are in offline, no need for a network request
            if (offline)
            {
                return returnList;
            }

            string bttvFolder = Path.Combine(cacheFolder, "bttv");
            string ffzFolder = Path.Combine(cacheFolder, "ffz");
            string stvFolder = Path.Combine(cacheFolder, "stv");

            EmoteResponse emoteDataResponse = await GetThirdPartyEmoteData(streamerId, bttv, ffz, stv, allowUnlistedEmotes, cancellationToken);

            if (bttv)
            {
                if (!Directory.Exists(bttvFolder))
                    PlatformHelper.CreateDirectory(bttvFolder);

                var emoteResponseItemsQuery = from emote in emoteDataResponse.BTTV
                                              where !alreadyAdded.Contains(emote.Code)
                                              let pattern = $@"(?<=^|\s){Regex.Escape(emote.Code)}(?=$|\s)"
                                              where comments.Any(comment => Regex.IsMatch(comment.message.body, pattern))
                                              select emote;

                foreach (var emote in emoteResponseItemsQuery)
                {
                    try
                    {
                        TwitchEmote newEmote = new TwitchEmote(await PlatformHelper.GetImage(bttvFolder, emote.ImageUrl.Replace("[scale]", "2"), emote.Id, "2", emote.ImageType, cancellationToken), EmoteProvider.TwitchThirdParty, 2, emote.Id, emote.Code);
                        if (emote.IsZeroWidth)
                            newEmote.IsZeroWidth = true;
                        returnList.Add(newEmote);
                        alreadyAdded.Add(emote.Code);
                    }
                    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound) { }
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (ffz)
            {
                if (!Directory.Exists(ffzFolder))
                    PlatformHelper.CreateDirectory(ffzFolder);

                var emoteResponseItemsQuery = from emote in emoteDataResponse.FFZ
                                              where !alreadyAdded.Contains(emote.Code)
                                              let pattern = $@"(?<=^|\s){Regex.Escape(emote.Code)}(?=$|\s)"
                                              where comments.Any(comment => Regex.IsMatch(comment.message.body, pattern))
                                              select emote;

                foreach (var emote in emoteResponseItemsQuery)
                {
                    try
                    {
                        TwitchEmote newEmote = new TwitchEmote(await PlatformHelper.GetImage(ffzFolder, emote.ImageUrl.Replace("[scale]", "2"), emote.Id, "2", emote.ImageType, cancellationToken), EmoteProvider.TwitchThirdParty, 2, emote.Id, emote.Code);
                        returnList.Add(newEmote);
                        alreadyAdded.Add(emote.Code);
                    }
                    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound) { }
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (stv)
            {
                if (!Directory.Exists(stvFolder))
                    PlatformHelper.CreateDirectory(stvFolder);

                var emoteResponseItemsQuery = from emote in emoteDataResponse.STV
                                              where !alreadyAdded.Contains(emote.Code)
                                              let pattern = $@"(?<=^|\s){Regex.Escape(emote.Code)}(?=$|\s)"
                                              where comments.Any(comment => Regex.IsMatch(comment.message.body, pattern))
                                              select emote;

                foreach (var emote in emoteResponseItemsQuery)
                {
                    try
                    {
                        TwitchEmote newEmote = new TwitchEmote(await PlatformHelper.GetImage(stvFolder, emote.ImageUrl.Replace("[scale]", "2"), emote.Id, "2", emote.ImageType, cancellationToken), EmoteProvider.TwitchThirdParty, 2, emote.Id, emote.Code);
                        if (emote.IsZeroWidth)
                            newEmote.IsZeroWidth = true;
                        returnList.Add(newEmote);
                        alreadyAdded.Add(emote.Code);
                    }
                    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound) { }
                }
            }

            return returnList;
        }

        public static async Task<List<TwitchEmote>> GetEmotes(List<Comment> comments, string cacheFolder, EmbeddedData embeddedData = null, bool offline = false, CancellationToken cancellationToken = default)
        {
            List<TwitchEmote> returnList = new List<TwitchEmote>();
            List<string> alreadyAdded = new List<string>();
            List<string> failedEmotes = new List<string>();

            string emoteFolder = Path.Combine(cacheFolder, "emotes");
            if (!Directory.Exists(emoteFolder))
                PlatformHelper.CreateDirectory(emoteFolder);

            // Load our embedded emotes
            if (embeddedData?.firstParty != null)
            {
                foreach (EmbedEmoteData emoteData in embeddedData.firstParty)
                {
                    try
                    {
                        TwitchEmote newEmote = new TwitchEmote(emoteData.data, EmoteProvider.TwitchFirstParty, emoteData.imageScale, emoteData.id, emoteData.name);
                        returnList.Add(newEmote);
                        alreadyAdded.Add(emoteData.id);
                    }
                    catch { }
                }
            }

            // Directly return if we are in offline, no need for a network request
            if (offline)
            {
                return returnList;
            }

            foreach (var comment in comments.Where(c => c.message.fragments != null))
            {
                cancellationToken.ThrowIfCancellationRequested();

                foreach (var id in comment.message.fragments
                             .Select(f => f.emoticon?.emoticon_id)
                             .Where(id => !alreadyAdded.Contains(id) && !failedEmotes.Contains(id)))
                {
                    try
                    {
                        byte[] bytes = await PlatformHelper.GetImage(emoteFolder, $"https://static-cdn.jtvnw.net/emoticons/v2/{id}/default/dark/2.0", id, "2", "png", cancellationToken);
                        TwitchEmote newEmote = new TwitchEmote(bytes, EmoteProvider.TwitchFirstParty, 2, id, id);
                        alreadyAdded.Add(id);
                        returnList.Add(newEmote);
                    }
                    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                    {
                        failedEmotes.Add(id);
                    }
                }
            }

            return returnList;
        }

        public static async Task<List<EmbedChatBadge>> GetChatBadgesData(List<Comment> comments, int streamerId, CancellationToken cancellationToken = new())
        {
            cancellationToken.ThrowIfCancellationRequested();

            // TODO: this currently only does twitch badges, but we could also support FFZ, BTTV, 7TV, etc badges!
            // TODO: would want to make this configurable as we do for emotes though...
            var globalBadgeRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri("https://gql.twitch.tv/gql"),
                Method = HttpMethod.Post,
                Content = new StringContent("{\"query\":\"query{badges{imageURL(size:DOUBLE),description,title,setID,version}}\",\"variables\":{}}", Encoding.UTF8, "application/json")
            };
            globalBadgeRequest.Headers.Add("Client-ID", "kimne78kx3ncx6brgo4mv6wki5h1ko");
            using var globalBadgeResponse = await httpClient.SendAsync(globalBadgeRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            globalBadgeResponse.EnsureSuccessStatusCode();
            var globalBadges = (await globalBadgeResponse.Content.ReadFromJsonAsync<GqlGlobalBadgeResponse>(cancellationToken: cancellationToken)).data.badges.GroupBy(x => x.name).ToDictionary(x => x.Key, x => x.ToList());

            var subBadgeRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri("https://gql.twitch.tv/gql"),
                Method = HttpMethod.Post,
                Content = new StringContent("{\"query\":\"query{user(id: " + streamerId + "){broadcastBadges{imageURL(size:DOUBLE),description,title,setID,version}}}\",\"variables\":{}}", Encoding.UTF8, "application/json")
            };
            subBadgeRequest.Headers.Add("Client-ID", "kimne78kx3ncx6brgo4mv6wki5h1ko");
            using var subBadgeResponse = await httpClient.SendAsync(subBadgeRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            subBadgeResponse.EnsureSuccessStatusCode();
            var subBadges = (await subBadgeResponse.Content.ReadFromJsonAsync<GqlSubBadgeResponse>(cancellationToken: cancellationToken)).data.user.badges.GroupBy(x => x.name).ToDictionary(x => x.Key, x => x.ToList());

            List<EmbedChatBadge> badges = new List<EmbedChatBadge>();

            var nameList = comments.Where(comment => comment.message.user_badges != null)
                .SelectMany(comment => comment.message.user_badges)
                .Where(badge => !string.IsNullOrWhiteSpace(badge._id))
                .Where(badge => globalBadges.ContainsKey(badge._id) || subBadges.ContainsKey(badge._id))
                .Select(badge => badge._id).Distinct();

            foreach (var name in nameList)
            {
                Dictionary<string, ChatBadgeData> versions = new();
                if (globalBadges.TryGetValue(name, out var globalBadge))
                {
                    foreach (var badge in globalBadge)
                    {
                        versions[badge.version] = new()
                        {
                            title = badge.title,
                            description = badge.description,
                            url = badge.image_url_2x
                        };
                    }
                }

                //Prefer channel specific badges over global ones
                if (subBadges.TryGetValue(name, out var subBadge))
                {
                    foreach (var badge in subBadge)
                    {
                        versions[badge.version] = new()
                        {
                            title = badge.title,
                            description = badge.description,
                            url = badge.image_url_2x
                        };
                    }
                }

                badges.Add(new EmbedChatBadge() { name = name, versions = versions });
            }

            return badges;
        }

        public static async Task<List<ChatBadge>> GetChatBadges(List<Comment> comments, int streamerId, string cacheFolder, EmbeddedData embeddedData = null, bool offline = false, CancellationToken cancellationToken = default)
        {
            List<ChatBadge> returnList = new List<ChatBadge>();
            List<string> alreadyAdded = new List<string>();

            // Load our embedded data from file
            if (embeddedData?.twitchBadges != null)
            {
                foreach (EmbedChatBadge data in embeddedData.twitchBadges)
                {
                    ChatBadge newBadge = new ChatBadge(data.name, data.versions);
                    returnList.Add(newBadge);
                    alreadyAdded.Add(data.name);
                }
            }

            // Directly return if we are in offline, no need for a network request
            if (offline)
            {
                return returnList;
            }

            List<EmbedChatBadge> badgesData = await GetChatBadgesData(comments, streamerId, cancellationToken);

            string badgeFolder = Path.Combine(cacheFolder, "badges");
            if (!Directory.Exists(badgeFolder))
                PlatformHelper.CreateDirectory(badgeFolder);

            foreach (var badge in badgesData)
            {
                try
                {
                    Dictionary<string, ChatBadgeData> versions = new();

                    if (alreadyAdded.Contains(badge.name))
                        continue;

                    foreach (var (version, data) in badge.versions)
                    {
                        string[] id_parts = data.url.Split('/');
                        string id = id_parts[id_parts.Length - 2];
                        byte[] bytes = await PlatformHelper.GetImage(badgeFolder, data.url, id, "2", "png", cancellationToken);
                        versions.Add(version, new ChatBadgeData
                        {
                            title = data.title,
                            description = data.description,
                            bytes = bytes
                        });
                    }

                    returnList.Add(new ChatBadge(badge.name, versions));
                }
                catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound) { }
            }

            return returnList;
        }

        public static async Task<Dictionary<string, SKBitmap>> GetEmojis(string cacheFolder, EmojiVendor emojiVendor, CancellationToken cancellationToken = default)
        {
            var returnCache = new Dictionary<string, SKBitmap>();

            if (emojiVendor == EmojiVendor.None)
                return returnCache;

            var emojiFolder = Path.Combine(cacheFolder, "emojis", emojiVendor.EmojiFolder());
            var emojiExtensions = new Regex(@"\.(?:png|PNG)$", RegexOptions.RightToLeft); // Extensions are case sensitive on Linux and Mac

            if (!Directory.Exists(emojiFolder))
                PlatformHelper.CreateDirectory(emojiFolder);

            var emojiFiles = Directory.GetFiles(emojiFolder)
                .Where(i => emojiExtensions.IsMatch(i)).ToArray();

            if (emojiFiles.Length < emojiVendor.EmojiCount())
            {
                var emojiZipPath = Path.Combine(emojiFolder, Path.GetRandomFileName());
                try
                {
                    using (var ms = emojiVendor.MemoryStream())
                    {
                        await using var fs = File.OpenWrite(emojiZipPath);
                        await ms.CopyToAsync(fs, cancellationToken);
                    }

                    using var archive = ZipFile.OpenRead(emojiZipPath);
                    var emojiAssetsPath = emojiVendor.AssetPath();
                    var emojis = archive.Entries
                        .Where(x => !string.IsNullOrWhiteSpace(x.Name) && Path.GetDirectoryName(x.FullName) == emojiAssetsPath);

                    foreach (var emoji in emojis)
                    {
                        var filePath = Path.Combine(emojiFolder,
                            emoji.Name.ToUpper().Replace(emojiVendor.UnicodeSequenceSeparator(), ' '));
                        if (!File.Exists(filePath))
                        {
                            try
                            {
                                emoji.ExtractToFile(filePath);
                            }
                            catch { /* Being written by a parallel process? */ }
                        }
                    }

                    emojiFiles = Directory.GetFiles(emojiFolder)
                        .Where(i => emojiExtensions.IsMatch(i)).ToArray();
                }
                finally
                {
                    if (File.Exists(emojiZipPath))
                    {
                        File.Delete(emojiZipPath);
                    }
                }
            }

            foreach (var emojiPath in emojiFiles)
            {
                await using var fs = File.OpenRead(emojiPath);
                var emojiImage = SKBitmap.Decode(fs);
                returnCache.Add(Path.GetFileNameWithoutExtension(emojiPath), emojiImage);
            }

            return returnCache;
        }

        public static async Task<List<CheerEmote>> GetBits(List<Comment> comments, string cacheFolder, string channel_id = "", EmbeddedData embeddedData = null, bool offline = false, CancellationToken cancellationToken = default)
        {
            List<CheerEmote> returnList = new List<CheerEmote>();
            List<string> alreadyAdded = new List<string>();

            // Load our embedded data from file
            if (embeddedData?.twitchBits != null)
            {
                foreach (EmbedCheerEmote data in embeddedData.twitchBits)
                {
                    List<KeyValuePair<int, TwitchEmote>> tierList = new List<KeyValuePair<int, TwitchEmote>>();
                    CheerEmote newEmote = new CheerEmote() { prefix = data.prefix, tierList = tierList };
                    foreach (KeyValuePair<int, EmbedEmoteData> tier in data.tierList)
                    {
                        TwitchEmote tierEmote = new TwitchEmote(tier.Value.data, EmoteProvider.TwitchFirstParty, tier.Value.imageScale, tier.Value.id, tier.Value.name);
                        tierList.Add(new KeyValuePair<int, TwitchEmote>(tier.Key, tierEmote));
                    }
                    returnList.Add(newEmote);
                    alreadyAdded.Add(data.prefix);
                }
            }

            // Directly return if we are in offline, no need for a network request
            if (offline)
            {
                return returnList;
            }

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri("https://gql.twitch.tv/gql"),
                Method = HttpMethod.Post,
                Content = new StringContent("{\"query\":\"query{cheerConfig{groups{nodes{id, prefix, tiers{bits}}, templateURL}},user(id:\\\"" + channel_id + "\\\"){cheer{cheerGroups{nodes{id,prefix,tiers{bits}},templateURL}}}}\",\"variables\":{}}", Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Client-ID", "kimne78kx3ncx6brgo4mv6wki5h1ko");
            using var cheerResponseMessage = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            cheerResponseMessage.EnsureSuccessStatusCode();
            var cheerResponse = await cheerResponseMessage.Content.ReadFromJsonAsync<GqlCheerResponse>(cancellationToken: cancellationToken);

            string bitFolder = Path.Combine(cacheFolder, "bits");
            if (!Directory.Exists(bitFolder))
                PlatformHelper.CreateDirectory(bitFolder);

            if (cheerResponse?.data != null)
            {
                List<CheerGroup> groupList = new List<CheerGroup>();

                foreach (CheerGroup group in cheerResponse.data.cheerConfig.groups)
                {
                    groupList.Add(group);
                }

                if (cheerResponse.data.user?.cheer?.cheerGroups != null)
                {
                    foreach (var group in cheerResponse.data.user.cheer.cheerGroups)
                    {
                        groupList.Add(group);
                    }
                }

                foreach (CheerGroup cheerGroup in groupList)
                {
                    string templateURL = cheerGroup.templateURL;

                    var cheerNodesQuery = from node in cheerGroup.nodes
                                          where !alreadyAdded.Contains(node.prefix)
                                          let pattern = $@"(?<=^|\s){Regex.Escape(node.prefix)}(?=[1-9])"
                                          where comments
                                              .Where(comment => comment.message.bits_spent > 0)
                                              .Any(comment => Regex.IsMatch(comment.message.body, pattern))
                                          select node;

                    foreach (CheerNode node in cheerNodesQuery)
                    {
                        string prefix = node.prefix;
                        try
                        {
                            List<KeyValuePair<int, TwitchEmote>> tierList = new List<KeyValuePair<int, TwitchEmote>>();
                            CheerEmote newEmote = new CheerEmote() { prefix = prefix, tierList = tierList };
                            foreach (Tier tier in node.tiers)
                            {
                                int minBits = tier.bits;
                                string url = templateURL.Replace("PREFIX", node.prefix.ToLower()).Replace("BACKGROUND", "dark").Replace("ANIMATION", "animated").Replace("TIER", tier.bits.ToString()).Replace("SCALE.EXTENSION", "2.gif");
                                TwitchEmote emote = new TwitchEmote(await PlatformHelper.GetImage(bitFolder, url, node.id + tier.bits, "2", "gif", cancellationToken), EmoteProvider.TwitchFirstParty, 2, prefix + minBits, prefix + minBits);
                                tierList.Add(new KeyValuePair<int, TwitchEmote>(minBits, emote));
                            }
                            returnList.Add(newEmote);
                        }
                        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound) { }
                    }
                }
            }

            return returnList;
        }

        /// <summary>
        /// Cleans up any unmanaged cache files from previous runs that were interrupted before cleaning up
        /// </summary>
        public static void CleanupUnmanagedCacheFiles(string cacheFolder, IProgress<ProgressReport> progress)
        {
            if (!Directory.Exists(cacheFolder))
            {
                return;
            }

            // Let's delete any video download cache folders older than 24 hours
            var videoFolderRegex = new Regex(@"\d+_(\d+)$", RegexOptions.RightToLeft); // Matches "...###_###" and captures the 2nd ###
            var directories = Directory.GetDirectories(cacheFolder);
            var directoriesDeleted = (from directory in directories
                                      let videoFolderMatch = videoFolderRegex.Match(directory)
                                      where videoFolderMatch.Success
                                      where DeleteOldDirectory(directory, videoFolderMatch.Groups[1].ValueSpan)
                                      select directory).Count();

            if (directoriesDeleted > 0)
            {
                progress.Report(new ProgressReport(ReportType.Log, $"{directoriesDeleted} old video caches were deleted."));
            }
        }

        private static bool DeleteOldDirectory(string directory, ReadOnlySpan<char> directoryCreationMillis)
        {
            var downloadTime = long.Parse(directoryCreationMillis);
            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            const int TWENTY_FOUR_HOURS_MILLIS = 86_400_000;
            if (currentTime - downloadTime > TWENTY_FOUR_HOURS_MILLIS)
            {
                try
                {
                    Directory.Delete(directory, true);
                    return true;
                }
                catch { /* Eat the exception */ }
            }
            return false;
        }

        private static bool DeleteColdDirectory(string directory)
        {
            // Directory.GetLastWriteTimeUtc() works as expected on both Windows and MacOS. Assuming it does on Linux too
            var directoryWriteTimeMillis = Directory.GetLastWriteTimeUtc(directory).Ticks / TimeSpan.TicksPerMillisecond;
            var currentTimeMillis = DateTimeOffset.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;

            const int SIX_HOURS_MILLIS = 21_600_000;
            if (currentTimeMillis - directoryWriteTimeMillis > SIX_HOURS_MILLIS)
            {
                try
                {
                    Directory.Delete(directory, true);
                    return true;
                }
                catch { /* Eat the exception */ }
            }
            return false;
        }

        public static int TimestampToSeconds(string input)
        {
            // Gets total seconds from timestamp in the format of 0h0m0s
            input = input.Replace('h', ':').Replace('m', ':').Replace("s", "");
            TimeSpan returnSpan = TimeSpan.Parse(input);

            return (int)returnSpan.TotalSeconds;
        }

        public static async Task<string> GetStreamerName(int id)
        {
            try
            {
                GqlUserInfoResponse info = await GetUserInfo(new List<string> { id.ToString() });
                return info.data.users[0].login;
            }
            catch { return ""; }
        }

        public static async Task<GqlUserInfoResponse> GetUserInfo(List<string> idList)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri("https://gql.twitch.tv/gql"),
                Method = HttpMethod.Post,
                Content = new StringContent("{\"query\":\"query{users(ids:[" + string.Join(",", idList.Select(x => "\\\"" + x + "\\\"").ToArray()) + "]){id,login,createdAt,updatedAt,description,profileImageURL(width:300)}}\",\"variables\":{}}", Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Client-ID", "kimne78kx3ncx6brgo4mv6wki5h1ko");
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<GqlUserInfoResponse>();
        }

        public static async Task<GqlVideoChapterResponse> GetVideoChapters(int videoId)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri("https://gql.twitch.tv/gql"),
                Method = HttpMethod.Post,
                Content = new StringContent("{\"extensions\":{\"persistedQuery\":{\"sha256Hash\":\"8d2793384aac3773beab5e59bd5d6f585aedb923d292800119e03d40cd0f9b41\",\"version\":1}},\"operationName\":\"VideoPlayer_ChapterSelectButtonVideo\",\"variables\":{\"videoID\":\"" + videoId + "\"}}", Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Client-ID", "kimne78kx3ncx6brgo4mv6wki5h1ko");
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<GqlVideoChapterResponse>();
        }
    }

}