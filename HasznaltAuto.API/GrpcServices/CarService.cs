using Grpc.Core;
using HasznaltAuto.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace HasznaltAuto.API.GrpcServices
{
    public class CarService(
        HasznaltAutoDbContext hasznaltAutoDbContext,
        BaseService baseService,
        ILogger<HasznaltAutoService> logger)
        : CarGrpc.CarGrpcBase
    {
        public override async Task ListVehicleRegistrations(Empty request, IServerStreamWriter<VehicleRegistrationType> responseStream, ServerCallContext context)
        {
            foreach (var vehRegEntity in hasznaltAutoDbContext.VehicleRegistrations)
            {
                await responseStream.WriteAsync(MapToProtobufVehReg(vehRegEntity));
            }
        }

        public override async Task<VehicleRegistrationType?> GetVehicleRegistration(GetVehicleRegistrationRequest request, ServerCallContext context)
        {
            var vehReg = await hasznaltAutoDbContext.VehicleRegistrations.FindAsync(request.LicensePlateId);
            if (vehReg is null) return new VehicleRegistrationType();
            return MapToProtobufVehReg(vehReg);
        }

        public override async Task<EntityCreatedResponse> CreateVehicleRegistration(VehicleRegistrationRequest request, ServerCallContext context)
        {
            if (request?.SessionId is null || !baseService.sessionList.Contains(request.SessionId))
            {
                return new EntityCreatedResponse
                {
                    Message = "Unauthorized access.",
                    Success = false
                };
            }

            if (string.IsNullOrEmpty(request.LicensePlate))
            {
                return new EntityCreatedResponse
                {
                    Message = "Unauthorized access.",
                    Success = false
                };
            }

            var exists = await hasznaltAutoDbContext.VehicleRegistrations.AnyAsync(reg => reg.LicensePlate.Equals(request.LicensePlate));
            if (exists)
            {
                return new EntityCreatedResponse
                {
                    Message = "License plate already exists.",
                    Success = false
                };
            }

            var newVehReg = new VehicleRegistration
            {
                LicensePlate = request.LicensePlate,
                LicensePlateTypeId = request.LicensePlateTypeTypeId
            };

            await hasznaltAutoDbContext.VehicleRegistrations.AddAsync(newVehReg);
            await hasznaltAutoDbContext.SaveChangesAsync();

            return new EntityCreatedResponse
            {
                EntityId = newVehReg.Id,
                Message = "Vehicle registration created.",
                Success = true
            };
        }

        public VehicleRegistrationType MapToProtobufVehReg(VehicleRegistration entity)
        {
            var vehicleRegistrationType = new VehicleRegistrationType()
            {
                Id = entity.Id,
                LicensePlate = entity.LicensePlate,
            };

            return vehicleRegistrationType;
        }
    }
}
