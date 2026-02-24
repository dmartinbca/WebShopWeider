using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using TentaloWebShop.Models;

namespace TentaloWebShop.Services;

/// <summary>
/// Servicio para gestionar las aprobaciones de pedidos de atletas.
/// Basado en el ApprovalService de Tentalo Blazor.
/// </summary>
public class ApprovalService
{
    private readonly HttpClient _http;
    private readonly ApiSettings _apiSettings;

    public ApprovalService(HttpClient http, IOptions<ApiSettings> apiSettings)
    {
        _http = http;
        _apiSettings = apiSettings.Value;
    }

    /// <summary>
    /// Obtiene un token de autenticación para acceder a las APIs de BC.
    /// </summary>
    private async Task<string> GetTokenAsync()
    {
        var request = new { tenant = _apiSettings.Tenant };
        var response = await _http.PostAsJsonAsync("https://authenticator.tentalo.com:441/api/token", request);

        if (response.IsSuccessStatusCode)
        {
            var tokenObj = await response.Content.ReadFromJsonAsync<TokenResponse>();
            return tokenObj?.AccessToken ?? string.Empty;
        }

        return string.Empty;
    }

    /// <summary>
    /// Construye la URL base de la API de BC.
    /// </summary>
    private string BuildApiUrl(string endpoint)
    {
        return $"{_apiSettings.Url}{_apiSettings.Tenant}/{_apiSettings.Entorno}/api/{_apiSettings.APIPublisher}/{_apiSettings.APIGroup}/{_apiSettings.APIVersion}/companies({_apiSettings.Empresa})/{endpoint}";
    }

    /// <summary>
    /// Obtiene los detalles completos de una aprobación (cabecera + líneas + info empresa).
    /// </summary>
    public async Task<ApprovalDetails?> GetApprovalDetailsAsync(string approvalId)
    {
        try
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("[ApprovalService] Error: No se pudo obtener token");
                return null;
            }

            // Llamar a la acción getApprovalDetails del API
            var url = BuildApiUrl($"athleteApprovals({approvalId})/Microsoft.NAV.getApprovalDetails");

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("Isolation", "snapshot");
            request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ApprovalService] Error {response.StatusCode}: {errorContent}");
                return null;
            }

            var responseText = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[ApprovalService] Response: {responseText}");

            // BC retorna { "value": "json_string" } — doble deserialización
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Intentar primero como wrapper { "value": "..." }
            try
            {
                var wrapper = JsonSerializer.Deserialize<ApprovalDetailsStringResponse>(responseText, options);
                if (wrapper != null && !string.IsNullOrEmpty(wrapper.Value))
                {
                    var details = JsonSerializer.Deserialize<ApprovalDetails>(wrapper.Value, options);
                    return details;
                }
            }
            catch
            {
                // Si falla, intentar deserializar directamente
            }

            // Intento directo
            var directResult = JsonSerializer.Deserialize<ApprovalDetails>(responseText, options);
            return directResult;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ApprovalService] Exception: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Aprueba un pedido de atleta.
    /// </summary>
    public async Task<ApprovalActionResult> ApproveOrderAsync(string approvalId)
    {
        try
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
                return new ApprovalActionResult { Success = false, Error = "Error de autenticación" };

            var url = BuildApiUrl($"athleteApprovals({approvalId})/Microsoft.NAV.approve");

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("Isolation", "snapshot");
            request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return new ApprovalActionResult { Success = true };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ApprovalService.Approve] Error {response.StatusCode}: {errorContent}");
                return new ApprovalActionResult { Success = false, Error = $"Error: {response.StatusCode}" };
            }
        }
        catch (Exception ex)
        {
            return new ApprovalActionResult { Success = false, Error = ex.Message };
        }
    }

    /// <summary>
    /// Rechaza un pedido de atleta con motivo.
    /// </summary>
    public async Task<ApprovalActionResult> RejectOrderAsync(string approvalId, string reason)
    {
        try
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
                return new ApprovalActionResult { Success = false, Error = "Error de autenticación" };

            var url = BuildApiUrl($"athleteApprovals({approvalId})/Microsoft.NAV.reject");

            var payload = new { rejectionReason = reason };
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("Isolation", "snapshot");
            request.Content = jsonContent;

            var response = await _http.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return new ApprovalActionResult { Success = true };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ApprovalService.Reject] Error {response.StatusCode}: {errorContent}");
                return new ApprovalActionResult { Success = false, Error = $"Error: {response.StatusCode}" };
            }
        }
        catch (Exception ex)
        {
            return new ApprovalActionResult { Success = false, Error = ex.Message };
        }
    }
}
// TokenResponse ya definida en Models/Models.cs

