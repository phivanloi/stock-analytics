using Ardalis.GuardClauses;
using Polly;
using Polly.Retry;
using System.Text.Json;

namespace Pl.Sas.Infrastructure.Helper
{
    public static class AsyncHttpHelper
    {
        private static readonly AsyncRetryPolicy _httpAsyncRetry = Policy.Handle<Exception>()
            .WaitAndRetryAsync(10, (_) => TimeSpan.FromMilliseconds(500));

        public static async Task<T?> GetJsonAsync<T>(this HttpClient httpClient, string url)
        {
            Guard.Against.NullOrEmpty(url, nameof(url));
            var httpResponseMessage = await _httpAsyncRetry.ExecuteAsync(async () =>
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

        public static async Task<string> GetJsonAsync(this HttpClient httpClient, string url)
        {
            Guard.Against.NullOrEmpty(url, nameof(url));
            var httpResponseMessage = await _httpAsyncRetry.ExecuteAsync(async () =>
            {
                return await httpClient.GetAsync(url);
            });
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return await httpResponseMessage.Content.ReadAsStringAsync();
            }
            return null!;
        }

        public static async Task<T?> PostJsonAsync<T>(this HttpClient httpClient, string url, StringContent stringContent)
        {
            Guard.Against.NullOrEmpty(url, nameof(url));
            var httpResponseMessage = await _httpAsyncRetry.ExecuteAsync(async () =>
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
            var httpResponseMessage = await _httpAsyncRetry.ExecuteAsync(async () =>
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
            var httpResponseMessage = await _httpAsyncRetry.ExecuteAsync(async () =>
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
