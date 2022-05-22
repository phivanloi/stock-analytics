using Ardalis.GuardClauses;
using Polly;
using Polly.Retry;
using System.Text.Json;

namespace Pl.Sas.Infrastructure.Helper
{
    public static class HttpHelper
    {
        private static readonly RetryPolicy _httpRetryPolicy = Policy.Handle<Exception>().WaitAndRetry(retryCount: 5, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(300));

        public static async Task<T?> GetJsonAsync<T>(this HttpClient httpClient, string url)
        {
            Guard.Against.NullOrEmpty(url, nameof(url));
            var httpResponseMessage = await _httpRetryPolicy.Execute(async () =>
            {
                return await httpClient.GetAsync(url);
            });
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var jsonContent = await httpResponseMessage.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(jsonContent))
                {
                    return JsonSerializer.Deserialize<T>(jsonContent);
                }
            }
            return default;
        }

        public static async Task<T?> PostJsonAsync<T>(this HttpClient httpClient, string url, StringContent stringContent)
        {
            Guard.Against.NullOrEmpty(url, nameof(url));
            var httpResponseMessage = await _httpRetryPolicy.Execute(async () =>
            {
                return await httpClient.PostAsync(url, stringContent);
            });

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var jsonContent = await httpResponseMessage.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(jsonContent))
                {
                    return JsonSerializer.Deserialize<T>(jsonContent);
                }
            }
            return default;
        }

        public static async Task<T?> PutJsonAsync<T>(this HttpClient httpClient, string url, StringContent stringContent)
        {
            Guard.Against.NullOrEmpty(url, nameof(url));
            var httpResponseMessage = await _httpRetryPolicy.Execute(async () =>
            {
                return await httpClient.PutAsync(url, stringContent);
            });
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var jsonContent = await httpResponseMessage.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(jsonContent))
                {
                    return JsonSerializer.Deserialize<T>(jsonContent);
                }
            }
            return default;
        }

        public static async Task<T?> DeleteJsonAsync<T>(this HttpClient httpClient, string url)
        {
            Guard.Against.NullOrEmpty(url, nameof(url));
            var httpResponseMessage = await _httpRetryPolicy.Execute(async () =>
            {
                return await httpClient.DeleteAsync(url);
            });
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var jsonContent = await httpResponseMessage.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(jsonContent))
                {
                    return JsonSerializer.Deserialize<T>(jsonContent);
                }
            }
            return default;
        }
    }
}
