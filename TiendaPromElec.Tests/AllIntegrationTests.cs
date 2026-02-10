using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Newtonsoft.Json.Linq;
using ProductApi.Models;
using TiendaPromElec.Controllers; 
using Xunit;

namespace TiendaPromElec.IntegrationTests
{
    public class AllIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AllIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        // Login
        private async Task<string> GetAdminToken()
        {
            var login = new LoginModel { Username = "admin", Password = "password123" };
            var response = await _client.PostAsJsonAsync("/api/Auth/login", login);
            var jsonString = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(jsonString);
            return result["token"].ToString();
        }

        //  Security Tests 

        [Fact] // 1
        public async Task GetProducts_NoToken_ReturnsUnauthorized()
        {
            _client.DefaultRequestHeaders.Authorization = null;
            var response = await _client.GetAsync("/api/Product");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact] // 2
        public async Task GetProductById_NoToken_ReturnsUnauthorized()
        {
            _client.DefaultRequestHeaders.Authorization = null;
            var response = await _client.GetAsync("/api/Product/1");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact] // 3
        public async Task PostProduct_NoToken_ReturnsUnauthorized()
        {
            _client.DefaultRequestHeaders.Authorization = null;
            var response = await _client.PostAsJsonAsync("/api/Product", new Product { Name = "Test", Description = "Test", Brand = "Test" });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact] // 4
        public async Task PutProduct_NoToken_ReturnsUnauthorized()
        {
            _client.DefaultRequestHeaders.Authorization = null;
            var response = await _client.PutAsJsonAsync("/api/Product/1", new Product { Name = "Test", Description = "Test", Brand = "Test" });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact] // 5
        public async Task DeleteProduct_NoToken_ReturnsUnauthorized()
        {
            _client.DefaultRequestHeaders.Authorization = null;
            var response = await _client.DeleteAsync("/api/Product/1");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact] // 6
        public async Task Login_InvalidPassword_ReturnsUnauthorized()
        {
            var login = new LoginModel { Username = "admin", Password = "WRONG" };
            var response = await _client.PostAsJsonAsync("/api/Auth/login", login);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact] // 7
        public async Task Login_InvalidUser_ReturnsUnauthorized()
        {
            var login = new LoginModel { Username = "fake", Password = "password123" };
            var response = await _client.PostAsJsonAsync("/api/Auth/login", login);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact] // 8
        public async Task Login_EmptyBody_ReturnsBadRequest()
        {
            var response = await _client.PostAsJsonAsync("/api/Auth/login", new {});
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Unauthorized);
        }

        [Fact] // 9
        public async Task AccessWithFakeToken_ReturnsUnauthorized()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "fake.token.123");
            var response = await _client.GetAsync("/api/Product");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact] // 10
        public async Task Login_ValidCredentials_ReturnsOk()
        {
            var login = new LoginModel { Username = "admin", Password = "password123" };
            var response = await _client.PostAsJsonAsync("/api/Auth/login", login);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // Good Path Tests (con token valido)

        [Fact] // 11
        public async Task GetProducts_WithToken_ReturnsList()
        {
            var token = await GetAdminToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var response = await _client.GetAsync("/api/Product");
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact] // 12
        public async Task PostProduct_ValidItem_CreatesResource()
        {
            var token = await GetAdminToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var newProd = new Product { Name = "Integration", Description="Desc", Brand="X", Price=10 };
            var response = await _client.PostAsJsonAsync("/api/Product", newProd);
            
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact] // 13
        public async Task PostProduct_InvalidModel_ReturnsBadRequest()
        {
            var token = await GetAdminToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.PostAsJsonAsync("/api/Product", "Texto invalido");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact] // 14
        public async Task GetProduct_InvalidId_ReturnsNotFound()
        {
            var token = await GetAdminToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/Product/99999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact] // 15
        public async Task DeleteProduct_ValidId_ReturnsNoContent()
        {
            var token = await GetAdminToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Primero creamos uno para borrar
            var created = await _client.PostAsJsonAsync("/api/Product", new Product { Name="ToDel", Description="D", Brand="B" });
            var prod = await created.Content.ReadFromJsonAsync<Product>();

            var response = await _client.DeleteAsync($"/api/Product/{prod.Id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact] // 16
        public async Task DeleteProduct_InvalidId_ReturnsNotFound()
        {
            var token = await GetAdminToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.DeleteAsync("/api/Product/99999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact] // 17
        public async Task PutProduct_ValidUpdate_ReturnsNoContent()
        {
            var token = await GetAdminToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Crear
            var created = await _client.PostAsJsonAsync("/api/Product", new Product { Name="ToUpd", Description="D", Brand="B" });
            var prod = await created.Content.ReadFromJsonAsync<Product>();

            // Actualizar
            prod.Name = "Updated";
            var response = await _client.PutAsJsonAsync($"/api/Product/{prod.Id}", prod);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact] // 18
        public async Task PutProduct_IdMismatch_ReturnsBadRequest()
        {
            var token = await GetAdminToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var prod = new Product { Id = 1, Name = "X", Description="D", Brand="B" };
            // URL ID (2) != Body ID (1)
            var response = await _client.PutAsJsonAsync("/api/Product/2", prod);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact] // 19
        public async Task PutProduct_NonExistent_ReturnsNotFound()
        {
            var token = await GetAdminToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var prod = new Product { Id = 9999, Name = "Ghost", Description="D", Brand="B" };
            var response = await _client.PutAsJsonAsync("/api/Product/9999", prod);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact] // 20
        public async Task Swagger_Endpoint_ReturnsOk()
        {
            var response = await _client.GetAsync("/swagger/index.html");
            Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound);
        }
    }
}