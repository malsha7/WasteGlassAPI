namespace WasteGlassAPI.Models;

public class CollectionRecord
{
    public int Id { get; set; }

    public int SupplierId { get; set; }

    public double ClearGlassKg { get; set; }

    public double ColouredGlassKg { get; set; }

    public string Condition { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}