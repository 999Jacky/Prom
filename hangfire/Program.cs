using Hangfire;
using hangfire.Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseInMemoryStorage()
);
builder.Services.AddHangfireServer();
builder.Services.Configure<HangFireConst>(
    builder.Configuration.GetSection(HangFireConst.ConfigureName)
);
builder.Services.AddSingleton<OnHangFireSettingChange>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


// var summaries = new[] {
//     "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
// };
//
// app.MapGet("/weatherforecast", () => {
//         var forecast = Enumerable.Range(1, 5).Select(index =>
//                 new WeatherForecast
//                 (
//                     DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//                     Random.Shared.Next(-20, 55),
//                     summaries[Random.Shared.Next(summaries.Length)]
//                 ))
//             .ToArray();
//         return forecast;
//     })
//     .WithName("GetWeatherForecast")
//     .WithOpenApi();
app.UseHangfireDashboard();
// var backgroundJobs = app.Services.GetService<BackgroundJobClient>();
// backgroundJobs.Enqueue(() => Console.WriteLine("Hello world from Hangfire!"));
app.Services.GetService<OnHangFireSettingChange>();
app.Run();
//
// record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary) {
//     public int TemperatureF => 32 + (int) (TemperatureC / 0.5556);
// }
