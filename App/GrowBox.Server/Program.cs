using GrowBox.Abstractions;
using GrowBox.Server;
using GrowBox.Server.Services;
using Microsoft.EntityFrameworkCore;

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
services.AddHostedService<DiarySnapshotService>();

services.AddDbContext<GrowBoxContext>(options =>
{
    options.UseNpgsql(
        serverConfiguration.PgSqlConnectionString, 
        b =>  b.MigrationsAssembly("GrowBox.Server"));
});

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