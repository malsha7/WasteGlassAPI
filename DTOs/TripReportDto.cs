namespace WasteGlassAPI.DTOs;

public class TripReportDto
{
    public int TotalSuppliers { get; set; }
    public double TotalExpectedKg { get; set; }
    public double TotalCollectedKg { get; set; }

    public List<SupplierReportDto> Suppliers { get; set; } = new();
}

public class SupplierReportDto
{
    public int SupplierId { get; set; }
    public string Name { get; set; } = string.Empty;

    public double ExpectedKg { get; set; }
    public double CollectedKg { get; set; }

    public string Status { get; set; } = string.Empty; // OK / Warning
}