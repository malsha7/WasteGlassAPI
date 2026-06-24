using Microsoft.EntityFrameworkCore;
using WasteGlassAPI.Data;
using WasteGlassAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<WasteGlassDbContext>(options =>
    options.UseSqlite("Data Source=wasteglass.db"));


var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WasteGlassDbContext>();

    db.Database.EnsureCreated();

    if (!db.Suppliers.Any())
    {
        db.Suppliers.AddRange(
            new Supplier
            {
                SupplierCode = "SUP001",
                Name = "Supplier A",
                Latitude = 6.9271,
                Longitude = 79.8612,
                ExpectedQuantityKg = 100
            },
            new Supplier
            {
                SupplierCode = "SUP002",
                Name = "Supplier B",
                Latitude = 6.9340,
                Longitude = 79.8500,
                ExpectedQuantityKg = 150
            }
        );

        db.SaveChanges();
    }
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapGet("/api/suppliers", async (WasteGlassDbContext db) =>
{
    return await db.Suppliers.ToListAsync();
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
