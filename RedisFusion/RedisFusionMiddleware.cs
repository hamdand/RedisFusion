namespace RedisFusion
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using RedisFusion.Services;
    using RedisFusion.Utilities;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class RedisFusionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRedisFusionService _cache;

        public RedisFusionMiddleware(RequestDelegate next, IRedisFusionService cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint != null)
            {
                var cacheAttribute = endpoint.Metadata.GetMetadata<RedisFusionOutputCache>();
                if (cacheAttribute != null)
                {
                    var key = GenerateCacheKeyFromRequest(context.Request);
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

        private string GenerateCacheKeyFromRequest(HttpRequest request)
        {
            var keyBuilder = new StringBuilder();
            keyBuilder.Append($"{request.Path}");

            foreach (var (key, value) in request.Query.OrderBy(x => x.Key))
            {
                keyBuilder.Append($"|{key}-{value}");
            }

            return keyBuilder.ToString();
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
