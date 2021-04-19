using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Xunit.Priority;
using XUnitPriorityOrderer;

// set to be sequential execution
[assembly: CollectionBehavior(DisableTestParallelization = true)]
// addset the custom test's collection orderer
[assembly: TestCollectionOrderer(CollectionPriorityOrderer.TypeName, CollectionPriorityOrderer.AssembyName)]
namespace Web.IntegrationTest
{
    [TestCaseOrderer(CasePriorityOrderer.TypeName, CasePriorityOrderer.AssembyName)]
    public class BasicTest
    {
        private static readonly string baseUrl = "http://localhost:19407";
        private readonly HttpClient _client;
        private readonly CookieContainer _cookies = new CookieContainer();

        public BasicTest()
        {
            var handler = new HttpClientHandler();
            handler.UseCookies = true;
            handler.CookieContainer = _cookies;
            _client = new HttpClient(handler);
        }

        [Theory]
        [InlineData("/Login")]
        [InlineData("/")]
        public async Task TestEndPoint(string url)
        {
            // Act
            var response = await _client.GetAsync(baseUrl + url);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }

        [Fact, Order(0)]
        public async Task TestLogin()
        {
            var data = JsonSerializer.Serialize(new { usuario = "ADMIN@GMAIL.COM", clave = "123456" });
            var content = new StringContent(data, Encoding.UTF8, "application/json");
            // Act
            var response = await _client.PostAsync(baseUrl + "/Login/Autenticar", content);

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Comun.ResponseModel>(json);


            Assert.True(result.response);
        }

        [Theory,Order(1)]
        [InlineData("/Notas")]
        public async Task TestNotasAutorization(string url)
        {
            // Act
            var response = await _client.GetAsync(baseUrl + url);
            
            // Assert
            //response.EnsureSuccessStatusCode();
            Assert.Equal($"{baseUrl}/Login", response.RequestMessage.RequestUri.ToString());
        }

    }
}
