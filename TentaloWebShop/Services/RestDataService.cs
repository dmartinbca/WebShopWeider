using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Text.Json;
using TentaloWebShop.Models;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static TentaloWebShop.Services.AuthService;
namespace TentaloWebShop.Services
{
    public class RestDataService
    {

        private const string KEY_USER = "auth.currentUser";
        //private const string Empresa = "34efe56f-d278-ee11-817a-002248a140e0";
        //private const string Tenant = "77a19bf5-b81d-4933-a146-51e48b3c063f";
        //private const string Url = "https://api.businesscentral.dynamics.com/v2.0/";
        //private const string usuarioCloud = "1994a7d2-3932-40d0-8691-4163bf044eb1";
        //private const string PassCloud = "YDo8Q~hmkUK_mTviuIrNWqpJoppuHQUNoTNGSdd6";
        //private const string Entorno = "BCA";
        //private const string APIPublisher = "BCA";
        //private const string APIGroup = "BCAApp";
        //private const string APIVersion = "v2.0";

        private readonly HttpClient _http;
        private readonly LocalStorageService _store;
        private readonly ApiSettings _apiSettings;
        private readonly IServiceProvider _serviceProvider;  // ✅ AÑADIDO para resolver AuthService cuando sea necesario
        public RestDataService(HttpClient http, LocalStorageService store, IOptions<ApiSettings> apiSettings, IServiceProvider serviceProvider)
        {
            _http = http;
            _store = store;
            _apiSettings = apiSettings.Value;
            _serviceProvider = serviceProvider;  // ✅ AÑADIDO
        }
        private AuthService? GetAuthService()
        {
            try
            {
                return _serviceProvider.GetService<AuthService>();
            }
            catch
            {
                return null;
            }
        }
        public async Task<NavUser> GetAppLoginAPICloud(string usuario, string password)
        {

            NavUser musuario = null;
            var request1 = new
            {
                tenant = _apiSettings.Tenant,
               
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
                var url = $"{_apiSettings.Url}{_apiSettings.Tenant}/{_apiSettings.Entorno}/api/{_apiSettings.APIPublisher}/{_apiSettings.APIGroup}/{_apiSettings.APIVersion}/companies({_apiSettings.Empresa})/AppUsuarios?$filter=usuario eq '{usuario.ToLower()}' and password eq '{password}'";

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
                    var result = System.Text.Json.JsonSerializer.Deserialize<AppLoginResponse>(content, options);

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
                    tenant = _apiSettings.Tenant,
                    
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
                string baseUrl = $"{_apiSettings.Url}{_apiSettings.Tenant}/{_apiSettings.Entorno}/api/{_apiSettings.APIPublisher}/{_apiSettings.APIGroup}/{_apiSettings.APIVersion}/companies({_apiSettings.Empresa})/AppCustomers";
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
                    var data = System.Text.Json.JsonSerializer.Deserialize<CustomerJson>(content, options);

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

        public async Task<List<Customer>> GetCustomersAPI(string vendedor, string fecha)
        {
            var resultado = new List<Customer>();
            try
            {
                // 1. Obtener token OAuth2
                var request1 = new
                {
                    tenant = _apiSettings.Tenant,

                };

                string token = string.Empty;
                var tokenResponse = await _http.PostAsJsonAsync("https://bca.bca-365.com:441/TentaloAuth/api/token", request1);
                if (tokenResponse.IsSuccessStatusCode)
                {

                    var tokenObj = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
                    token = tokenObj.AccessToken;

                }
                // 2. Construir URL con filtro por cliente
             
                string baseUrl = $"{_apiSettings.Url}{_apiSettings.Tenant}/{_apiSettings.Entorno}/api/{_apiSettings.APIPublisher}/{_apiSettings.APIGroup}/{_apiSettings.APIVersion}/companies({_apiSettings.Empresa})/AppCustomers";
                var authService = GetAuthService();
                string filter = authService?.CurrentUser?.IdiomaPais == "ITA"
                    ? $"?$filter=countryRegionCode eq 'IT' and orderDateFilterOnly le {fecha}"
                    : authService?.CurrentUser?.EsMaster == true
                        ? $"?$filter=orderDateFilterOnly le {fecha}"
                        : $"?$filter=salespersonCode eq '{vendedor}' and orderDateFilterOnly le {fecha}";

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
                    var data = System.Text.Json.JsonSerializer.Deserialize<CustomerJson>(content, options);

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
        public async Task<List<FamliasCloud>> GetFamiliasAPICloud()
        {
            var resultado = new List<FamliasCloud>();

            try
            {
                // 1. Obtener token OAuth2 (ajusta si ya lo tienes guardado en tu AppState)
                var request1 = new
                {
                    tenant = _apiSettings.Tenant,
                    
                };

                string token = string.Empty;
                var tokenResponse = await _http.PostAsJsonAsync("https://bca.bca-365.com:441/TentaloAuth/api/token", request1);
                if (tokenResponse.IsSuccessStatusCode)
                {

                    var tokenObj = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
                    token = tokenObj.AccessToken;

                }
                var url = "";
                var authService = GetAuthService();
                string stipo = authService?.CurrentUser?.Tipo;
                if (stipo == "Customer") 
                {
                    url = $"{_apiSettings.Url}{_apiSettings.Tenant}/{_apiSettings.Entorno}/api/{_apiSettings.APIPublisher}/{_apiSettings.APIGroup}/{_apiSettings.APIVersion}/companies({_apiSettings.Empresa})/AppFamiliasWeb?$expand=subfamlines";

                }
                else
                {
                    url = $"{_apiSettings.Url}{_apiSettings.Tenant}/{_apiSettings.Entorno}/api/{_apiSettings.APIPublisher}/{_apiSettings.APIGroup}/{_apiSettings.APIVersion}/companies({_apiSettings.Empresa})/AppFamiliasWebII?$expand=subfamlines";

                }
                // 2. Construir URL

                // 3. Preparar HttpClient

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Remove("Isolation");
                request.Headers.Add("Isolation", "snapshot");

                // 4. Llamada (puedes envolver con política Polly si la tienes configurada)
                var response = await _http.SendAsync(request);

                // 5. Procesar respuesta
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = System.Text.Json.JsonSerializer.Deserialize<FamiliasCloudJson>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (data?.Value != null)
                    {
                         

                        resultado.AddRange(
                            data.Value
                                .OrderBy(c => c.Order)
                                
                        );
                    }

                     
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetFamiliasAPICloud] {ex.Message}");
            }

            return resultado;
        }
        public async Task<List<ProductCloud>> GetProductosAPICloud(string familia, string subfamilia)
        {
            var resultado = new List<ProductCloud>();
            var saved = await _store.GetAsync<User>("auth.currentUser");
            try
            {
                // 1. Obtener token OAuth2 (igual que en GetFamiliasAPICloud)
                var requestToken = new
                {
                    tenant = _apiSettings.Tenant,
                   
                };

                string token = string.Empty;
                var tokenResponse = await _http.PostAsJsonAsync("https://bca.bca-365.com:441/TentaloAuth/api/token", requestToken);
                if (tokenResponse.IsSuccessStatusCode)
                {
                    var tokenObj = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
                    token = tokenObj.AccessToken;
                }

                // 2. Construir la URL dinámica según los parámetros recibidos
                var baseUrl = $"{_apiSettings.Url}{_apiSettings.Tenant}/{_apiSettings.Entorno}/api/{_apiSettings.APIPublisher}/{_apiSettings.APIGroup}/{_apiSettings.APIVersion}/companies({_apiSettings.Empresa})";
                string endpoint;
                var authService = GetAuthService();
                string stipo = authService?.CurrentUser?.Tipo;
                if(stipo=="Customer")
                {
                    // Lógica igual que tu versión MAUI
                    if (string.IsNullOrWhiteSpace(familia) && string.IsNullOrWhiteSpace(subfamilia))
                    {
                        endpoint = $"/ApiListaProductosIV?$filter=no eq '{saved.CustomerNo}' and FamiliaN ne ' ' and SubFamilia ne ' ' and ImageUrl ne ' '";
                    }
                    else
                    {
                        endpoint = $"/ApiListaProductosIV?$filter=no eq '{saved.CustomerNo}' and FamiliaN eq '{familia}' and SubFamilia eq '{subfamilia}'";
                    }
                }
                else
                {
                    // Lógica igual que tu versión MAUI
                    if (string.IsNullOrWhiteSpace(familia) && string.IsNullOrWhiteSpace(subfamilia))
                    {
                        endpoint = $"/ApiListaProductosII?$filter=no eq '{saved.CustomerNo}' and FamiliaN ne ' ' and SubFamilia ne ' '";
                    }
                    else
                    {
                        endpoint = $"/ApiListaProductosII?$filter=no eq '{saved.CustomerNo}' and FamiliaN eq '{familia}' and SubFamilia eq '{subfamilia}'";
                    }
                }
               

                var url = baseUrl + endpoint;

                // 3. Crear petición con HttpClient y cabeceras OAuth2 y Isolation
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Remove("Isolation");
                request.Headers.Add("Isolation", "snapshot");

                // 4. Llamada
                var response = await _http.SendAsync(request);

                // 5. Procesar respuesta
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = System.Text.Json.JsonSerializer.Deserialize<ProductoCloudJson>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (data?.Value != null)
                    {
                        // Si hay cliente seleccionado y filtro por familia/subfamilia, filtra por precio válido
                        if (!string.IsNullOrWhiteSpace(familia) && !string.IsNullOrWhiteSpace(subfamilia))
                        {
                            if (!string.IsNullOrWhiteSpace(saved.CustomerNo))
                                resultado.AddRange(data.Value.Where(c => c.ActualPrice >= 0));
                            else
                                resultado.AddRange(data.Value.Where(c => c.ActualPrice == 0));
                        }
                        else
                        {
                            resultado.AddRange(data.Value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetProductosAPICloud] {ex.Message}");
            }

            return resultado;
        }
        public async Task<Status> BorrarPedido(string docref)
        {
            var result = new Status();

            try
            {
                var requestToken = new
                {
                    tenant = _apiSettings.Tenant,

                };

                string token = string.Empty;
                var tokenResponse = await _http.PostAsJsonAsync("https://bca.bca-365.com:441/TentaloAuth/api/token", requestToken);
                if (tokenResponse.IsSuccessStatusCode)
                {
                    var tokenObj = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
                    token = tokenObj.AccessToken;
                }
                var baseUrl = $"{_apiSettings.Url}{_apiSettings.Tenant}/{_apiSettings.Entorno}/api/{_apiSettings.APIPublisher}/{_apiSettings.APIGroup}/{_apiSettings.APIVersion}/companies({_apiSettings.Empresa})/APiBorrarPedidos({docref})/Microsoft.NAV.BorrarPedido";

                // 3. Construir URL
 


                var request = new HttpRequestMessage(HttpMethod.Post, baseUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Remove("Isolation");
                request.Headers.Add("Isolation", "snapshot");

                var response = await _http.SendAsync(request);
                // 5. Evaluar resultado
                if (!response.IsSuccessStatusCode)
                {
                    result.IsSuccess = false;
                    result.Message = await response.Content.ReadAsStringAsync();
                    return result;
                }

                result.IsSuccess = true;
                return result;
            }
            catch (Exception ex)
            {
                return new Status { IsSuccess = false, Message = ex.Message };
            }
        }


        public async Task<List<Estadisticas>> GetEstadisticasVentas(string cliente)
        {
            var mensajes = new List<Estadisticas>();

            try
            {
                var request1 = new
                {
                    tenant = _apiSettings.Tenant,
                   
                };

                string token = string.Empty;
                var tokenResponse = await _http.PostAsJsonAsync("https://bca.bca-365.com:441/TentaloAuth/api/token", request1);
                if (tokenResponse.IsSuccessStatusCode)
                {

                    var tokenObj = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
                    token = tokenObj.AccessToken;

                }

                // Construir URL con filtro
                var url = $"{_apiSettings.Url}{_apiSettings.Tenant}/{_apiSettings.Entorno}/api/{_apiSettings.APIPublisher}/{_apiSettings.APIGroup}/{_apiSettings.APIVersion}/companies({_apiSettings.Empresa})/APIDataSales?$filter=Cliente eq '{cliente}'";

                // 3. Crear petición con HttpClient y cabeceras OAuth2 y Isolation
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Remove("Isolation");
                request.Headers.Add("Isolation", "snapshot");

                var response = await _http.SendAsync(request);


                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = System.Text.Json.JsonSerializer.Deserialize<EstadisticasJson>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (data?.Value != null)
                    {
                        mensajes.AddRange(data.Value);
                         
                    }
                }

                return mensajes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetEstadisticasVentas(cliente): {ex.Message}");
                return mensajes;
            }
        }


        public async Task<List<CustomerAddres>> GetDirecciones(string cliente)
        {
            var resultado = new List<CustomerAddres>();

            try
            {
                var request1 = new
                {
                    tenant = _apiSettings.Tenant,
                   
                };

                string token = string.Empty;
                var tokenResponse = await _http.PostAsJsonAsync("https://bca.bca-365.com:441/TentaloAuth/api/token", request1);
                if (tokenResponse.IsSuccessStatusCode)
                {

                    var tokenObj = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
                    token = tokenObj.AccessToken;

                }

              
                // Construir URL
                var url = $"{_apiSettings.Url}{_apiSettings.Tenant}/{_apiSettings.Entorno}/api/{_apiSettings.APIPublisher}/{_apiSettings.APIGroup}/{_apiSettings.APIVersion}/companies({_apiSettings.Empresa})/ApiDirEnvios?$filter=customerNo eq '{cliente}'";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Remove("Isolation");
                request.Headers.Add("Isolation", "snapshot");

                var response = await _http.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = System.Text.Json.JsonSerializer.Deserialize<CustomerAddresJSON>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (data?.Value != null)
                    {
                        resultado.AddRange(data.Value);

                    }
                }

              
                return resultado;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR GetDirecciones] {ex.Message}");
                return resultado;
            }
        }


        // Método actualizado en RestDataService.cs
        // Reemplazar el método PedidoVenta existente con este
        // ============================================================================
        // MÉTODO A REEMPLAZAR EN RestDataService.cs
        // ============================================================================
        // Cambiar la firma del método PedidoVenta de:
        //   public async Task<Status> PedidoVenta(List<CartItem> carro, string Observaciones, string direnvio, string usuario, string cliente)
        // A:
        //   public async Task<Status> PedidoVenta(List<CartItem> carro, string cliente, double descuentoCabecera, string Observaciones, string direnvio, string usuario)
        // ============================================================================
        // ============================================================================
        // MÉTODO A ACTUALIZAR EN RestDataService.cs
        // ============================================================================
        // Buscar este método:
        //   public async Task<Status> PedidoVenta(List<CartItem> carro, string Observaciones, string direnvio, string usuario, string cliente)
        //
        // Y cambiar SOLO la firma y una línea:
        // ============================================================================

        public async Task<Status> PedidoVenta(
            List<CartItem> carro,
            string cliente,            // ← Nuevo orden de parámetros
            double descuentoCabecera,  // ← NUEVO PARÁMETRO
            string Observaciones,
            string direnvio,
            string usuario)
        {
            var result = new Status();
            direnvio ??= string.Empty;
            var fecha = DateTime.Now.ToString("yyyy-MM-dd");
            int nuevoNumeroPedido = 0;

            try
            {
                // 1. Obtener token de autenticación
                var request1 = new
                {
                    tenant = _apiSettings.Tenant,
                };

                string token = string.Empty;
                var tokenResponse = await _http.PostAsJsonAsync("https://bca.bca-365.com:441/TentaloAuth/api/token", request1);
                if (tokenResponse.IsSuccessStatusCode)
                {
                    var tokenObj = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
                    token = tokenObj.AccessToken;
                }

                // 2. Obtener número de pedido
                var urlBuffer = $"{_apiSettings.Url}{_apiSettings.Tenant}/{_apiSettings.Entorno}/api/{_apiSettings.APIPublisher}/{_apiSettings.APIGroup}/{_apiSettings.APIVersion}/companies({_apiSettings.Empresa})/AppBufferNumero";

                var request = new HttpRequestMessage(HttpMethod.Get, urlBuffer);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Remove("Isolation");
                request.Headers.Add("Isolation", "snapshot");

                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var contentB = await response.Content.ReadAsStringAsync();
                    var data = System.Text.Json.JsonSerializer.Deserialize<BufferNumeroJSON>(contentB, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })?.Value?.FirstOrDefault();
                    if (data == null)
                        return new Status { IsSuccess = false, Message = "Número de pedido no válido" };
                    nuevoNumeroPedido = data.maxId + 1;
                }

                // 3. Construir líneas del pedido a partir del carrito
                var lineas = carro.Select((p, i) => new BufferPedidos
                {
                    Id = nuevoNumeroPedido + i,
                    Almacen = "001",
                    Articulo = p.Product.Itemno,
                    Descripcion = p.Product.Name,
                    Cantidad = p.Quantity,
                    Tipo_Pedido = "WebShop",
                    Cliente = cliente,
                    EnviadaHacienda = false,
                    Estado = "Pendiente",
                    Fecha = fecha,
                    Fecha_Envio = fecha,
                    MailFactura = false,
                    MailPedido = false,
                    Notificacion = false,
                    Obervaciones = Observaciones,
                    Pedido = nuevoNumeroPedido,
                    Precio = Convert.ToDouble(p.Product.PriceFrom),
                    ProcesadaFactura = false,
                    ProcesadoPedido = false,
                    Proveedor = "",
                    Tipo = "Customer",
                    Tocken = "",
                    Unidad = p.Product.UnitofMeasure,
                    Usuario = usuario,
                    IdGUID = Guid.NewGuid(),
                    Token = "",
                    Descuento_Linea = 0
                }).ToList();

                // 4. Crear cabecera con el descuento en factura
                var cabecera = new cabPedidosVenta
                {
                    Pedido = nuevoNumeroPedido,
                    Cod_Cliente = cliente,
                    Fecha = fecha,
                    Descuento_Cabecera = descuentoCabecera, // ← ÚNICO CAMBIO EN EL CUERPO DEL MÉTODO
                    Observaciones = Observaciones,
                    Estado = "Pendiente",
                    Direnvio = direnvio,
                    Usuario = usuario,
                    Obervaciones_Pedido = "",
                    Tipo = "Pedido",
                    detPedidos = lineas
                };

                // 5. Enviar pedido a Business Central
                var urlPedido = $"{_apiSettings.Url}{_apiSettings.Tenant}/{_apiSettings.Entorno}/api/{_apiSettings.APIPublisher}/{_apiSettings.APIGroup}/{_apiSettings.APIVersion}/companies({_apiSettings.Empresa})/cabPedidos?$expand=detPedidos";

                var json = System.Text.Json.JsonSerializer.Serialize(cabecera);
                Console.WriteLine($"JSON generado: {json}"); // Para debug

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var requestP = new HttpRequestMessage(HttpMethod.Post, urlPedido);
                requestP.Content = content;
                requestP.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                requestP.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                requestP.Headers.Remove("Isolation");
                requestP.Headers.Add("Isolation", "snapshot");

                var responsep = await _http.SendAsync(requestP);

                if (responsep.StatusCode != HttpStatusCode.Created)
                {
                    var errorContent = await responsep.Content.ReadAsStringAsync();
                    return new Status
                    {
                        IsSuccess = false,
                        Message = $"Error al crear pedido: {responsep.StatusCode} - {errorContent}"
                    };
                }

                // 6. Procesar pedido (enviar notificaciones, etc.)
                var procesar = await ProcesarPedido(cabecera.Pedido.ToString(), "", "", "", "Envío Pedido", "");
                return procesar ?? new Status { IsSuccess = false, Message = "Error procesando el pedido" };
            }
            catch (Exception ex)
            {
                return new Status { IsSuccess = false, Message = $"[ERROR PedidoVenta] {ex.Message}" };
            }
        }

        // ============================================================================
        // RESUMEN DE CAMBIOS:
        // ============================================================================
        // 1. Agregar parámetro: double descuentoCabecera
        // 2. Cambiar línea: Descuento_Cabecera = descuentoCabecera (antes era = 0)
        // ============================================================================
        public async Task<Status> ProcesarPedido(string docref, string email, string emailcc, string emailcco, string asunto, string body)
        {
            try
            {
                // 1. Obtener token
                var request1 = new
                {
                    tenant = _apiSettings.Tenant,
                    
                };

                string token = string.Empty;
                var tokenResponse = await _http.PostAsJsonAsync("https://bca.bca-365.com:441/TentaloAuth/api/token", request1);
                if (tokenResponse.IsSuccessStatusCode)
                {
                    var tokenObj = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
                    token = tokenObj.AccessToken;
                }
                else
                {
                    return new Status { IsSuccess = false, Message = "Error obteniendo token de autenticación" };
                }

                // 2. Procesar pedido
                var url = $"{_apiSettings.Url}{_apiSettings.Tenant}/{_apiSettings.Entorno}/api/{_apiSettings.APIPublisher}/{_apiSettings.APIGroup}/{_apiSettings.APIVersion}/companies({_apiSettings.Empresa})/ApiProcesarpedidos({docref})/Microsoft.NAV.ProcesarPedido";

                var envio = new DocumentoEnvio
                {
                    email = email,
                    emailcc = emailcc,
                    emailcco = emailcco,
                    asunto = asunto,
                    body = body
                };

                var content = new StringContent(JsonConvert.SerializeObject(envio), Encoding.UTF8, "application/json");
                var requestP = new HttpRequestMessage(HttpMethod.Post, url);
                requestP.Content = content;
                requestP.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                requestP.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                requestP.Headers.Remove("Isolation");
                requestP.Headers.Add("Isolation", "snapshot");

                var responsep = await _http.SendAsync(requestP);

                if (responsep.IsSuccessStatusCode)
                {
                    return new Status { IsSuccess = true, Message = "Pedido procesado correctamente." };
                }
                else
                {
                    var errorContent = await responsep.Content.ReadAsStringAsync();
                    return new Status { IsSuccess = false, Message = $"Error procesando pedido: {responsep.StatusCode} - {errorContent}" };
                }
            }
            catch (Exception ex)
            {
                return new Status
                {
                    IsSuccess = false,
                    Message = $"[ERROR ProcesarPedido] {ex.Message}"
                };
            }
        }

        public async Task<List<OrderNAVCabecera>> ListaPedidosCabeceraVenta(string cliente)
        {
            var resultado = new List<OrderNAVCabecera>();

            try
            {
                var request1 = new
                {
                    tenant = _apiSettings.Tenant,

                };

                string token = string.Empty;
                var tokenResponse = await _http.PostAsJsonAsync("https://bca.bca-365.com:441/TentaloAuth/api/token", request1);
                if (tokenResponse.IsSuccessStatusCode)
                {

                    var tokenObj = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
                    token = tokenObj.AccessToken;

                }

            
                var fecha = DateTime.Now;
                var fechastr = new DateTime(fecha.Year, 1, 1).ToString("yyyy-MM-dd");

                var urlBase = $"{_apiSettings.Url}{_apiSettings.Tenant}/{_apiSettings.Entorno}/api/{_apiSettings.APIPublisher}/{_apiSettings.APIGroup}/{_apiSettings.APIVersion}/companies({_apiSettings.Empresa})/APIPSalesOrders";
                var filtro =  $"$filter=Document_Type eq 'Order' and Sell_to_Customer_No eq '{cliente}' and Order_Date ge {fechastr}&$expand=salelines";

                var url = $"{urlBase}?{filtro}";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Remove("Isolation");
                request.Headers.Add("Isolation", "snapshot");

                var response = await _http.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = System.Text.Json.JsonSerializer.Deserialize<PedidosNAVJson>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (data?.Value != null)
                    {
                        resultado.AddRange(data.Value);

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR ListaPedidosCabeceraVenta] {ex.Message}");
            }

            return resultado;
        }
 
        public async Task<List<OrderNAVCabecera>> ListaFacturaCabeceraVenta(string cliente)
        {
            var resultado = new List<OrderNAVCabecera>();

            try
            {
                var request1 = new
                {
                    tenant = _apiSettings.Tenant,

                };

                string token = string.Empty;
                var tokenResponse = await _http.PostAsJsonAsync("https://bca.bca-365.com:441/TentaloAuth/api/token", request1);
                if (tokenResponse.IsSuccessStatusCode)
                {

                    var tokenObj = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
                    token = tokenObj.AccessToken;

                }


                var fecha = DateTime.Now;
                var fechastr = new DateTime(fecha.Year, 1, 1).ToString("yyyy-MM-dd");

                var urlBase = $"{_apiSettings.Url}{_apiSettings.Tenant}/{_apiSettings.Entorno}/api/{_apiSettings.APIPublisher}/{_apiSettings.APIGroup}/{_apiSettings.APIVersion}/companies({_apiSettings.Empresa})/APISalesInvoices";
                var filtro = $"$filter=Sell_to_Customer_No eq '{cliente}' and Posting_Date ge {fechastr}&$expand=saleInvoicelines";

                var url = $"{urlBase}?{filtro}";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Remove("Isolation");
                request.Headers.Add("Isolation", "snapshot");

                var response = await _http.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = System.Text.Json.JsonSerializer.Deserialize<PedidosNAVJson>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (data?.Value != null)
                    {
                        resultado.AddRange(data.Value);

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR ListaPedidosCabeceraVenta] {ex.Message}");
            }

            return resultado;
        }
        // MODIFICAR EL MÉTODO GetProductosAPICloud en RestDataService.cs
        // Cambiar la firma del método de:
        // public async Task<List<ProductosApiCloud>> GetProductosAPICloud(string familia, string subfamilia)
        // A:
        // public async Task<List<ProductosApiCloud>> GetProductosAPICloud(string familia, string subfamilia, string customerNo = "")

        // Y modificar la lógica para usar el customerNo recibido:

        public async Task<List<ProductCloud>> GetProductosAPICloud(string familia, string subfamilia, string customerNo = "")
        {
            var resultado = new List<ProductCloud>();

            try
            {
                // 1. Obtener token OAuth2
                var request1 = new
                {
                    tenant = _apiSettings.Tenant,
                };

                string token = string.Empty;
                var tokenResponse = await _http.PostAsJsonAsync("https://bca.bca-365.com:441/TentaloAuth/api/token", request1);
                if (tokenResponse.IsSuccessStatusCode)
                {
                    var tokenObj = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
                    token = tokenObj.AccessToken;
                }

                // 2. Determinar qué customerNo usar
                string efectiveCustomerNo = customerNo;
                if (string.IsNullOrWhiteSpace(efectiveCustomerNo))
                {
                    var saved = await _store.GetAsync<User>(KEY_USER);
                    efectiveCustomerNo = saved?.CustomerNo ?? "";
                }

                // 3. Construir la URL dinámica según los parámetros recibidos
                var baseUrl = $"{_apiSettings.Url}{_apiSettings.Tenant}/{_apiSettings.Entorno}/api/{_apiSettings.APIPublisher}/{_apiSettings.APIGroup}/{_apiSettings.APIVersion}/companies({_apiSettings.Empresa})";
                string endpoint;
               
                var authService = GetAuthService();
                string stipo = authService?.CurrentUser?.Tipo;
                // Lógica igual que tu versión MAUI
                if(stipo=="Customer")
                {
                    if (string.IsNullOrWhiteSpace(familia) && string.IsNullOrWhiteSpace(subfamilia))
                    {
                        endpoint = $"/ApiListaProductosIV?$filter=no eq '{efectiveCustomerNo}' and FamiliaN ne ' ' and SubFamilia ne ' ' and ImageUrl ne ' '";
                    }
                    else
                    {
                        endpoint = $"/ApiListaProductosIV?$filter=no eq '{efectiveCustomerNo}' and FamiliaN eq '{familia}' and SubFamilia eq '{subfamilia}'";
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(familia) && string.IsNullOrWhiteSpace(subfamilia))
                    {
                        endpoint = $"/ApiListaProductosII?$filter=no eq '{efectiveCustomerNo}' and FamiliaN ne ' ' and SubFamilia ne ' '";
                    }
                    else
                    {
                        endpoint = $"/ApiListaProductosII?$filter=no eq '{efectiveCustomerNo}' and FamiliaN eq '{familia}' and SubFamilia eq '{subfamilia}'";
                    }

                }


                    var url = baseUrl + endpoint;

                // 4. Crear petición con HttpClient y cabeceras OAuth2 y Isolation
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Remove("Isolation");
                request.Headers.Add("Isolation", "snapshot");

                // 5. Llamada
                var response = await _http.SendAsync(request);

                // 6. Deserializar
                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var data = System.Text.Json.JsonSerializer.Deserialize<ProductoCloudJson>(content, options);

                    if (data?.Value != null)
                    {
                        resultado.AddRange(data.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR GetProductosAPICloud] {ex.Message}");
            }

            return resultado;
        }

    }


}
