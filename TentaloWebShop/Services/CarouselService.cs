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
    private readonly LocalizationService _localization;
    private List<CarouselItem>? _cache;
    private List<CarouselItem>? _rawCache;
    private string? _lastCustomerNo;
    private string? _lastLanguage;

    public CarouselService(RestDataService rest, AuthService auth, LocalizationService localization)
    {
        _rest = rest;
        _auth = auth;
        _localization = localization;
        _auth.OnCustomerChanged += OnCustomerChanged;
        _localization.OnLanguageChanged += OnLanguageChanged;
    }

    /// <summary>
    /// Obtiene todos los elementos del carrusel desde Business Central
    /// Implementa caché para evitar llamadas repetidas
    /// </summary>
    public async Task<List<CarouselItem>> GetCarruselesAsync()
    {
        // Determinar qué número de cliente usar
        string customerNo = GetEffectiveCustomerNo();

        string currentLang = _localization.CurrentLanguage;

        // Si cambió el cliente, invalidar caché
        if (_lastCustomerNo != customerNo)
        {
            _cache = null;
            _rawCache = null;
            _lastCustomerNo = customerNo;
        }

        // Si cambió el idioma, re-traducir desde raw
        if (_lastLanguage != currentLang && _rawCache is not null)
        {
            _cache = TranslateCarouselItems(_rawCache);
            _lastLanguage = currentLang;
            return _cache;
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

        // Guardar en caché raw y traducir
        _rawCache = list;
        _cache = TranslateCarouselItems(list);
        _lastLanguage = currentLang;
        return _cache;
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
        _rawCache = null;
        _lastCustomerNo = null;
        _lastLanguage = null;
        Console.WriteLine("[CarouselService] Caché limpiada");
    }

    private List<CarouselItem> TranslateCarouselItems(List<CarouselItem> raw)
    {
        return raw.Select(item => new CarouselItem
        {
            No = item.No,
            Title = _localization.TranslateCarousel(item.Title),
            Subtitle = _localization.TranslateCarousel(item.Subtitle),
            Alt = item.Alt,
            ImageUrl = item.ImageUrl,
            ButtonText = _localization.TranslateCarousel(item.ButtonText),
            ButtonLink = item.ButtonLink,
            ExtensionImagen = item.ExtensionImagen
        }).ToList();
    }

    private void OnLanguageChanged()
    {
        if (_rawCache is not null)
        {
            _cache = TranslateCarouselItems(_rawCache);
            _lastLanguage = _localization.CurrentLanguage;
        }
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
