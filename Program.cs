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

// This endpoint calculates the optimal route for the collection team based on the suppliers' locations. It uses a simple nearest neighbor algorithm to determine the order of visits.
app.MapGet("/api/route", async (WasteGlassDbContext db) =>
{
    var suppliers = await db.Suppliers.ToListAsync();

    if (!suppliers.Any())
        return Results.NotFound("No suppliers found");

    var unvisited = new List<Supplier>(suppliers);
    var route = new List<RouteStopDto>();

    // Start Point (first supplier OR depot)
    var current = unvisited.First();
    unvisited.Remove(current);

    double totalDistance = 0;

    route.Add(new RouteStopDto
    {
        SupplierId = current.Id,
        Name = current.Name,
        Latitude = current.Latitude,
        Longitude = current.Longitude,
        DistanceFromPrevious = 0
    });

    while (unvisited.Any())
    {
        Supplier nearest = null;
        double minDistance = double.MaxValue;

        foreach (var s in unvisited)
        {
            var dist = GetDistance(
                current.Latitude, current.Longitude,
                s.Latitude, s.Longitude);

            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = s;
            }
        }

        current = nearest!;
        unvisited.Remove(current);

        totalDistance += minDistance;

        route.Add(new RouteStopDto
        {
            SupplierId = current.Id,
            Name = current.Name,
            Latitude = current.Latitude,
            Longitude = current.Longitude,
            DistanceFromPrevious = minDistance
        });
    }

    return Results.Ok(new RouteResponseDto
    {
        TotalDistance = totalDistance,
        Route = route
    });
});

// This endpoint generates a trip report summarizing the collection from all suppliers. It calculates the total expected and collected quantities, and provides a status for each supplier based on whether the collected quantity meets the expected quantity.
app.MapGet("/api/trip/report", async (WasteGlassDbContext db) =>
{
    var suppliers = await db.Suppliers.ToListAsync();
    var collections = await db.CollectionRecords.ToListAsync();

    if (!suppliers.Any())
        return Results.NotFound("No suppliers found");

    double totalExpected = 0;
    double totalCollected = 0;

    var report = new TripReportDto
    {
        TotalSuppliers = suppliers.Count
    };

    foreach (var supplier in suppliers)
    {
        var collected = collections
            .Where(c => c.SupplierId == supplier.Id)
            .Sum(c => c.ClearGlassKg + c.ColouredGlassKg);

         var expected = supplier.ExpectedQuantityKg;

        totalExpected += expected;
        totalCollected += collected;

        var status = collected < expected ? "Warning" : "OK";

        report.Suppliers.Add(new SupplierReportDto
        {
            SupplierId = supplier.Id,
            Name = supplier.Name,
            ExpectedQuantityKg = expected,
            CollectedKg = collected,
            Status = status
        });
    }

    report.TotalExpectedKg = totalExpected;
    report.TotalCollectedKg = totalCollected;

    return Results.Ok(report);
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
