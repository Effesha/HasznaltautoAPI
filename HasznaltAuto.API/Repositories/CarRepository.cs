using HasznaltAuto.API.Entities;
using HasznaltAuto.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HasznaltAuto.API.Repositories
{
    public class CarRepository(HasznaltAutoDbContext dbContext) : IRepository<Car>
    {
        public async Task AddAsync(Car entity)
        {
            await dbContext.Cars.AddAsync(entity);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var carToSoftDelete = await dbContext.Cars.FindAsync(id);
            if (carToSoftDelete is null)  return;
            carToSoftDelete.IsDeleted = true;
            await dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Car>> GetAllAsync()
        {
            return await dbContext.Cars.ToListAsync();
        }

        public async Task<Car?> GetByIdAsync(int id)
        {
            return await dbContext.Cars.FindAsync(id);
        }

        public async Task UpdateAsync(Car updatedCar)
        {
            dbContext.Update(updatedCar);
            await dbContext.SaveChangesAsync();
        }
    }
}
