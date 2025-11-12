using HasznaltAuto.API.DTOs;
using HasznaltAuto.API.Entities;
using HasznaltAuto.API.Repositories.Interfaces;
using HasznaltAuto.API.REST.Mappers;

namespace HasznaltAuto.API.REST.Services
{
    public class CarRestService(IRepository<Car> carRepository)
    {
        public async Task<IEnumerable<CarDto>> GetAll(CancellationToken ct)
        {
            var cars = await carRepository.GetAllAsync(ct);
            return cars.Select(car => car.MapToCarDto());
        }

        public async Task<CarDto> Get(int id, CancellationToken ct)
        {
            var car = await carRepository.GetByIdAsync(id, ct);
            return car?.MapToCarDto() ?? new();
        }
    }
}
