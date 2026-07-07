using HotelStay.Blazor;
using HotelStay.Blazor.Configuration;
using HotelStay.Blazor.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiSettings = new ApiSettings
{
    BaseUrl = builder.Configuration[$"{nameof(ApiSettings)}:{nameof(ApiSettings.BaseUrl)}"] ?? string.Empty
};

if (string.IsNullOrWhiteSpace(apiSettings.BaseUrl))
{
    throw new InvalidOperationException("ApiSettings:BaseUrl must be configured.");
}

builder.Services.AddSingleton(apiSettings);
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiSettings.BaseUrl, UriKind.Absolute) });
builder.Services.AddScoped<IApiClient, ApiClient>();
builder.Services.AddScoped<IHotelApiService, HotelApiService>();

await builder.Build().RunAsync();
