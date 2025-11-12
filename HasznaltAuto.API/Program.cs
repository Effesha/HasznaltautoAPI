using HasznaltAuto.API.Entities;
using HasznaltAuto.API.GrpcServices;
using HasznaltAuto.API.Repositories;
using HasznaltAuto.API.Repositories.Interfaces;
using HasznaltAuto.API.REST.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddSingleton<BaseService>();

builder.Services.AddDbContext<HasznaltAutoDbContext>(options =>
{
    options.UseSqlServer("name=ConnectionStrings:HasznaltAutoDbContext");
});

builder.Services.AddScoped<IRepository<Car>, CarRepository>();
builder.Services.AddScoped<CarRestService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Local", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:56863")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "HasznaltautoRESTAPI", Version = "v1" });
    options.EnableAnnotations();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<HasznaltAutoService>().EnableGrpcWeb();
app.MapGrpcService<UserService>().EnableGrpcWeb();
app.MapGrpcService<CarService>().EnableGrpcWeb();

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

IWebHostEnvironment env = app.Environment;

app.MapGrpcReflectionService();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("Local");
app.MapControllers();

app.Run();
