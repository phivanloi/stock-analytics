using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pl.Sas.UnitTests
{
    public class LoggerTests
    {
        [Fact]
        public async Task WriteLogTestAsync()
        {
            var postObject = new
            {
                Host = "TestHost",
                Message = "Test message",
                FullMessage = "Test Full message",
                Type = 1
            };
            var _httpClient = new HttpClient()
            {
                BaseAddress = new Uri("http://log.vuigreens.com"),
            };
            _httpClient.DefaultRequestHeaders.Add("SecurityKey", "CFBF2CB13C19325684FD");
            var response = await _httpClient.PostAsync($"write", new StringContent(JsonSerializer.Serialize(postObject), Encoding.UTF8, "application/json"), CancellationToken.None);
            Assert.True(response.IsSuccessStatusCode);
        }
    }
}