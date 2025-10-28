using TentaloWebShop.Models;

namespace TentaloWebShop.Services;

/// <summary>
/// Servicio para gestionar los carruseles de la página de inicio
/// Obtiene datos desde Business Central Cloud API
/// </summary>
public class CarouselService
{
    private readonly RestDataService _rest;
    private readonly AuthService _auth;
    private List<CarouselItem>? _cache;
    private string? _lastCustomerNo; // Para detectar cambios de cliente

    public CarouselService(RestDataService rest, AuthService auth)
    {
        _rest = rest;
        _auth = auth;
        _auth.OnCustomerChanged += OnCustomerChanged;
    }

    /// <summary>
    /// Obtiene todos los elementos del carrusel desde Business Central
    /// Implementa caché para evitar llamadas repetidas
    /// </summary>
    public async Task<List<CarouselItem>> GetCarruselesAsync()
    {
        // Determinar qué número de cliente usar
        string customerNo = GetEffectiveCustomerNo();

        // Si cambió el cliente, invalidar caché
        if (_lastCustomerNo != customerNo)
        {
            _cache = null;
            _lastCustomerNo = customerNo;
        }

        // Retornar desde caché si está disponible
        if (_cache is not null)
            return _cache;

        var list = new List<CarouselItem>();

        try
        {
            // Obtener datos del API
            var carruseles = await _rest.GetCarruselesAPICloud(customerNo);

            if (carruseles != null)
            {
                foreach (var item in carruseles)
                {
                    try
                    {
                        list.Add(new CarouselItem
                        {
                            No = item.No,
                            Title = item.Title ?? "",
                            Subtitle = item.Subtitle ?? "",
                            Alt = item.Alt ?? "",
                            ImageUrl = string.IsNullOrWhiteSpace(item.ImageUrl) 
                                ? "/images/image.png" 
                                : item.ImageUrl,
                            ButtonText = item.ButtonText ?? "",
                            ButtonLink = item.ButtonLink ?? "",
                            ExtensionImagen = item.ExtensionImagen ?? ".jpg"
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[CarouselService.GetCarruselesAsync] Error mapeando item: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CarouselService.GetCarruselesAsync] {ex.Message}");
        }

        // Guardar en caché
        _cache = list;
        return list;
    }

    /// <summary>
    /// Obtiene un carrusel específico por su número
    /// </summary>
    public async Task<CarouselItem?> GetCarruselByNoAsync(int no)
    {
        var all = await GetCarruselesAsync();
        return all.FirstOrDefault(c => c.No == no);
    }

    /// <summary>
    /// Limpia la caché para forzar una nueva carga desde el API
    /// </summary>
    public void ClearCache()
    {
        _cache = null;
        _lastCustomerNo = null;
        Console.WriteLine("[CarouselService] Caché limpiada");
    }

    // ========== MÉTODOS PRIVADOS ==========

    /// <summary>
    /// Determina el número de cliente efectivo a usar
    /// </summary>
    private string GetEffectiveCustomerNo()
    {
        return _auth.CurrentUser?.CustomerNo ?? "";
    }

    /// <summary>
    /// Handler para cuando cambia el cliente autenticado
    /// </summary>
    private async Task OnCustomerChanged()
    {
        Console.WriteLine("[CarouselService.OnCustomerChanged] Limpiando caché");
        ClearCache();
        await Task.CompletedTask;
    }
}

/// <summary>
/// Modelo para representar un elemento del carrusel
/// </summary>
public class CarouselItem
{
    /// <summary>Identificador único del carrusel en Business Central</summary>
    public int No { get; set; }

    /// <summary>Título principal del slide</summary>
    public string Title { get; set; } = "";

    /// <summary>Subtítulo descriptivo</summary>
    public string Subtitle { get; set; } = "";

    /// <summary>Texto alternativo de la imagen</summary>
    public string Alt { get; set; } = "";

    /// <summary>URL de la imagen desde Azure Blob Storage</summary>
    public string ImageUrl { get; set; } = "";

    /// <summary>Texto del botón de acción</summary>
    public string ButtonText { get; set; } = "";

    /// <summary>Enlace del botón de acción</summary>
    public string ButtonLink { get; set; } = "";

    /// <summary>Extensión de archivo de la imagen</summary>
    public string ExtensionImagen { get; set; } = ".jpg";
}
