using HasznaltAuto.API.Entities;
using HasznaltAuto.API.Repositories;
using HasznaltAuto.API.REST.Services;
using Microsoft.EntityFrameworkCore;

namespace HasznaltAuto.Tests
{
    public class CarRepositoryTests
    {
        private readonly HasznaltAutoDbContext _dbContext;
        private readonly CarRepository _carRepository;
        private readonly CarRestService _carRestService; // TODO car rest service tests?

        public CarRepositoryTests()
        {
            _dbContext = new HasznaltAutoDbContext(GetDbContextOptions());
            _carRepository = new CarRepository(_dbContext);
            //_carRestService = new CarRestService(_carRepository);
        }

        [Fact(DisplayName = "[GET] Existing car")]
        public async Task Get_Should_GetCarById_ReturnActualEntity()
        {
            Car? car = await _carRepository.GetByIdAsync(1);

            Assert.NotNull(car);
        }

        [Fact(DisplayName = "[GET] Non existent car")]
        public async Task Get_NonExistentCarId_ShouldNotReturnEntity()
        {
            Car? car = await _carRepository.GetByIdAsync(99999);

            Assert.Null(car);
        }

        [Fact(DisplayName = "[GET] All cars")]
        public async Task GetAllCars_Should_ReturnMultipleCars()
        {
            IEnumerable<Car> cars = await _carRepository.GetAllAsync();

            Assert.True(cars.Any() && cars.Count() > 1);
        }

        [Fact(DisplayName = "[GET] All cars but deleted")]
        public async Task GetAllCars_ShouldNot_ReturnDeletedCars()
        {
            IEnumerable<Car> cars = await _carRepository.GetAllAsync();

            Assert.True(cars.All(car => !car.IsDeleted));
        }

        [Fact(DisplayName = "[CREATE] Return new car id", Skip = "skipping test: do not create a car every test run.")]
        public async Task Should_ReturnNewId_WhenNewCarInserted()
        {
            int newCarId = await _carRepository.AddAsync(NewCar());

            Assert.True(newCarId > 0);
        }

        [Fact(DisplayName = "[CREATE] Get new car id", Skip = "skipping test: do not create a car every test run.")]
        public async Task Car_ShouldNotBeNull_When_Retrieving_After_Insert()
        {
            int newCarId = await _carRepository.AddAsync(NewCar());

            var freshlyInsertedCar = await _carRepository.GetByIdAsync(newCarId);

            Assert.NotNull(freshlyInsertedCar);
        }

        [Fact(DisplayName = "[UPDATE] Price")]
        public async Task CarPrice_Should_BeUpdated()
        {
            Car? originalPriceCar = await _carRepository.GetByIdAsync(1);

            Assert.NotNull(originalPriceCar);

            int originalPrice = originalPriceCar.Price;
            int newPrice = originalPrice + 1000;
            
            originalPriceCar.Price = newPrice;
            await _carRepository.UpdateAsync(originalPriceCar);

            Car? newPriceCar = await _carRepository.GetByIdAsync(1);

            Assert.NotNull(newPriceCar);

            Assert.Equal(originalPriceCar.Price, newPriceCar.Price);
        }

        [Fact(DisplayName = "[DELETE] Car")]
        public async Task Car_Should_BeDeleted()
        {
            int newCarId = await _carRepository.AddAsync(NewCar());
            await _carRepository.DeleteAsync(newCarId);

            Car? deletedCar = await _carRepository.GetByIdAsync(newCarId);

            Assert.Null(deletedCar); // Get methods do not return deleted entities
        }

        private static DbContextOptions<HasznaltAutoDbContext> GetDbContextOptions()
        {
            return new DbContextOptionsBuilder<HasznaltAutoDbContext>()
                .UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=HasznaltAutoTestDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;")
                .Options;
        }

        private static Car NewCar()
        {
            return new()
            {
                CurrentOwner = 1,
                FuelTypeId = 1,
                MakeId = 1,
                Mileage = 15000,
                ModelId = 1,
                Price = 1_000_000,
                ProductionDate = "2005/3",
                VehicleRegistration = new()
                {
                    LicensePlate = "TEST-123",
                    LicensePlateTypeId = 1
                }
            };
        }
    }
}