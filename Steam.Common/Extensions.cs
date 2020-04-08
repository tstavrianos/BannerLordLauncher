using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Steam.Common
{
    public static class Extensions
    {
        public static bool ExpiredSince(this DateTime dateTime, int minutes)
        {
            return (dateTime - DateTime.Now).TotalMinutes < minutes;
        }
        
        public static string ToJson(this object obj, bool indented = true) {
            return JsonConvert.SerializeObject(obj, (indented ? Formatting.Indented : Formatting.None), new StringEnumConverter());
        }
    }
}