using Blazor.MinimalApi.Client;
using GrowBox.Controls.Overlay;
using GrowBox.Navigation;
using GrowBox.Services;
using Blazored.LocalStorage;
using GrowBox;
using GrowBox.State;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;
using Refit;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<AppComponent>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var services = builder.Services;
services.AddScoped<IClipboardService, ClipboardService>();
services.AddHttpClient("GrowBox.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
services.AddMinimalApiClient();
services.AddScoped<App>();

// Supply HttpClient instances that include access tokens when making requests to the server project
services.AddScoped(sp => {
    var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient("GrowBox.ServerAPI");
    return client;
});

// 3rd party
services.AddBlazoredLocalStorage();
services.AddRadzenComponents();
services.AddRefitClient<IWaterPumpsEsp>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://192.168.178.194"));

// My
services.AddScoped(typeof(SimpleStorage<>));
services.AddScoped(typeof(SimpleStorage<,>));
services.AddScoped<IOverlayService, OverlayService>();
services.AddScoped<INavigationPanelService, NavigationPanelService>();
services.AddScoped<GrowBoxRepositoryFactory>();
services.AddScoped<MyJsInterop>();

var host = builder.Build();
var app = host.Services.GetRequiredService<App>();
await app.Startup();

await host.RunAsync();
