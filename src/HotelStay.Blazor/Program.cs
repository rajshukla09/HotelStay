using HotelStay.Blazor;
using HotelStay.Blazor.Configuration;
using HotelStay.Blazor.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Options;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection(nameof(ApiSettings)));
builder.Services.AddScoped(sp =>
{
    var apiSettings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;
    if (string.IsNullOrWhiteSpace(apiSettings.BaseUrl))
    {
        throw new InvalidOperationException("ApiSettings:BaseUrl must be configured.");
    }

    return new HttpClient { BaseAddress = new Uri(apiSettings.BaseUrl, UriKind.Absolute) };
});
builder.Services.AddScoped<IApiClient, ApiClient>();
builder.Services.AddScoped<IHotelApiService, HotelApiService>();

await builder.Build().RunAsync();
