using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Syncfusion.Blazor;
using TentaloWebShop;
using TentaloWebShop.Models;
using TentaloWebShop.Services;
using WeiderShop.Services;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

//Register Syncfusion license
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JEaF5cXmRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXhfeHRWRmNYWUF1XURWYEE=");

var builder = WebAssemblyHostBuilder.CreateDefault(args);
 

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
var httpCfg = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };

using var cfgMain = await httpCfg.GetStreamAsync("appsettings.json");
builder.Configuration.AddJsonStream(cfgMain);

try
{
    using var cfgEnv = await httpCfg.GetStreamAsync($"appsettings.{builder.HostEnvironment.Environment}.json");
    builder.Configuration.AddJsonStream(cfgEnv);
}
catch { /* si no existe, seguimos */ }

// 2) Enlaza la sección ApiSettings a la clase tipada
// Program.cs (después de cargar el appsettings.json en WASM si aplica)



var section = builder.Configuration.GetSection("ApiSettings");

builder.Services
    .AddOptions<ApiSettings>()
    .Configure(o => section.Bind(o))                   // ← en vez de .Bind(section)
    .Validate(o => !string.IsNullOrWhiteSpace(o.Url), "ApiSettings.Url requerido")
    .ValidateOnStart();


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
builder.Services.AddSyncfusionBlazor();
await builder.Build().RunAsync();
