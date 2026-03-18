using System.Net.Http.Json;
using Microsoft.JSInterop;

namespace TentaloWebShop.Services
{
    public class TranslationOverrides
    {
        public Dictionary<string, string> Families { get; set; } = new();
        public Dictionary<string, string> Subfamilies { get; set; } = new();
        public Dictionary<string, string> Carousel { get; set; } = new();
        public Dictionary<string, string> Promos { get; set; } = new();
        public Dictionary<string, string> Products { get; set; } = new();
        public Dictionary<string, string> Words { get; set; } = new();
    }

    public class LocalizationService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        private Dictionary<string, string> _translations = new();
        private TranslationOverrides _overrides = new();
        private string _currentLanguage = "es";

        public event Action? OnLanguageChanged;

        public string CurrentLanguage => _currentLanguage;

        public static readonly Dictionary<string, LanguageInfo> SupportedLanguages = new()
        {
            { "es", new LanguageInfo("Español", "es", "🇪🇸") },
            { "en", new LanguageInfo("English", "en", "🇬🇧") },
            { "fr", new LanguageInfo("Français", "fr", "🇫🇷") },
            { "it", new LanguageInfo("Italiano", "it", "🇮🇹") }
        };

        public LocalizationService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public async Task InitializeAsync()
        {
            try
            {
                // Intentar obtener el idioma guardado en localStorage
                var savedLanguage = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "app_language");
                if (!string.IsNullOrEmpty(savedLanguage) && SupportedLanguages.ContainsKey(savedLanguage))
                {
                    _currentLanguage = savedLanguage;
                }
                else
                {
                    // Detectar idioma del navegador
                    var browserLanguage = await _jsRuntime.InvokeAsync<string>("eval", "navigator.language || navigator.userLanguage");
                    var langCode = browserLanguage?.Split('-')[0]?.ToLower() ?? "es";
                    _currentLanguage = SupportedLanguages.ContainsKey(langCode) ? langCode : "es";
                }

                await LoadTranslationsAsync(_currentLanguage);
                await LoadOverridesAsync(_currentLanguage);
            }
            catch
            {
                // Si falla, usar español por defecto
                _currentLanguage = "es";
                await LoadTranslationsAsync("es");
                await LoadOverridesAsync("es");
            }

            // Notificar a los componentes suscritos que las traducciones están listas
            OnLanguageChanged?.Invoke();
        }

        public async Task SetLanguageAsync(string languageCode)
        {
            if (!SupportedLanguages.ContainsKey(languageCode) || languageCode == _currentLanguage)
                return;

            _currentLanguage = languageCode;
            await LoadTranslationsAsync(languageCode);
            await LoadOverridesAsync(languageCode);

            // Guardar preferencia en localStorage
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "app_language", languageCode);

            OnLanguageChanged?.Invoke();
        }

        private async Task LoadTranslationsAsync(string languageCode)
        {
            try
            {
                var translations = await _httpClient.GetFromJsonAsync<Dictionary<string, string>>($"locales/{languageCode}.json");
                _translations = translations ?? new Dictionary<string, string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading translations for {languageCode}: {ex.Message}");
                _translations = new Dictionary<string, string>();
            }
        }

        private async Task LoadOverridesAsync(string languageCode)
        {
            try
            {
                var overrides = await _httpClient.GetFromJsonAsync<TranslationOverrides>($"locales/overrides_{languageCode}.json");
                _overrides = overrides ?? new TranslationOverrides();
            }
            catch
            {
                _overrides = new TranslationOverrides();
            }
        }

        /// <summary>
        /// Traduce un nombre de familia de BC al idioma actual.
        /// Si no hay traducción, devuelve el nombre original.
        /// </summary>
        public string TranslateFamily(string familyName)
        {
            if (string.IsNullOrEmpty(familyName)) return familyName;
            return _overrides.Families.TryGetValue(familyName, out var translated) ? translated : familyName;
        }

        /// <summary>
        /// Traduce un nombre de subfamilia de BC al idioma actual.
        /// </summary>
        public string TranslateSubfamily(string subfamilyName)
        {
            if (string.IsNullOrEmpty(subfamilyName)) return subfamilyName;
            return _overrides.Subfamilies.TryGetValue(subfamilyName, out var translated) ? translated : subfamilyName;
        }

        /// <summary>
        /// Traduce un texto de carrusel de BC al idioma actual.
        /// </summary>
        public string TranslateCarousel(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return _overrides.Carousel.TryGetValue(text, out var translated) ? translated : text;
        }

        /// <summary>
        /// Traduce una descripción de promoción de BC al idioma actual.
        /// Primero busca coincidencia exacta en "promos", si no la encuentra
        /// aplica reemplazos por fragmentos desde "words".
        /// </summary>
        public string TranslatePromo(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            // 1. Coincidencia exacta
            if (_overrides.Promos.TryGetValue(text, out var exact)) return exact;
            // 2. Reemplazo por palabras/fragmentos
            return ApplyWordReplacements(text);
        }

        /// <summary>
        /// Traduce un nombre de producto de BC al idioma actual.
        /// Primero busca coincidencia exacta en "products", si no aplica words.
        /// </summary>
        public string TranslateProduct(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            if (_overrides.Products.TryGetValue(text, out var exact)) return exact;
            return ApplyWordReplacements(text);
        }

        /// <summary>
        /// Aplica reemplazos de fragmentos/palabras sobre un texto.
        /// Reemplaza de mayor a menor longitud para evitar conflictos parciales.
        /// </summary>
        private string ApplyWordReplacements(string text)
        {
            if (_overrides.Words.Count == 0) return text;
            var result = text;
            // Ordenar por longitud descendente para reemplazar primero los fragmentos más largos
            foreach (var kvp in _overrides.Words.OrderByDescending(k => k.Key.Length))
            {
                if (result.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                {
                    result = ReplacePreservingCase(result, kvp.Key, kvp.Value);
                }
            }
            return result;
        }

        private static string ReplacePreservingCase(string input, string search, string replacement)
        {
            int index = input.IndexOf(search, StringComparison.OrdinalIgnoreCase);
            while (index >= 0)
            {
                input = input.Substring(0, index) + replacement + input.Substring(index + search.Length);
                index = input.IndexOf(search, index + replacement.Length, StringComparison.OrdinalIgnoreCase);
            }
            return input;
        }

        /// <summary>
        /// Obtiene la traducción para una clave. Si no existe, devuelve la clave.
        /// </summary>
        public string T(string key)
        {
            if (string.IsNullOrEmpty(key))
                return key;

            return _translations.TryGetValue(key, out var value) ? value : key;
        }

        /// <summary>
        /// Obtiene la traducción con parámetros de formato.
        /// Ejemplo: T("welcome_message", "Juan") donde welcome_message = "Bienvenido, {0}"
        /// </summary>
        public string T(string key, params object[] args)
        {
            var translation = T(key);
            try
            {
                return string.Format(translation, args);
            }
            catch
            {
                return translation;
            }
        }

        public LanguageInfo GetCurrentLanguageInfo()
        {
            return SupportedLanguages[_currentLanguage];
        }
    }

    public record LanguageInfo(string Name, string Code, string Flag);
}
