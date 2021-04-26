using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Web.IntegrationTest
{

    public class AdminTests
    {
        private static readonly string baseUrl = "http://localhost:19407";
        public HttpClient client;
        public HttpClientHandler handler;
        public CookieContainer cookies;

        [OneTimeSetUp]
        public void Setup()
        {
            handler = new HttpClientHandler();
            cookies = new CookieContainer();
            handler.UseCookies = true;
            handler.CookieContainer = this.cookies;
            client = new HttpClient(handler);
        }

        [OneTimeTearDown]
        public void CleanUp()
        {
            client.Dispose();
        }

        [Test, Order(0)]
        public async Task TestLoginView()
        {
            // Act
            var response = await client.GetAsync(baseUrl + "/Login");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.AreEqual("text/html; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }

        [Test, Order(1)]
        public async Task TestAuthentication()
        {
            var data = JsonSerializer.Serialize(new { usuario = "ADMIN@GMAIL.COM", clave = "123456" });
            var content = new StringContent(data, Encoding.UTF8, "application/json");
            // Act
            var response = await client.PostAsync(baseUrl + "/Login/Autenticar", content);

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Comun.ResponseModel>(json);

            var uri = new Uri("http://localhost");
            var responseCookies = cookies.GetCookies(uri);

            cookies.Add(responseCookies);

            Assert.True(result.response);
        }

        [Test, Order(2)]
        [TestCase("/Alumno/Tabla")]
        [TestCase("/Personal/Tabla")]
        [TestCase("/Especialidad/Tabla")]
        [TestCase("/Curso/Tabla")]
        [TestCase("/Periodo/Tabla")]
        [TestCase("/Aula/Tabla")]
        [TestCase("/Usuario/Tabla")]
        [TestCase("/Horario/Tabla")]
        [TestCase("/Notas/Tabla")]
        public async Task TestTablas(string url)
        {
            // Act
            var response = await client.GetAsync(baseUrl + url);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }

        [Test, Order(2)]
        [TestCase("/Alumno/Guardar", "{\"idAlumno\":0,\"EspecialidadesId\":[1],\"Dni\":\"0000\",\"Paterno\":\"Test\",\"Materno\":\"Test\",\"Nombres\":\"Test\",\"Codigo\":\"0000\",\"Nacimiento\":null,\"Direccion\":null,\"Estado\":true}")]
        [TestCase("/Personal/Guardar", "{\"idPersonal\":0,\"TiposPersonalId\":[1],\"Dni\":\"0000\",\"Paterno\":\"Test\",\"Materno\":\"Test\",\"Nombres\":\"Test\",\"Correo\":null,\"Celular\":null,\"Nacimiento\":null,\"Direccion\":null,\"Honorario\":1500,\"Estado\":true}")]
        [TestCase("/Especialidad/Guardar", "{\"Id\":0,\"Denominacion\":\"Test\",\"Matricula\":1500,\"Mensualidad\":100,\"Cuotas\":12,\"Duracion\":\"Test\",\"TotalHoras\":6,\"Estado\":true}")]
        [TestCase("/Curso/Guardar", "{\"Id\":0,\"EspecialidadId\":1,\"Codigo\":\"0000\",\"Denominacion\":\"Test\",\"Credito\":4,\"Matricula\":80,\"Mensualidad\":50,\"Cuotas\":4,\"Duracion\":\"Test\",\"TotalHoras\":null,\"HorasTeoria\":null,\"HorasPractica\":null,\"Ciclo\":null,\"ReqCurso\":\"Test\",\"ReqCredito\":null,\"Estado\":true}")]
        [TestCase("/Periodo/Guardar", "{\"idPeriodo\":0,\"Denominacion\":\"Test\",\"FechaInicio\":\"2021-02-24\",\"FechaFin\":\"2021-04-24\",\"Estado\":true}")]
        [TestCase("/Aula/Guardar", "{\"Id\":0,\"Denominacion\":\"Test\",\"Estado\":true}")]
        [TestCase("/Usuario/Guardar", "{\"Id\":0,\"RolId\":1,\"PersonalId\":1,\"Nombre\":\"0000\",\"Correo\":\"0000@gmail.com\",\"Clave\":\"Test\",\"IndCambio\":true,\"IndUso\":true,\"activo\":true}")]
        [TestCase("/Horario/Guardar", "{\"Id\":0,\"PeriodoId\":1,\"CursoId\":1,\"AulaId\":1,\"HoraInicio\":\"2021-02-02\",\"HoraFin\":\"2021-04-04\",\"Dias\":\"Lunes,Martes\",\"DocenteId\":1}")]
        [TestCase("/Notas/Guardar", "{\"alumno_id\":1,\"curso_id\":1,\"notra\":15,\"Observacion\":\"Test\"}")]
        public async Task TestMantener(string url, string json)
        {
            var rng = new Random();
            var cod = rng.Next(10000000, 99999999);
            json = json.Replace("0000", cod.ToString());
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            // Act
            var response = await client.PostAsync(baseUrl + url, content);

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Comun.ResponseModel>(jsonResponse);

            Assert.False(result.isException);
        }

        [Test, Order(2)]
        [TestCase("/Reportes/ReportesAlumno")]
        [TestCase("/Reportes/ReportesPersonal")]
        [TestCase("/Reportes/ReportesMatricula?curso_id=1")]
        [TestCase("/Reportes/FichaMatricula?id=1")]
        public async Task TestReportes(string url)
        {
            // Act
            var response = await client.GetAsync(baseUrl + url);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.AreEqual("application/pdf", response.Content.Headers.ContentType.ToString());
        }
    }

    public class SecretaryTests
    {
        private static readonly string baseUrl = "http://localhost:19407";
        public HttpClient client;
        public HttpClientHandler handler;
        public CookieContainer cookies;

        [OneTimeSetUp]
        public void Setup()
        {
            handler = new HttpClientHandler();
            cookies = new CookieContainer();
            handler.UseCookies = true;
            handler.CookieContainer = this.cookies;
            client = new HttpClient(handler);
        }

        [OneTimeTearDown]
        public void CleanUp()
        {
            client.Dispose();
        }

        [Test, Order(0)]
        public async Task TestLoginView()
        {
            // Act
            var response = await client.GetAsync(baseUrl + "/Login");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.AreEqual("text/html; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }

        [Test, Order(1)]
        public async Task TestAuthentication()
        {
            var data = JsonSerializer.Serialize(new { usuario = "EDITH@GMAIL.COM", clave = "123456" });
            var content = new StringContent(data, Encoding.UTF8, "application/json");
            // Act
            var response = await client.PostAsync(baseUrl + "/Login/Autenticar", content);

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Comun.ResponseModel>(json);

            var uri = new Uri("http://localhost");
            var responseCookies = cookies.GetCookies(uri);

            cookies.Add(responseCookies);

            Assert.True(result.response);
        }

        [Test, Order(2)]
        [TestCase("/Matricula/Tabla")]
        [TestCase("/CuentasPorCobrar/Tabla")]
        [TestCase("/Pagos/TablaCobranzas")]
        [TestCase("/Pagos/TablaPagos")]
        [TestCase("/Pagos/TablaEntradas")]
        [TestCase("/Pagos/TablaSalidas")]
        public async Task TestTablas(string url)
        {
            // Act
            var response = await client.GetAsync(baseUrl + url);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }

        [Test,Order(2)]
        public async Task TestMatricula()
        {
            var rawData = new {CondicionEstudioId=1,PeriodoId=1, IndPagoUnico=true, AlumnoId=1, EspecialidadId=1, Monto=300, Observacion="TEST", MatriculaDetalle = new List<dynamic>() { new { CursoId=1 }, new { CursoId=2 } } };
            var data = JsonSerializer.Serialize(rawData);
            var content = new StringContent(data, Encoding.UTF8, "application/json");
            // Act
            var response = await client.PostAsync(baseUrl + "/Matricula/Registrar", content);

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Comun.ResponseModel>(json);

            var uri = new Uri("http://localhost");
            var responseCookies = cookies.GetCookies(uri);

            cookies.Add(responseCookies);

            Assert.False(result.isException);
        }

        [Test, Order(2)]
        public async Task TestCuentasPorCobrar()
        {
            var rawData = new { AlumnoId = 1, Fecha = "", Total = 100, Descripcion = "TEST", CuentasPorCobrarDetalle = new List<dynamic>() { new { ConceptoPagoId = 1, ItemId=1 }, new { ConceptoPagoId = 2, ItemId = 1 } } };
            var data = JsonSerializer.Serialize(rawData);
            var content = new StringContent(data, Encoding.UTF8, "application/json");
            // Act
            var response = await client.PostAsync(baseUrl + "/CuentasPorCobrar/Guardar", content);

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Comun.ResponseModel>(json);

            var uri = new Uri("http://localhost");
            var responseCookies = cookies.GetCookies(uri);

            cookies.Add(responseCookies);

            Assert.False(result.isException);

        }

        [Test, Order(2)]
        public async Task TestPagos()
        {
            var rawData = new { CuentaPorCobrarId = 2, TipoComprobante = "BL" };
            var data = JsonSerializer.Serialize(rawData);
            var content = new StringContent(data, Encoding.UTF8, "application/json");
            // Act
            var response = await client.PostAsync(baseUrl + "/Pagos/GuardarCobro", content);

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Comun.ResponseModel>(json);

            var uri = new Uri("http://localhost");
            var responseCookies = cookies.GetCookies(uri);

            cookies.Add(responseCookies);

            Assert.False(result.isException);

        }

        [Test, Order(2)]
        public async Task TestTransferencia()
        {
            var rawData = new { OperacionId = 6, OperacionDenominacion = "", Importe = 100, Descripcion = "TEST"};
            var data = JsonSerializer.Serialize(rawData);
            var content = new StringContent(data, Encoding.UTF8, "application/json");
            // Act
            var response = await client.PostAsync(baseUrl + "/Pagos/TransferirSaldos", content);

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Comun.ResponseModel>(json);

            var uri = new Uri("http://localhost");
            var responseCookies = cookies.GetCookies(uri);

            cookies.Add(responseCookies);

            Assert.False(result.isException);

        }

        [Test, Order(2)]
        public async Task TestIngresosEgresos()
        {
            var rawData = new { PersonalFiltro = "", OperacionId = 6, OperacionDenominacion = "PAGO DE SERVICIOS", PersonalId = 1, Total=100, Descripcion="TEST" };
            var data = JsonSerializer.Serialize(rawData);
            var content = new StringContent(data, Encoding.UTF8, "application/json");
            // Act
            var response = await client.PostAsync(baseUrl + "/Pagos/EgresosIngresos", content);

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Comun.ResponseModel>(json);

            var uri = new Uri("http://localhost");
            var responseCookies = cookies.GetCookies(uri);

            cookies.Add(responseCookies);

            Assert.False(result.isException);

        }


        [Test, Order(2)]
        [TestCase("/Reportes/ReportesIngreso")]
        [TestCase("/Reportes/ReportesEgreso")]
        public async Task TestReportes(string url)
        {
            // Act
            var response = await client.GetAsync(baseUrl + url);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.AreEqual("application/pdf", response.Content.Headers.ContentType.ToString());
        }
    }
}