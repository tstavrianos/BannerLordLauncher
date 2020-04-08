using System;
using System.Collections.Generic;

namespace BannerLord.Common.Extensions
{
    public static class StringExtensions
    {
        internal static IEnumerable<string> GetLines(this string text)
        {
            var newLine = text.IndexOf("\r", StringComparison.Ordinal) > -1 ? "\r\n" : "\n";
            return text.Split(new[] { newLine }, StringSplitOptions.None);
        }

    }
}