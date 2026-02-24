using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Syncfusion.Blazor;
using TentaloWebShop;
using TentaloWebShop.Models;
using TentaloWebShop.Services;
using WeiderShop.Services;

//Register Syncfusion license
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JEaF5cXmRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXhfeHRWRmNYWUF1XURWYEE=");

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient base para la aplicación
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Cargar configuración
var httpCfg = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
using var cfgMain = await httpCfg.GetStreamAsync("appsettings.json");
builder.Configuration.AddJsonStream(cfgMain);

try
{
    using var cfgEnv = await httpCfg.GetStreamAsync($"appsettings.{builder.HostEnvironment.Environment}.json");
    builder.Configuration.AddJsonStream(cfgEnv);
}
catch { /* si no existe, seguimos */ }

// Configurar ApiSettings
var section = builder.Configuration.GetSection("ApiSettings");
builder.Services
    .AddOptions<ApiSettings>()
    .Configure(o => section.Bind(o))
    .Validate(o => !string.IsNullOrWhiteSpace(o.Url), "ApiSettings.Url requerido")
    .ValidateOnStart();

// ✅ SOLUCIÓN: Registrar SimpleApiService (ya no necesita HttpClient inyectado)
builder.Services.AddScoped<SimpleApiService>();
builder.Services.AddScoped<IApiService>(sp => sp.GetRequiredService<SimpleApiService>());

// Servicios existentes
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<RestDataService>();
builder.Services.AddScoped<BusyService>();
builder.Services.AddScoped<EstadisticasService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<InvoiceService>();
builder.Services.AddScoped<ClientSelectionService>(); // ← NUEVO
builder.Services.AddScoped<LoadingService>();
builder.Services.AddScoped<ShippingAddressService>();
builder.Services.AddScoped<CarouselService>();
builder.Services.AddScoped<PromoService>();
builder.Services.AddScoped<LocalizationService>();
builder.Services.AddScoped<ApprovalService>(); // Servicio de aprobaciones de atletas via WhatsApp
builder.Services.AddSyncfusionBlazor();

await builder.Build().RunAsync();