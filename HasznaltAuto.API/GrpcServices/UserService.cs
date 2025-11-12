using Grpc.Core;
using HasznaltAuto.API.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace HasznaltAuto.API.GrpcServices;

public class UserService(
    HasznaltAutoDbContext hasznaltAutoDbContext,
    BaseService baseService,
    ILogger<HasznaltAutoService> logger)
    : UserGrpc.UserGrpcBase
{
    public override async Task ListUsers(Empty request, IServerStreamWriter<ListUsersResponse> responseStream, ServerCallContext context)
    {
        foreach (var userEntity in hasznaltAutoDbContext.Users)
        {
            await responseStream.WriteAsync(MapToProtobufListUser(userEntity));
        }
    }

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
                // Note AB: Biztonsági okokból nem a legjobb megoldás, hiszen így információt adunk arról, hogy létezik ilyen user az adatbázisban
                Message = "Username taken."
            });
        }

        var hashedPassword = GetHashedPassword(request.User.Password);
        var newUserToAdd = new User
        {
            Name = request.User.Name,
            Password = hashedPassword
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

        var isValidPassword = VerifyPassword(request.User.Password, userToLogin.Password);
        if (!isValidPassword)
        {
            return await Task.FromResult(new LoginResponse
            {
                SessionId = string.Empty,
                CurrentUser = 0,
                Message = "Invalid password"
            });
        }

        var guid = Guid.NewGuid().ToString();
        lock (baseService.sessionList)
        {
            baseService.sessionList.Add(guid);
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

        lock (baseService.sessionList)
        {
            if (baseService.sessionList.Contains(request.SessionId))
            {
                baseService.sessionList.Remove(request.SessionId);
            }
        }

        return await baseService.RequestSuccessful("Logout successful");
    }

    public ListUsersResponse MapToProtobufListUser(User userEntity)
    {
        return new ListUsersResponse()
        {
            Id = userEntity.Id,
            Name = userEntity.Name,
        };
    }

    #region Private methods
    private static string GetHashedPassword(string password)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(password);
        byte[] hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        return GetHashedPassword(password) == storedHash;
    }
    #endregion
}
