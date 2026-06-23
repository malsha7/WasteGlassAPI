using Microsoft.EntityFrameworkCore;
using WasteGlassAPI.Models;

namespace WasteGlassAPI.Data;

public class WasteGlassDbContext : DbContext
{
    public WasteGlassDbContext(DbContextOptions<WasteGlassDbContext> options)
        : base(options)
    {
    }

    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<CollectionRecord> CollectionRecords { get; set; }
}