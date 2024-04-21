using GrowBox.Abstractions;
using GrowBox.Server;
using GrowBox.Server.Services;

var builder = WebApplication.CreateBuilder(args);
var serverConfiguration = new ServerConfiguration();
builder.Configuration.GetSection("Configuration").Bind(serverConfiguration);

// Add services to the container.
var services = builder.Services;
services.AddHttpClient("GrowBox.ServerAPI");
services.AddScoped(sp => {
    var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient("GrowBox.ServerAPI");
    return client;
});
services.AddSingleton(serverConfiguration);
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
services.AddEndpointDefinitions(typeof(Program));
services.AddHostedService<RetentionService>();

services.AddScoped<GrowBoxContext>();

var app = builder.Build();
await app.UseGrowBoxContext();
app.UseBlazorFrameworkFiles();
app.MapFallbackToFile("index.html");
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseWebAssemblyDebugging();
} 

app.UseHttpsRedirection();
app.UseEndpointDefinitions();

    // .WithName("GetWeatherForecast")
    // .WithOpenApi();

app.Run();