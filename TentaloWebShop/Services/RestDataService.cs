using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Text.Json;
using TentaloWebShop.Models;
using static System.Net.WebRequestMethods;

namespace TentaloWebShop.Services
{
    public class RestDataService
    {
        private readonly HttpClient _http;
        private const string Empresa = "34efe56f-d278-ee11-817a-002248a140e0";
        private const string Tenant = "77a19bf5-b81d-4933-a146-51e48b3c063f";
        private const string Url = "https://api.businesscentral.dynamics.com/v2.0/";
        private const string usuarioCloud = "1994a7d2-3932-40d0-8691-4163bf044eb1";
        private const string PassCloud = "YDo8Q~hmkUK_mTviuIrNWqpJoppuHQUNoTNGSdd6";
        private const string Entorno = "BCA";
        private const string APIPublisher = "BCA";
        private const string APIGroup = "BCAApp";
        private const string APIVersion = "v2.0";


        public RestDataService(HttpClient http )
        {
            _http = http;
          
        }
       
        public async Task<NavUser> GetAppLoginAPICloud(string usuario, string password)
        {

            NavUser musuario = null;
            var request1 = new
            {
                tenant = Tenant,
                client_id = usuarioCloud,
                client_secret = PassCloud,
                scope = "https://api.businesscentral.dynamics.com/.default"
            };

            try
            {
                Console.WriteLine(request1);
                string token = string.Empty;
                var tokenResponse = await _http.PostAsJsonAsync("https://bca.bca-365.com:441/TentaloAuth/api/token", request1);
                if (tokenResponse.IsSuccessStatusCode)
                {
                    
                    var tokenObj = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
                    token = tokenObj.AccessToken;
                  
                }
                 
               
                // 3. Construir URL
                var url = $"{Url}{Tenant}/{Entorno}/api/{APIPublisher}/{APIGroup}/{APIVersion}/companies({ Empresa})/AppUsuarios?$filter=usuario eq '{usuario}' and password eq '{password}'";

                // 4. Crear petición HTTP y añadir cabecera OAuth Bearer
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.Add("Isolation", "snapshot");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // 5. Enviar petición
                var response = await _http.SendAsync(request);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    // Puedes seguir usando Newtonsoft si prefieres, aquí con System.Text.Json:
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var result = JsonSerializer.Deserialize<AppLoginResponse>(content, options);

                    if (result?.Value?.Any() == true)
                    {
                        musuario = result.Value.FirstOrDefault();
                        if (musuario != null)
                        {
                          
                            Console.WriteLine(token);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR GetAppLoginAPICloud] {ex.Message}");
            }

            return musuario;
        }


        public async Task<List<Customer>> GetCustomersAPI( string cliente)
        {
            var resultado = new List<Customer>();
            try
            {
                // 1. Obtener token OAuth2
                var request1 = new
                {
                    tenant = Tenant,
                    client_id = usuarioCloud,
                    client_secret = PassCloud,
                    scope = "https://api.businesscentral.dynamics.com/.default"
                };

                string token = string.Empty;
                var tokenResponse = await _http.PostAsJsonAsync("https://bca.bca-365.com:441/TentaloAuth/api/token", request1);
                if (tokenResponse.IsSuccessStatusCode)
                {

                    var tokenObj = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
                    token = tokenObj.AccessToken;

                }
                // 2. Construir URL con filtro por cliente
                string fecha = DateTime.Now.ToString("yyyy-MM-dd");
                string baseUrl = $"{Url}{Tenant}/{Entorno}/api/{APIPublisher}/{APIGroup}/{APIVersion}/companies({Empresa})/AppCustomers";
                string filter = $"?$filter=no eq '{cliente}' and orderDateFilterOnly le {fecha}";

                string fullUrl = baseUrl + filter;

                // 3. Crear petición
                var request = new HttpRequestMessage(HttpMethod.Get, fullUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Add("Isolation", "snapshot");

                // 4. Ejecutar petición
                var response = await _http.SendAsync(request);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var data = JsonSerializer.Deserialize<CustomerJson>(content, options);

                    if (data?.Value != null)
                    {
                        resultado.AddRange(data.Value
                            .Where(c => !string.IsNullOrEmpty(c.Name))
                            .OrderBy(c => c.Name));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR GetCustomersAPI(cliente)] {ex.Message}");
            }

            return resultado;
        }
        //public async Task<List<FamliasCloud>> GetFamiliasAPICloud()
        //{
        //    var resultado = new List<FamliasCloud>();

        //    try
        //    {
        //        // 1. Obtener token OAuth2 (ajusta si ya lo tienes guardado en tu AppState)
        //        var request1 = new
        //        {
        //            tenant = _appState.Aplicacion.Applicaion_Tenant,
        //            client_id = _appState.Aplicacion.Usuario,
        //            client_secret = _appState.Aplicacion.Password,
        //            scope = "https://api.businesscentral.dynamics.com/.default"
        //        };

        //        string token = string.Empty;
        //        var tokenResponse = await _http.PostAsJsonAsync("https://bca.bca-365.com:441/TentaloAuth/api/token", request1);
        //        if (tokenResponse.IsSuccessStatusCode)
        //        {

        //            var tokenObj = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
        //            token = tokenObj.AccessToken;

        //        }

        //        // 2. Construir URL
        //        var url = $"{_appState.Aplicacion.Applicaion_URL}{_appState.Aplicacion.Applicaion_Tenant}/{_appState.Aplicacion.Entorno}/api/{_appState.Aplicacion.APIPublisher}/{_appState.Aplicacion.APIGroup}/{_appState.Aplicacion.APIVersion}/companies({_appState.Aplicacion.Empresa})/AppFamilias";

        //        // 3. Preparar HttpClient

        //        var request = new HttpRequestMessage(HttpMethod.Get, url);
        //        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        //        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //        request.Headers.Remove("Isolation");
        //        request.Headers.Add("Isolation", "snapshot");

        //        // 4. Llamada (puedes envolver con política Polly si la tienes configurada)
        //        var response = await _http.SendAsync(request);

        //        // 5. Procesar respuesta
        //        if (response.IsSuccessStatusCode)
        //        {
        //            var content = await response.Content.ReadAsStringAsync();
        //            var data = JsonSerializer.Deserialize<FamiliasCloudJson>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //            if (data?.Value != null)
        //            {
        //                resultado.Add(new FamliasCloud
        //                {
        //                    Parent_Category = "BUSCAR PRODUCTO",
        //                    OrderFamily = -1,
        //                    Famlia = "Buscar Producto",
        //                    Descripcion = "Buscar Producto"
        //                });

        //                resultado.AddRange(
        //                    data.Value
        //                        .OrderBy(c => c.OrderFamily)
        //                        .ThenBy(d => d.Order)
        //                );
        //            }

        //            // Normalizar Parent_Category
        //            foreach (var item in resultado)
        //            {
        //                if (string.IsNullOrWhiteSpace(item.Parent_Category))
        //                    item.Parent_Category = item.Famlia;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"[GetFamiliasAPICloud] {ex.Message}");
        //    }

        //    return resultado;
        //}
        //public async Task<List<ProductCloud>> GetProductosAPICloud(string familia, string subfamilia)
        //{
        //    var resultado = new List<ProductCloud>();

        //    try
        //    {
        //        // 1. Obtener token OAuth2 (igual que en GetFamiliasAPICloud)
        //        var requestToken = new
        //        {
        //            tenant = _appState.Aplicacion.Applicaion_Tenant,
        //            client_id = _appState.Aplicacion.Usuario,
        //            client_secret = _appState.Aplicacion.Password,
        //            scope = "https://api.businesscentral.dynamics.com/.default"
        //        };

        //        string token = string.Empty;
        //        var tokenResponse = await _http.PostAsJsonAsync("https://bca.bca-365.com:441/TentaloAuth/api/token", requestToken);
        //        if (tokenResponse.IsSuccessStatusCode)
        //        {
        //            var tokenObj = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
        //            token = tokenObj.AccessToken;
        //        }

        //        // 2. Construir la URL dinámica según los parámetros recibidos
        //        var baseUrl = $"{_appState.Aplicacion.Applicaion_URL}{_appState.Aplicacion.Applicaion_Tenant}/{_appState.Aplicacion.Entorno}/api/{_appState.Aplicacion.APIPublisher}/{_appState.Aplicacion.APIGroup}/{_appState.Aplicacion.APIVersion}/companies({_appState.Aplicacion.Empresa})";
        //        string endpoint;

        //        // Lógica igual que tu versión MAUI
        //        if (subfamilia == "BUSCAR PRODUCTO")
        //        {
        //            endpoint = _clientState.Customer == null
        //                ? "/ApiListaProductosIII?$filter=code eq '0'"
        //                : $"/ApiListaProductosIII?$filter=no eq '{_clientState.Customer.CustNo}' and familia ne ' ' and FamiliaN ne ' '";
        //        }
        //        else if (string.IsNullOrWhiteSpace(familia) && string.IsNullOrWhiteSpace(subfamilia))
        //        {
        //            endpoint = "/ApiCuentaProductos";
        //        }
        //        else
        //        {
        //            endpoint = _clientState.Customer == null
        //                ? $"/ApiListaProductosIII?$filter=code eq '0' and familia eq '{familia}' and FamiliaN eq '{subfamilia}'"
        //                : $"/ApiListaProductosIII?$filter=no eq '{_clientState.Customer.CustNo}' and familia eq '{familia}' and FamiliaN eq '{subfamilia}'";
        //        }

        //        var url = baseUrl + endpoint;

        //        // 3. Crear petición con HttpClient y cabeceras OAuth2 y Isolation
        //        var request = new HttpRequestMessage(HttpMethod.Get, url);
        //        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        //        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //        request.Headers.Remove("Isolation");
        //        request.Headers.Add("Isolation", "snapshot");

        //        // 4. Llamada
        //        var response = await _http.SendAsync(request);

        //        // 5. Procesar respuesta
        //        if (response.IsSuccessStatusCode)
        //        {
        //            var content = await response.Content.ReadAsStringAsync();
        //            var data = JsonSerializer.Deserialize<ProductoCloudJson>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //            if (data?.Value != null)
        //            {
        //                // Si hay cliente seleccionado y filtro por familia/subfamilia, filtra por precio válido
        //                if (!string.IsNullOrWhiteSpace(familia) && !string.IsNullOrWhiteSpace(subfamilia))
        //                {
        //                    if (!string.IsNullOrWhiteSpace(_clientState.Customer?.CustNo))
        //                        resultado.AddRange(data.Value.Where(c => c.ActualPrice >= 0));
        //                    else
        //                        resultado.AddRange(data.Value.Where(c => c.ActualPrice == 0));
        //                }
        //                else
        //                {
        //                    resultado.AddRange(data.Value);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"[GetProductosAPICloud] {ex.Message}");
        //    }

        //    return resultado;
        //}


    }

}
