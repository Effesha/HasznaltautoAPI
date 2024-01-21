using Grpc.Core;
using HasznaltAuto.Entities;
using Microsoft.EntityFrameworkCore;

namespace HasznaltAuto.Services
{
    public class HasznaltAutoService : HasznaltAuto.HasznaltAutoBase
    {
        private readonly ILogger<HasznaltAutoService> _logger;
        private readonly HasznaltAutoDbContext _hasznaltAutoDbContext;
        private static readonly List<string> _sessionList = new();

        public HasznaltAutoService(
            ILogger<HasznaltAutoService> logger,
            HasznaltAutoDbContext hasznaltAutoDbContext
            )
        {
            _logger = logger;
            _hasznaltAutoDbContext = hasznaltAutoDbContext;
        }

        public override async Task<ResultResponse> Register(RegistrationRequest request, ServerCallContext context)
        {
            if (request is null || string.IsNullOrWhiteSpace(request?.Name) || string.IsNullOrWhiteSpace(request?.Password))
            {
                return await RequestFailed("Empty username or password");
            }

            var usernameTaken = await _hasznaltAutoDbContext.Users.AnyAsync(user => user.Name == request.Name);
            if (usernameTaken)
            {
                return await Task.FromResult(new ResultResponse
                {
                    Success = false,
                    // Biztonsági okokból nem a legjobb megoldás, hiszen így információt adunk arról, hogy létezik ilyen user az adatbázisban
                    Message = "Username taken."
                });
            }

            var newUserToAdd = new User
            {
                Name = request.Name,
                Password = request.Password
            };

            await _hasznaltAutoDbContext.Users.AddAsync(newUserToAdd);
            await _hasznaltAutoDbContext.SaveChangesAsync();
            return await RequestSuccessful("Account created");
        }

        public override async Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
        {
            if (request is null || string.IsNullOrWhiteSpace(request?.Name) || string.IsNullOrWhiteSpace(request?.Password))
            {
                return await Task.FromResult(new LoginResponse
                {
                    SessionId = string.Empty,
                    CurrentUser = 0,
                    Message = "Empty username or password."
                });
            }

            var userToLogin = await _hasznaltAutoDbContext.Users.FirstOrDefaultAsync(user => user.Name == request.Name);
            if (userToLogin is null)
            {
                return await Task.FromResult(new LoginResponse
                {
                    SessionId = string.Empty,
                    CurrentUser = 0,
                    Message = "User not found."
                });
            }

            var guid = Guid.NewGuid().ToString();
            lock (_sessionList)
            {
                _sessionList.Add(guid);
            }

            return await Task.FromResult(new LoginResponse
            {
                SessionId = guid,
                CurrentUser = userToLogin.Id,
                Message = "Login successful!"
            });
        }

        public override async Task<ResultResponse> Logout(LogoutRequest request, ServerCallContext context)
        {
            if (request is null || string.IsNullOrWhiteSpace(request?.SessionId))
            {
                return await RequestFailed("You are not logged in.");
            }

            lock (_sessionList)
            {
                if (_sessionList.Contains(request.SessionId))
                {
                    _sessionList.Remove(request.SessionId);
                }
            }

            return await RequestSuccessful("Logout successful");
        }

        public override async Task ListCars(Empty request, IServerStreamWriter<Car> responseStream, ServerCallContext context)
        {
            if (await _hasznaltAutoDbContext.Cars.AnyAsync(car => !car.IsDeleted))
            {
                foreach (var carEntity in _hasznaltAutoDbContext.Cars.Where(car => !car.IsDeleted))
                {
                    await responseStream.WriteAsync(MapToProtobufCar(carEntity));
                }
            }
        }

        public override async Task ListCarsFiltered(ListCarsFilteredRequest request, IServerStreamWriter<Car> responseStream, ServerCallContext context)
        {
            if (request.CurrentUser < 0 &&
                await _hasznaltAutoDbContext.Cars.AnyAsync(car => car.CurrentOwner == request.CurrentUser))
            {
                var carsFiltered = await _hasznaltAutoDbContext.Cars.Where(car => car.CurrentOwner == request.CurrentUser)
                                                                    .ToListAsync();

                foreach (var carEntity in carsFiltered)
                {
                    await responseStream.WriteAsync(MapToProtobufCar(carEntity));
                }
            }

            if (request.FuelType is not null &&
                await _hasznaltAutoDbContext.Cars.AnyAsync(car => car.FuelType == int.Parse(request.FuelType)))
            {
                var carsFiltered = await _hasznaltAutoDbContext.Cars.Where(car => car.FuelType == int.Parse(request.FuelType))
                                                                    .ToListAsync();

                foreach (var carEntity in carsFiltered)
                {
                    await responseStream.WriteAsync(MapToProtobufCar(carEntity));
                }
            }
        }

        public override async Task<Car> GetCar(GetCarRequest request, ServerCallContext context)
        {
            var car = await _hasznaltAutoDbContext.Cars.FindAsync(request.CarId);
            if (car is null)
            {
                return new Car();
            }

            return MapToProtobufCar(car);
        }

        public override async Task<ResultResponse> CreateCar(CreateCarRequest request, ServerCallContext context)
        {
            if (request?.SessionId is null || !_sessionList.Contains(request.SessionId))
            {
                return await RequestFailed("Unauthorized access.");
            }

            if (request is null || request?.Car is null)
            {
                return await RequestFailed("Empty create car request.");
            }

            await _hasznaltAutoDbContext.Cars.AddAsync(MapToEntityCar(request.Car));
            await _hasznaltAutoDbContext.SaveChangesAsync();
            return await RequestSuccessful("Car created");
        }

        public override async Task<ResultResponse> UpdateCar(UpdateCarRequest request, ServerCallContext context)
        {
            if (request?.SessionId is null || !_sessionList.Contains(request.SessionId))
            {
                return await RequestFailed("Unauthorized access.");
            }

            if (request is null || request.Car is null)
            {
                return await RequestFailed("Empty car update request details.");
            }

            var carToUpdate = await _hasznaltAutoDbContext.Cars.FindAsync(request.Car.Id);
            if (carToUpdate == null)
            {
                return await RequestFailed("Car not found.");
            }

            carToUpdate.FuelType = (int)request.Car.FuelType;
            carToUpdate.Kilometres = request.Car.Kilometres;
            carToUpdate.LicensePlate = request.Car.LicensePlate;
            carToUpdate.Make = request.Car.Make;
            carToUpdate.Model = request.Car.Model;
            carToUpdate.PreviousOwners = request.Car.PreviousOwners;
            carToUpdate.Price = request.Car.Price;
            await _hasznaltAutoDbContext.SaveChangesAsync();
            return await RequestSuccessful("Car updated.");
        }

        public override async Task<ResultResponse> DeleteCar(DeleteCarRequest request, ServerCallContext context)
        {
            if (request?.SessionId is null || !_sessionList.Contains(request.SessionId))
            {
                return await RequestFailed("Unauthorized access.");
            }

            if (request is null || request.CurrentUser < 0 || request.CarId < 0)
            {
                return await RequestFailed("Empty delete car request details.");
            }

            var carToDelete = await _hasznaltAutoDbContext.Cars.FindAsync(request.CarId);
            if (carToDelete is null)
            {
                return await RequestFailed("Car not found.");
            }

            if (carToDelete.CurrentOwner != request.CurrentUser)
            {
                return await RequestFailed("Car is not yours.");
            }

            carToDelete.IsDeleted = true;
            await _hasznaltAutoDbContext.SaveChangesAsync();
            return await RequestSuccessful("Car deleted.");
        }

        public override async Task<ResultResponse> BuyCar(BuyCarRequest request, ServerCallContext context)
        {
            if (request?.SessionId is null || !_sessionList.Contains(request.SessionId))
            {
                return await RequestFailed("Unauthorized access.");
            }

            if (request is null || request.CurrentUser < 0 || request.CarId < 0)
            {
                return await RequestFailed("Empty buy car request details.");
            }

            var carToBuy = await _hasznaltAutoDbContext.Cars.FindAsync(request.CarId);
            if (carToBuy is null)
            {
                return await RequestFailed("Car not found.");
            }

            if (carToBuy.CurrentOwner == request.CurrentUser)
            {
                return await RequestFailed("Car is already yours.");
            }

            carToBuy.CurrentOwner = request.CurrentUser;
            carToBuy.PreviousOwners++;
            await _hasznaltAutoDbContext.SaveChangesAsync();
            return await RequestSuccessful("Car bought.");
        }

        #region Private Methods

        private static async Task<ResultResponse> RequestFailed(string message)
        {
            return await Task.FromResult(new ResultResponse
            {
                Success = false,
                Message = message
            });
        }

        private static async Task<ResultResponse> RequestSuccessful(string message)
        {
            return await Task.FromResult(new ResultResponse
            {
                Success = true,
                Message = message
            });
        }

        private static Car MapToProtobufCar(Entities.Car car)
        {
            return new Car
            {
                Id = car.Id,
                CurrentOwner = car.CurrentOwner,
                FuelType = (FuelType)car.FuelType,
                IsDeleted = car.IsDeleted,
                Kilometres = car.Kilometres,
                LicensePlate = car.LicensePlate,
                Make = car.Make,
                Model = car.Model,
                PreviousOwners = car.PreviousOwners,
                Price = car.Price,
                Registration = car.Registration
            };
        }

        private static Entities.Car MapToEntityCar(Car car)
        {
            return new Entities.Car
            {
                CurrentOwner = car.CurrentOwner,
                FuelType = (int)car.FuelType,
                IsDeleted = car.IsDeleted,
                Kilometres = car.Kilometres,
                LicensePlate = car.LicensePlate,
                Make = car.Make,
                Model = car.Model,
                PreviousOwners = car.PreviousOwners,
                Price = car.Price,
                Registration = car.Registration
            };
        }

        #endregion
    }
}