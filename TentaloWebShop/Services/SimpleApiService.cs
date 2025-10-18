using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TentaloWebShop.Models;
using static System.Net.WebRequestMethods;

namespace TentaloWebShop.Services
{
    public interface IApiService
    {
        Task<T> GetAsync<T>(string path);
        Task<HttpResponseMessage> PostAsync(string path, object data);
        Task<HttpResponseMessage> PostAsync2(string path, object data);
        Task<HttpResponseMessage> PatchAsync(string path, object data);
    }

    public class SimpleApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _baseUrl;
        private readonly string _username;
        private readonly string _password;
        private readonly ApiSettings _apiSettings;
        public SimpleApiService(IConfiguration configuration, IOptions<ApiSettings> apiSettings)
        {
            _configuration = configuration;

            _baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "";
            _username = _configuration["ApiSettings:Username"] ?? "";
            _password = _configuration["ApiSettings:Password"] ?? "";
            _apiSettings = apiSettings.Value;
            // ✅ Crear un HttpClient NUEVO y configurarlo ANTES de usarlo
            _httpClient = new HttpClient();
            ConfigureHttpClient();
        }

        private void ConfigureHttpClient()
        {
            // ✅ Configurar TIMEOUT primero
            _httpClient.Timeout = TimeSpan.FromSeconds(30);

            // ✅ Configurar autenticación
            var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_username}:{_password}"));
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", authToken);

            // ✅ Agregar headers adicionales
            _httpClient.DefaultRequestHeaders.Add("Isolation", "snapshot");
        }

        public async Task<T> GetAsync<T>(string path)
        {
            try
            {
                var fullUrl = $"{_apiSettings.Url}{_apiSettings.Tenant}/{_apiSettings.Entorno}/api/{_apiSettings.APIPublisher}/{_apiSettings.APIGroup}/{_apiSettings.APIVersion}/companies({_apiSettings.Empresa})/{path}";
                Console.WriteLine($"API GET: {fullUrl}");

                var sw = System.Diagnostics.Stopwatch.StartNew();

                var response = await _httpClient.GetAsync(fullUrl);
                Console.WriteLine($"⏱️ HTTP response received in {sw.ElapsedMilliseconds}ms");

                response.EnsureSuccessStatusCode();

                if (typeof(T) == typeof(JsonDocument))
                {
                    Console.WriteLine($"📝 Starting JsonDocument.ParseAsync from stream...");
                    using var stream = await response.Content.ReadAsStreamAsync();
                    Console.WriteLine($"📝 Stream obtained in {sw.ElapsedMilliseconds}ms");

                    var doc = await JsonDocument.ParseAsync(stream);
                    Console.WriteLine($"✅ JsonDocument parsed in {sw.ElapsedMilliseconds}ms total");
                    return (T)(object)doc;
                }
                else
                {
                    Console.WriteLine($"📝 Deserializing {typeof(T).Name}...");
                    var result = await response.Content.ReadFromJsonAsync<T>();
                    Console.WriteLine($"✅ {typeof(T).Name} deserialized in {sw.ElapsedMilliseconds}ms total");
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en GET request to {path}: {ex.Message}");
                throw;
            }
        }

        public async Task<HttpResponseMessage> PostAsync(string path, object data)
        {
            try
            {
                var request1 = new
                {
                    tenant = _apiSettings.Tenant,

                };
                string token = string.Empty;
                var tokenResponse = await _httpClient.PostAsJsonAsync("https://bca.bca-365.com:441/TentaloAuth/api/token", request1);
                if (tokenResponse.IsSuccessStatusCode)
                {

                    var tokenObj = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
                    token = tokenObj.AccessToken;

                }
                var fullUrl = $"{_apiSettings.Url }{ _apiSettings.Tenant}/{ _apiSettings.Entorno}/api/{ _apiSettings.APIPublisher}/{ _apiSettings.APIGroup}/{ _apiSettings.APIVersion}/companies({ _apiSettings.Empresa})/{path}";
               // var json = JsonSerializer.Serialize(data);
              
                var json = System.Text.Json.JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                Console.WriteLine($"JSON generado: {json}"); // Para debug - eliminar en producción

 


                var requestP = new HttpRequestMessage(HttpMethod.Post, fullUrl); // CORREGIDO: era urlBuffer
                requestP.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                requestP.Content = content;
                requestP.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                requestP.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                requestP.Headers.Remove("Isolation");
                requestP.Headers.Add("Isolation", "snapshot");
                Console.WriteLine($"API POST: {fullUrl}");
                var responsep = await _httpClient.SendAsync(requestP); // CORREGIDO: era request

                if (responsep.StatusCode != HttpStatusCode.Created)
                {
                    var errorContent = await responsep.Content.ReadAsStringAsync();
                    return responsep;


                }

                return responsep;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en POST request: {ex.Message}");
                throw;
            }
        }

        public async Task<HttpResponseMessage> PostAsync2(string path, object data)
        {
            try
            {
                var fullUrl = $"{_apiSettings.Url}{_apiSettings.Tenant}/{_apiSettings.Entorno}/api/{_apiSettings.APIPublisher}/{_apiSettings.APIGroup}/{_apiSettings.APIVersion}/companies({_apiSettings.Empresa})/{path}";
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage(HttpMethod.Post, fullUrl)
                {
                    Content = content
                };
                Console.WriteLine($"API POST: {fullUrl}");
                return await _httpClient.SendAsync(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en POST request: {ex.Message}");
                throw;
            }
        }

        public async Task<HttpResponseMessage> PatchAsync(string path, object data)
        {
            try
            {
                var fullUrl = $"{_apiSettings.Url}{_apiSettings.Tenant}/{_apiSettings.Entorno}/api/{_apiSettings.APIPublisher}/{_apiSettings.APIGroup}/{_apiSettings.APIVersion}/companies({_apiSettings.Empresa})/{path}";
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Patch, fullUrl)
                {
                    Content = content
                };

                request.Headers.Add("If-Match", "*");

                Console.WriteLine($"API PATCH: {fullUrl}");
                return await _httpClient.SendAsync(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en PATCH request: {ex.Message}");
                throw;
            }
        }
    }
}