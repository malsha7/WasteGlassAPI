namespace WasteGlassAPI.DTOs;

public class RouteResponseDto
{
    public double TotalDistance { get; set; }
    public List<RouteStopDto> Route { get; set; } = new();
}

public class RouteStopDto
{
    public int SupplierId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double DistanceFromPrevious { get; set; }
}