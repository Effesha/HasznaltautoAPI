using Grpc.Core;
using HasznaltAuto.Entities;
using Microsoft.EntityFrameworkCore;

namespace HasznaltAuto.Services
{
    public class UserService(
        HasznaltAutoDbContext hasznaltAutoDbContext,
        BaseService baseService,
        ILogger<HasznaltAutoService> logger) : UserGrpc.UserGrpcBase
    {
        public override async Task<ResultResponse> Register(RegistrationRequest request, ServerCallContext context)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.User.Name) || string.IsNullOrWhiteSpace(request.User.Password))
            {
                return await baseService.RequestFailed("Empty username or password");
            }

            var usernameTaken = await hasznaltAutoDbContext.Users.AnyAsync(user => user.Name == request.User.Name);
            if (usernameTaken)
            {
                return await Task.FromResult(new ResultResponse
                {
                    Success = false,
                    // TODO AB Biztonsági okokból nem a legjobb megoldás, hiszen így információt adunk arról, hogy létezik ilyen user az adatbázisban
                    Message = "Username taken."
                });
            }

            var newUserToAdd = new User
            {
                Name = request.User.Name,
                Password = request.User.Password
            };

            await hasznaltAutoDbContext.Users.AddAsync(newUserToAdd);
            await hasznaltAutoDbContext.SaveChangesAsync();
            return await baseService.RequestSuccessful("Account created");
        }

        public override async Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.User.Name) || string.IsNullOrWhiteSpace(request.User.Password))
            {
                return await Task.FromResult(new LoginResponse
                {
                    SessionId = string.Empty,
                    CurrentUser = 0,
                    Message = "Empty username or password."
                });
            }

            var userToLogin = await hasznaltAutoDbContext.Users.FirstOrDefaultAsync(user => user.Name == request.User.Name);
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
            lock (baseService._sessionList)
            {
                baseService._sessionList.Add(guid);
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
                return await baseService.RequestFailed("You are not logged in.");
            }

            lock (baseService._sessionList)
            {
                if (baseService._sessionList.Contains(request.SessionId))
                {
                    baseService._sessionList.Remove(request.SessionId);
                }
            }

            return await baseService.RequestSuccessful("Logout successful");
        }
    }
}
