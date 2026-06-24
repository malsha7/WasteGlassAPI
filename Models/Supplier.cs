namespace WasteGlassAPI.Models;

public class Supplier
{
    public int Id { get; set; }

    // This is barcode values
    public string SupplierCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public double ExpectedKg { get; set; }

    public string Status { get; set; } = "Pending";
}