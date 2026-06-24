using Microsoft.EntityFrameworkCore;
using WasteGlassAPI.Data;
using WasteGlassAPI.Models;
using WasteGlassAPI.DTOs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

app.MapPost("/api/collections",
async (CollectionRequest request, WasteGlassDbContext db) =>
{
    var supplier = await db.Suppliers.FindAsync(request.SupplierId);

    if (supplier == null)
        return Results.NotFound("Supplier not found");

    var record = new CollectionRecord
    {
        SupplierId = request.SupplierId,
        ClearGlassKg = request.ClearGlassKg,
        ColouredGlassKg = request.ColouredGlassKg,
        Condition = request.Condition,
        Timestamp = DateTime.UtcNow
    };

    supplier.Status = "Collected";

    db.CollectionRecords.Add(record);

    await db.SaveChangesAsync();

    return Results.Ok(record);
});

// barcode validation endpoint
app.MapGet("/api/suppliers/validate/{supplierCode}",
async (string supplierCode, WasteGlassDbContext db) =>
{
    var supplier = await db.Suppliers
        .FirstOrDefaultAsync(s => s.SupplierCode == supplierCode);

    if (supplier == null)
        return Results.NotFound();

    return Results.Ok(supplier);
    
});

app.MapGet("/api/suppliers/{id}",
async (int id, WasteGlassDbContext db) =>
{
    var supplier = await db.Suppliers.FindAsync(id);

    return supplier == null
        ? Results.NotFound()
        : Results.Ok(supplier);
});

// This endpoint is for validating the supplier code (barcode) and retrieving supplier details. It will be used by the collection team to verify the supplier before collection.
app.MapGet("/api/suppliers/validate/{code}",
async (string code, WasteGlassDbContext db) =>
{
    var supplier = await db.Suppliers
        .FirstOrDefaultAsync(s => s.SupplierCode == code);

    if (supplier == null)
        return Results.NotFound(new { message = "Invalid barcode" });

    return Results.Ok(new
    {
        supplier.Id,
        supplier.SupplierCode,
        supplier.Name,
        supplier.Status
    });
});

app.Run();

//Harversine function to calculate distance between two lat/lon points
static double GetDistance(double lat1, double lon1, double lat2, double lon2)
{
    var R = 6371;

    var dLat = (lat2 - lat1) * Math.PI / 180;
    var dLon = (lon2 - lon1) * Math.PI / 180;

    var a =
        Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
        Math.Cos(lat1 * Math.PI / 180) *
        Math.Cos(lat2 * Math.PI / 180) *
        Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

    var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

    return R * c;
}


record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
