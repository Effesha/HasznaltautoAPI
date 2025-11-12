using HasznaltAuto.API.Entities;
using HasznaltAuto.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HasznaltAuto.API.Repositories
{
    public class CarRepository(HasznaltAutoDbContext dbContext) : IRepository<Car>
    {
        public async Task<int> AddAsync(Car entity, CancellationToken ct = default)
        {
            var result = await dbContext.Cars.AddAsync(entity, ct);
            await dbContext.SaveChangesAsync(ct);
            return result.Entity.Id;
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var carToSoftDelete = await dbContext.Cars.FindAsync(id, ct);
            if (carToSoftDelete is null) return;
            carToSoftDelete.IsDeleted = true;
            await dbContext.SaveChangesAsync(ct);
        }

        public async Task<IEnumerable<Car>> GetAllAsync(CancellationToken ct = default)
        {
            return await dbContext.Cars
                .IncludeAllNavProps()
                .Where(car => !car.IsDeleted)
                .ToListAsync(ct);
        }

        public async Task<Car?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await dbContext.Cars
                .IncludeAllNavProps()
                .Where(car => !car.IsDeleted)
                .FirstOrDefaultAsync(car => car.Id == id, ct);
        }

        public async Task UpdateAsync(Car updatedCar, CancellationToken ct = default)
        {
            dbContext.Update(updatedCar);
            await dbContext.SaveChangesAsync(ct);
        }
    }

    public static class CarRepositoryExtensions
    {
        public static IQueryable<Car> IncludeAllNavProps(this IQueryable<Car> query)
        {
            return query
                .Include(i => i.CurrentOwnerNavigation)
                .Include(i => i.FuelType)
                .Include(i => i.Make)
                .Include(i => i.Model)
                .Include(i => i.VehicleRegistration);
        }
    }
}
