using System;
using System.Collections.Generic;
using Alphaleonis.Win32.Filesystem;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Splat;
using Steam.Common.WebAPI.Responses;
using Steam.Common.WebAPI.Responses.ISteamRemoteStorage;

namespace Steam.Common.WebAPI.Requests
{
    public sealed class SteamRemoteStorage : ISteamRemoteStorage, IEnableLogger
    {
        private readonly HttpClient _httpClient;
        private readonly bool _cached;
        private List<CachedResponse<PublishedFileDetail>> _publishedFileDetailsCached;
        private readonly FileInfo _publishedFileDetailsCacheFile;

        public SteamRemoteStorage(HttpClient httpClient, bool cached = true, string publishedFileDetailsCacheFile = "steam.publishedFileDetails.cache.json")
        {
            this._httpClient = httpClient ?? new HttpClient();
            this._publishedFileDetailsCacheFile = new FileInfo(publishedFileDetailsCacheFile);
            this._cached = cached;
        }

        private void CheckCache<T>(ref List<CachedResponse<T>> property, FileSystemInfo file)
        {
            if (!(property is null)) return;
            if (file.Exists)
            {
                try
                {
                    property = JsonConvert.DeserializeObject<List<CachedResponse<T>>>(File.ReadAllText(file.FullName));
                }
                catch (Exception ex)
                {
                    this.Log().Error(ex, "[Steam] Unable to load cache");
                    property = new List<CachedResponse<T>>();
                }
            }
            else
            {
                property = new List<CachedResponse<T>>();
            }
        }

        public Task<GetPublishedFileDetailsResponse> GetPublishedFileDetailsAsync(string fileId)
        {
            return this.GetPublishedFileDetailsAsync(new[] { fileId });
        }

        public Task<GetPublishedFileDetailsResponse> GetPublishedFileDetailsAsync(ulong fileId)
        {
            return this.GetPublishedFileDetailsAsync(new[] { fileId });
        }

        public Task<GetPublishedFileDetailsResponse> GetPublishedFileDetailsAsync(IEnumerable<ulong> fileIds)
        {
            return this.GetPublishedFileDetailsAsync(fileIds.Select(x => $"{x}").ToString());
        }

        public async Task<GetPublishedFileDetailsResponse> GetPublishedFileDetailsAsync(IReadOnlyList<string> fileIds)
        {
            var parsedResponse = new GetPublishedFileDetailsResponse();
            if (fileIds.Count < 1) return parsedResponse;
            if (this._cached)
            {
                this.CheckCache(ref this._publishedFileDetailsCached, this._publishedFileDetailsCacheFile);
                if (this._publishedFileDetailsCacheFile.Exists &&
                    (!this._publishedFileDetailsCacheFile.LastWriteTime.ExpiredSince(10)))
                {
                    foreach (var item in fileIds.Select(fileId => this._publishedFileDetailsCached.FirstOrDefault(x =>
                        $"{x.Response.PublishedFileId}" == fileId)).Where(item => item != null))
                    {
                        parsedResponse.Response.PublishedFileDetails.Add(item.Response);
                    }

                    if (parsedResponse.Response.PublishedFileDetails.Count >= fileIds.Count)
                        return parsedResponse;
                }
            }

            var values = new Dictionary<string, string> { { "itemcount", fileIds.Count.ToString() } };
            for (var i = 0; i < fileIds.Count; i++)
            {
                values.Add($"publishedfileids[{i}]", fileIds[i]);
            }
            var content = new FormUrlEncodedContent(values);
            var url = new Uri("https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/");
            this.Log().Debug($"[Steam] POST to {url} with payload {content.ToJson(false)} and values {values.ToJson(false)}");
            var response = await this._httpClient.PostAsync(url, content).ConfigureAwait(false);
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                parsedResponse = JsonConvert.DeserializeObject<GetPublishedFileDetailsResponse>(responseString);
            }
            catch (Exception ex)
            {
                this.Log().Error(ex, $"[Steam] Could not deserialize response: {responseString}");
            }

            if (!this._cached) return parsedResponse;
            foreach (var item in parsedResponse.Response.PublishedFileDetails)
            {
                this._publishedFileDetailsCached.RemoveAll(x => x.Response.PublishedFileId == item.PublishedFileId);
                this._publishedFileDetailsCached.Add(CachedResponse<PublishedFileDetail>.FromResponse(item));
            }

            File.WriteAllText(this._publishedFileDetailsCacheFile.FullName,
                JsonConvert.SerializeObject(this._publishedFileDetailsCached));

            return parsedResponse;
        }
    }
}