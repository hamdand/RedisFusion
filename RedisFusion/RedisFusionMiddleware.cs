namespace RedisFusion
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using RedisFusion.Services;
    using RedisFusion.Utilities;
    using System.IO;
    using System.Threading.Tasks;

    public class RedisFusionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRedisFusionService _cache;
        private readonly RedisFusionConfigurations _config;


        public RedisFusionMiddleware(RequestDelegate next, IRedisFusionService cache, IOptions<RedisFusionConfigurations> config)
        {
            _next = next;
            _cache = cache;
            _config = config.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();

            if (endpoint != null)
            {
                var cacheAttribute = endpoint.Metadata.GetMetadata<RedisFusionOutputCache>();

                if (cacheAttribute != null)
                {
                    // Parse the Accept-Language header
                    var acceptLanguageHeader = context.Request.Headers["Accept-Language"].ToString();

                    var selectedLanguage = RedisFusionUtilities.ParseSingleLanguage(acceptLanguageHeader, _config.AcceptableLanguages);

                    selectedLanguage = !string.IsNullOrEmpty(selectedLanguage) ? "/" + selectedLanguage : string.Empty;

                    var key = $"{RedisFusionUtilities.GenerateCacheKeyFromRequest(context.Request)}{selectedLanguage}";

                    var cachedResponse = await _cache.GetCachedObjectAsync<string>(key);

                    if (!string.IsNullOrEmpty(cachedResponse))
                    {
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(cachedResponse);
                        return;
                    }

                    var originalBodyStream = context.Response.Body;
                    using (var memoryStream = new MemoryStream())
                    {
                        context.Response.Body = memoryStream;
                        await _next(context);
                        context.Response.Body = originalBodyStream;

                        if (context.Response.StatusCode == 200)
                        {
                            memoryStream.Seek(0, SeekOrigin.Begin);
                            var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();
                            await _cache.SetCachedItemAsync<string>(key, responseBody, cacheAttribute.durationInSeconds);
                            memoryStream.Seek(0, SeekOrigin.Begin);
                            await memoryStream.CopyToAsync(originalBodyStream);
                        }
                    }
                    return;
                }
            }

            await _next(context);
        }
    }

    public static class RedisFusionMiddlewareExtensions
    {
        public static IApplicationBuilder UseRedisFusionOutputCache(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RedisFusionMiddleware>();
        }
    }
}
