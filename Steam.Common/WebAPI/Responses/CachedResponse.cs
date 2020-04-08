using System;
using Newtonsoft.Json;

namespace Steam.Common.WebAPI.Responses
{
    public sealed class CachedResponse<T>
    {
        [JsonProperty("lastFetched")]
        public DateTime LastFetched { get; set; }
        [JsonProperty("response")]
        public T Response { get; private set; }

        public static CachedResponse<T> FromResponse(T response)
        {
            return new CachedResponse<T>{LastFetched = DateTime.Now, Response = response};
        }

    }
}