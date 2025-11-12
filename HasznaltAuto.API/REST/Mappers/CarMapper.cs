using HasznaltAuto.API.DTOs;
using HasznaltAuto.API.Entities;

namespace HasznaltAuto.API.REST.Mappers
{
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

        public static CarDto MapToCarDto(this Car car)
        {
            return new CarDto()
            {
                Id = car.Id,
                CurrentOwner = car.CurrentOwnerNavigation.Id,
                CurrentOwnerName = car.CurrentOwnerNavigation.Name,
                FuelTypeId = car.FuelType.Id,
                FuelTypeName = car.FuelType.Name,
                MakeId = car.Make.Id,
                MakeName = car.Make.Name,
                Mileage = car.Mileage,
                ModelId = car.Model.Id,
                ModelName = car.Model.Name,
                Price = car.Price,
                ProductionDate = car.ProductionDate,
                VehicleRegistrationDto = new()
                {
                    LicensePlate = car.VehicleRegistration?.LicensePlate
                }
            };
        }
    }
}
