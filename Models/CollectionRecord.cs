namespace WasteGlassAPI.Models;

public class CollectionRecord
{
    //This is record of collection from suppliers. It will be used to track the amount of glass collected from each supplier and the condition of the glass.
    public int Id { get; set; }

    public int SupplierId { get; set; }

    public double ClearGlassKg { get; set; }

    public double ColouredGlassKg { get; set; }

    public string Condition { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}