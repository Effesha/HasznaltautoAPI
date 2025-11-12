namespace HasznaltAuto.API.DTOs;

public class CarDto
{
    public int Id { get; set; }

    public int CurrentOwner { get; set; }
    public string? CurrentOwnerName { get; set; }

    public int MakeId { get; set; }
    public string? MakeName { get; set; }

    public int ModelId { get; set; }
    public string? ModelName { get; set; }

    public int FuelTypeId { get; set; }
    public string? FuelTypeName { get; set; }

    public int Price { get; set; }
    public int Mileage { get; set; }

    public string ProductionDate { get; set; } = string.Empty;

    public int VehicleRegistrationId { get; set; }
    public VehicleRegistrationDto VehicleRegistrationDto { get; set; } = new();
}

public class VehicleRegistrationDto
{
    public string? LicensePlate { get; set; }
    public LicensePlateTypeDto LicensePlateTypeDto { get; set; } = new();
}

public class LicensePlateTypeDto
{
    public string Name { get; set; } = string.Empty;
}
