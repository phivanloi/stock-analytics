using Ardalis.GuardClauses;
using Pl.Sas.Core.Interfaces;
using Polly;
using Polly.Retry;
using System.Text.Json;

namespace Pl.Sas.Infrastructure.Helper
{
    public class HttpHelper : IHttpHelper
    {
        protected readonly RetryPolicy _httpRetryPolicy;
        private readonly HttpClient _httpClient;
        public HttpHelper(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("downloader");
            _httpRetryPolicy = Policy.Handle<Exception>().WaitAndRetry(retryCount: 5, sleepDurationProvider: _ => TimeSpan.FromMilliseconds(300));
        }

        public virtual async Task<T?> GetJsonAsync<T>(string url)
        {
            Guard.Against.NullOrEmpty(url, nameof(url));
            var responseData = await _httpRetryPolicy.Execute(async () =>
            {
                return await _httpClient.GetStringAsync(url);
            });

            if (!string.IsNullOrEmpty(responseData))
            {
                return JsonSerializer.Deserialize<T>(responseData);
            }
            return default;
        }

        public virtual async Task<T?> PostJsonAsync<T>(string url, StringContent stringContent)
        {
            Guard.Against.NullOrEmpty(url, nameof(url));
            var httpResponseMessage = await _httpRetryPolicy.Execute(async () =>
            {
                return await _httpClient.PostAsync(url, stringContent);
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

        public virtual async Task<T?> PutJsonAsync<T>(string url, StringContent stringContent)
        {
            Guard.Against.NullOrEmpty(url, nameof(url));
            var httpResponseMessage = await _httpRetryPolicy.Execute(async () =>
            {
                return await _httpClient.PutAsync(url, stringContent);
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

        public virtual async Task<T?> DeleteJsonAsync<T>(string url)
        {
            Guard.Against.NullOrEmpty(url, nameof(url));
            var httpResponseMessage = await _httpRetryPolicy.Execute(async () =>
            {
                return await _httpClient.DeleteAsync(url);
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
