using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Steam.Common.WebAPI.Responses.ISteamRemoteStorage
{
    public sealed class PublishedFileDetail
    {
        [JsonProperty("publishedfileid")]
        public ulong PublishedFileId { get; set; }
        [JsonProperty("result")]
        public uint Result { get; set; }
        [JsonProperty("creator")]
        public string Creator { get; set; }
        [JsonProperty("creator_app_id")]
        public uint CreatorAppId { get; set; }
        [JsonProperty("consumer_app_id")]
        public uint ConsumerAppId { get; set; }
        [JsonProperty("filename")]
        public string Filename { get; set; }
        [JsonProperty("file_size")]
        public uint FileSize { get; set; }
        [JsonProperty("file_url")]
        public string FileUrl { get; set; }
        [JsonProperty("hcontent_file")]
        public string HContentFile { get; set; }
        [JsonProperty("preview_url")]
        public string PreviewUrl { get; set; }
        [JsonProperty("hcontent_preview")]
        public string HContentPreview { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("time_created")]
        public DateTime TimeCreated { get; set; }
        [JsonProperty("time_updated")]
        public DateTime TimeUpdated { get; set; }
        [JsonProperty("visibility")]
        public PublishedFileVisibility Visibility { get; set; }
        [JsonProperty("banned")]
        public bool Banned { get; set; }
        [JsonProperty("ban_reason")]
        public string BanReason { get; set; }
        [JsonProperty("subscriptions")]
        public ulong  Subscriptions { get; set; }
        [JsonProperty("favorited")]
        public ulong  Favorited { get; set; }
        [JsonProperty("lifetime_subscriptions")]
        public ulong  LifetimeSubscriptions { get; set; }
        [JsonProperty("lifetime_favorited")]
        public ulong  LifetimeFavorited { get; set; }
        [JsonProperty("views")]
        public ulong  Views { get; set; }
        [JsonProperty("tags")]
        public List<PublishedFileDetailTag> Tags { get; set; }
    }
}