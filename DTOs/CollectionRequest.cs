namespace WasteGlassAPI.DTOs;

public class CollectionRequest
{
    public int SupplierId { get; set; }

    public double ClearGlassKg { get; set; }

    public double ColouredGlassKg { get; set; }

    public string Condition { get; set; } = string.Empty;
}