using Grpc.Core;
using HasznaltAuto.Entities;
using Microsoft.EntityFrameworkCore;

namespace HasznaltAuto.API.Services;

public class HasznaltAutoService(
    HasznaltAutoDbContext hasznaltAutoDbContext,
    BaseService baseService,
    ILogger<HasznaltAutoService> logger) 
    : HasznaltAutoGrpc.HasznaltAutoGrpcBase
{
    public override async Task ListCars(Empty request, IServerStreamWriter<CarType> responseStream, ServerCallContext context)
    {
        if (await hasznaltAutoDbContext.Cars.AnyAsync(car => !car.IsDeleted))
        {
            foreach (var carEntity in hasznaltAutoDbContext.Cars.Where(car => !car.IsDeleted))
            {
                await responseStream.WriteAsync(MapToProtobufCar(carEntity));
            }
        }
    }

    public override async Task ListCarsFiltered(ListCarsFilteredRequest request, IServerStreamWriter<CarType> responseStream, ServerCallContext context)
    {
        IQueryable<Car> storedUserCars = hasznaltAutoDbContext.Cars.Where(car => !car.IsDeleted);

        if (string.IsNullOrEmpty(request.FuelType) == false)
        {
            // TODO AB check hogy kell-e include
            // TODO AB API-n megadni a lehetseges ertekeket
            storedUserCars = storedUserCars.Where(car => car.FuelType.Equals(request.FuelType));
        }

        if (string.IsNullOrEmpty(request.Make) == false)
        {
            storedUserCars = storedUserCars.Where(car => car.Make.Equals(request.Make));
        }

        if (request.MileageMax > 0)
        {
            storedUserCars = storedUserCars.Where(car => car.Mileage <= request.MileageMax);
        }

        if (request.MileageMin > 0)
        {
            storedUserCars = storedUserCars.Where(car => car.Mileage >= request.MileageMin);
        }

        if (string.IsNullOrEmpty(request.Model) == false)
        {
            storedUserCars = storedUserCars.Where(car => car.Model.Name.Equals(request.Model));
        }

        if (request.PriceMax > 0)
        {
            storedUserCars = storedUserCars.Where(car => car.Price <= request.PriceMax);
        }

        if (request.PriceMin > 0)
        {
            storedUserCars = storedUserCars.Where(car => car.Price >= request.PriceMin);
        }

        if (string.IsNullOrEmpty(request.ProductionDateMax) == false)
        {
            storedUserCars = storedUserCars.Where(car => Convert.ToDateTime(car.ProductionDate) <= Convert.ToDateTime(request.ProductionDateMax));
        }

        if (string.IsNullOrEmpty(request.ProductionDateMin) == false)
        {
            storedUserCars = storedUserCars.Where(car => Convert.ToDateTime(car.ProductionDate) >= Convert.ToDateTime(request.ProductionDateMin));
        }

        List<Car> carsFiltered = await storedUserCars.ToListAsync(context.CancellationToken);
        foreach (var carEntity in carsFiltered)
        {
            await responseStream.WriteAsync(MapToProtobufCar(carEntity));
        }
    }

    public override async Task<CarType> GetCar(GetCarRequest request, ServerCallContext context)
    {
        var car = await hasznaltAutoDbContext.Cars.FindAsync(request.CarId);
        if (car is null)
        {
            return new CarType();
        }

        return MapToProtobufCar(car);
    }

    public override async Task<ResultResponse> CreateCar(CreateCarRequest request, ServerCallContext context)
    {
        if (request?.SessionId is null || !baseService._sessionList.Contains(request.SessionId))
        {
            return await baseService.RequestFailed("Unauthorized access.");
        }

        if (request is null || request?.Car is null)
        {
            return await baseService.RequestFailed("Empty create car request.");
        }

        await hasznaltAutoDbContext.Cars.AddAsync(MapToEntityCar(request.Car));
        await hasznaltAutoDbContext.SaveChangesAsync();
        return await baseService.RequestSuccessful("Car created");
    }

    public override async Task<ResultResponse> UpdateCar(UpdateCarRequest request, ServerCallContext context)
    {
        if (request?.SessionId is null || !baseService._sessionList.Contains(request.SessionId))
        {
            return await baseService.RequestFailed("Unauthorized access.");
        }

        if (request is null || request.Car is null)
        {
            return await baseService.RequestFailed("Empty car update request details.");
        }

        var carToUpdate = await hasznaltAutoDbContext.Cars.FindAsync(request.Car.Id);
        if (carToUpdate == null)
        {
            return await baseService.RequestFailed("Car not found.");
        }

        //carToUpdate.FuelType = (int)request.Car.FuelType;
        //carToUpdate.Kilometres = request.Car.Kilometres;
        //carToUpdate.LicensePlate = request.Car.LicensePlate;
        //carToUpdate.Make = request.Car.Make;
        //carToUpdate.Model = request.Car.Model;
        //carToUpdate.PreviousOwners = request.Car.PreviousOwners;
        carToUpdate.Price = request.Car.Price;
        await hasznaltAutoDbContext.SaveChangesAsync();
        return await baseService.RequestSuccessful("Car updated.");
    }

    public override async Task<ResultResponse> DeleteCar(DeleteCarRequest request, ServerCallContext context)
    {
        // TODO AB ez mehet basebe, repetitiv
        if (request?.SessionId is null || !baseService._sessionList.Contains(request.SessionId))
        {
            return await baseService.RequestFailed("Unauthorized access.");
        }

        if (request is null || request.CurrentUser < 0 || request.CarId < 0)
        {
            return await baseService.RequestFailed("Empty delete car request details.");
        }

        var carToDelete = await hasznaltAutoDbContext.Cars.FindAsync(request.CarId);
        if (carToDelete is null)
        {
            return await baseService.RequestFailed("Car not found.");
        }

        if (carToDelete.CurrentOwner != request.CurrentUser)
        {
            return await baseService.RequestFailed("Car is not yours.");
        }

        carToDelete.IsDeleted = true;
        await hasznaltAutoDbContext.SaveChangesAsync();
        return await baseService.RequestSuccessful("Car deleted.");
    }

    public override async Task<ResultResponse> BuyCar(BuyCarRequest request, ServerCallContext context)
    {
        if (request?.SessionId is null || !baseService._sessionList.Contains(request.SessionId))
        {
            return await baseService.RequestFailed("Unauthorized access.");
        }

        if (request is null || request.CurrentUser < 0 || request.CarId < 0)
        {
            return await baseService.RequestFailed("Empty buy car request details.");
        }

        var carToBuy = await hasznaltAutoDbContext.Cars.FindAsync(request.CarId);
        if (carToBuy is null)
        {
            return await baseService.RequestFailed("Car not found.");
        }

        if (carToBuy.CurrentOwner == request.CurrentUser)
        {
            return await baseService.RequestFailed("Car is already yours.");
        }

        carToBuy.CurrentOwner = request.CurrentUser;
        //carToBuy.PreviousOwners++;
        await hasznaltAutoDbContext.SaveChangesAsync();
        return await baseService.RequestSuccessful("Car bought.");
    }

    #region Private Methods

    private static CarType MapToProtobufCar(Car car)
    {
        return new CarType
        {
            Id = car.Id,
            CurrentOwner = car.CurrentOwner,
            FuelTypeId = car.FuelTypeId,
            IsDeleted = car.IsDeleted,
            MakeId = car.MakeId,
            Mileage = car.Mileage,
            ModelId = car.ModelId,
            Price = car.Price,
            ProductionDate = car.ProductionDate,
            VehicleRegistrationId = car.VehicleRegistrationId ?? 0,
        };
    }

    private static Car MapToEntityCar(CarType carType)
    {
        return new Car
        {
            Id = carType.Id,
            CurrentOwner = carType.CurrentOwner,
            FuelTypeId = carType.FuelTypeId,
            IsDeleted = carType.IsDeleted,
            MakeId = carType.MakeId,
            Mileage = carType.Mileage,
            ModelId = carType.ModelId,
            Price = carType.Price,
            ProductionDate = carType.ProductionDate,
            VehicleRegistrationId = carType.VehicleRegistrationId,
        };
    }

    #endregion
}