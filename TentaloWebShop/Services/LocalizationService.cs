using System.Net.Http.Json;
using Microsoft.JSInterop;

namespace TentaloWebShop.Services
{
    public class LocalizationService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        private Dictionary<string, string> _translations = new();
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
            }
            catch
            {
                // Si falla, usar español por defecto
                _currentLanguage = "es";
                await LoadTranslationsAsync("es");
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
