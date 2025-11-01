using HasznaltAuto.API.Entities;

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

public static class CarMapper
{
    public static CarType? MapToCarType(this CarDto carDto, int currentUser, int vehicleRegistrationId)
    {
        if (carDto == null) return null;

        var carType = new CarType()
        {
            CurrentOwner = currentUser,
            FuelTypeId = carDto.FuelTypeId,
            MakeId = carDto.MakeId,
            ModelId = carDto.ModelId,
            Price = carDto.Price,
            Mileage = carDto.Mileage,
            ProductionDate = carDto.ProductionDate,
            VehicleRegistrationId = vehicleRegistrationId,
        };

        return carType;
    }

    public static CarType? MapToCarType(this CarDto carDto, int currentUser)
    {
        if (carDto == null) return null;

        var carType = new CarType()
        {
            CurrentOwner = currentUser,
            FuelTypeId = carDto.FuelTypeId,
            MakeId = carDto.MakeId,
            ModelId = carDto.ModelId,
            Price = carDto.Price,
            Mileage = carDto.Mileage,
            ProductionDate = carDto.ProductionDate,
            VehicleRegistrationId = carDto.VehicleRegistrationId
        };

        return carType;
    }

    public static CarDto MapToCarDto(this CarType carType, List<ListUsersResponse> users, List<FuelType> fuelTypes, List<MakeType> makes, List<ModelType> models, List<VehicleRegistrationType> regTypes)
    {
        return new CarDto()
        {
            Id = carType.Id,
            CurrentOwner = carType.CurrentOwner,
            CurrentOwnerName = users.FirstOrDefault(u => u.Id == carType.CurrentOwner)?.Name,
            FuelTypeId = carType.FuelTypeId,
            FuelTypeName = fuelTypes.FirstOrDefault(ft => ft.Id == carType.FuelTypeId)?.Name,
            MakeId = carType.MakeId,
            MakeName = makes.FirstOrDefault(m => m.Id == carType.MakeId)?.Name,
            Mileage = carType.Mileage,
            ModelId = carType.ModelId,
            ModelName = models.FirstOrDefault(m => m.Id == carType.ModelId)?.Name,
            Price = carType.Price,
            ProductionDate = carType.ProductionDate,
            VehicleRegistrationDto = new()
            {
                LicensePlate = regTypes.FirstOrDefault(vr => vr.Id == carType.VehicleRegistrationId)?.LicensePlate
            }
        };
    }
}
