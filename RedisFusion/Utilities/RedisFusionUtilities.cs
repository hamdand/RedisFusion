using Microsoft.AspNetCore.Http;
using System.Text;

namespace RedisFusion.Utilities
{
    /// <summary>
    /// Provides utility methods for RedisFusion.
    /// </summary>
    internal static class RedisFusionUtilities
    {
        /// <summary>
        /// Generates a cache key from the given HTTP request.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>A cache key string.</returns>
        internal static string GenerateCacheKeyFromRequest(HttpRequest request)
        {
            var keyBuilder = new StringBuilder();
            keyBuilder.Append($"{request.Path}");

            foreach (var (key, value) in request.Query.OrderBy(x => x.Key))
            {
                keyBuilder.Append($"|{key}-{value}");
            }

            return keyBuilder.ToString();
        }

        /// <summary>
        /// Parses the Accept-Language header to retrieve the first valid language.
        /// </summary>
        /// <param name="acceptLanguageHeader">The Accept-Language header value.</param>
        /// <returns>The first valid language tag, or an empty string if none is valid.</returns>
        internal static string ParseSingleLanguage(string acceptLanguageHeader, List<string> acceptableLanguages)
        {
            if (!string.IsNullOrEmpty(acceptLanguageHeader))
            {
                var languageTokens = acceptLanguageHeader.Split(',').Select(token => token.Trim());
                var firstLanguage = languageTokens.FirstOrDefault();
                if (acceptableLanguages.Contains(firstLanguage, StringComparer.OrdinalIgnoreCase))
                {
                    return firstLanguage;
                }
            }
            return string.Empty;
        }

    }
}
