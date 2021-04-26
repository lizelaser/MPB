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
    public class BasicTest: IClassFixture<ClientFixture>
    {
        private static readonly string baseUrl = "http://localhost:19407";
        private readonly ClientFixture fixture;

        public BasicTest(ClientFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact, Order(0)]
        public async Task TestLoginView()
        {
            // Act
            var response = await fixture.client.GetAsync(baseUrl + "/Login");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }

        [Fact, Order(1)]
        public async Task TestAuthentication()
        {
            var data = JsonSerializer.Serialize(new { usuario = "ADMIN@GMAIL.COM", clave = "123456" });
            var content = new StringContent(data, Encoding.UTF8, "application/json");
            // Act
            var response = await fixture.client.PostAsync(baseUrl + "/Login/Autenticar", content);

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Comun.ResponseModel>(json);

            var uri = new Uri("http://localhost");
            var responseCookies = fixture.cookies.GetCookies(uri);

            fixture.cookies.Add(responseCookies);

            Assert.True(result.response);
        }

        [Theory]
        [InlineData("/Alumno/Tabla")]
        [InlineData("/Personal/Tabla")]
        [InlineData("/CuentasPorCobrar/Tabla")]
        [InlineData("/Matricula/Tabla")]
        [InlineData("/Especialidad/Tabla")]
        [InlineData("/Curso/Tabla")]
        [InlineData("/Periodo/Tabla")]
        [InlineData("/Aula/Tabla")]
        [InlineData("/Usuario/Tabla")]
        [InlineData("/Horario/Tabla")]
        [InlineData("/Notas/Tabla")]
        public async Task TestTablas(string url)
        {
            // Act
            var response = await fixture.client.GetAsync(baseUrl + url);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }

    }

    public class ClientFixture : IDisposable
    {
        public HttpClient client;
        public HttpClientHandler handler;
        public CookieContainer cookies;

        public ClientFixture()
        {
            handler= new HttpClientHandler();
            cookies = new CookieContainer();
            handler.UseCookies = true;
            handler.CookieContainer = this.cookies;
            client = new HttpClient(handler);
        }
        public void Dispose()
        {
            client.Dispose();
        }
    }
}
