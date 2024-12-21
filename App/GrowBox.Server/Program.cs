using GrowBox.Abstractions;
using GrowBox.Abstractions.Model.EspApi;
using GrowBox.Server;
using GrowBox.Server.Services;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;

// await TimelapseService.GenerateTimelapse(
//     @"D:\timelapsetest.mp4", 
//     Directory.GetFiles(@"Z:\GrowBox\x52kmAC4JEWu0JaENCRRRA", "*.jpeg"),
//     CancellationToken.None,
//     singleImageDuration: TimeSpan.FromSeconds(1d / 4d)
//     );

//await DiarySnapshotService.GenerateWeeklyTimelapses(@"Z:\GrowBox\x52kmAC4JEWu0JaENCRRRA", CancellationToken.None);

// var image = await new GrowBoxImageMutator().Mutate(
//     File.ReadAllBytes(@"Z:\GrowBox\x52kmAC4JEWu0JaENCRRRA\gbd-2024-12-20T19-12-18.jpeg"), 
//     DateTime.Now, 
//     new GrowBoxEspRoot("Growbox Name", 0, 189, 23.4232534d, 77.55345d, 123,123,123,12,null),
//     CancellationToken.None);
// await image.SaveAsWebpAsync(@"Z:\GrowBox\x52kmAC4JEWu0JaENCRRRA\gbd-2024-12-20T19-12-18-test.webp");

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


app.Run();