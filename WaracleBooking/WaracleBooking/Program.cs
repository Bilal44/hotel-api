using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using WaracleBooking.Filters;
using WaracleBooking.Persistence.Context;
using WaracleBooking.Persistence.Repositories;
using WaracleBooking.Persistence.Repositories.Interfaces;
using WaracleBooking.Services;
using WaracleBooking.Services.Interfaces;

namespace WaracleBooking;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<BookingDbContext>(options =>
            options.UseInMemoryDatabase("BookingDb"));

        // Add services to the container.
        builder.Services
            .AddScoped<IHotelRepository, HotelRepository>()
            .AddScoped<IRoomRepository, RoomRepository>()
            .AddScoped<IBookingRepository, BookingRepository>()
            .AddScoped<IDataRepository, DataRepository>()
            .AddScoped<IBookingService, BookingService>()
            .AddScoped<ApiExceptionFilter>();

        builder.Services.AddControllers(options =>
            {
                options.Filters.Add<ApiExceptionFilter>();
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

        builder.Services.AddSerilog();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "Waracle Booking API",
                Description = "An .NET 8 API facilitating hotel inquiry and room booking for Waracle tech task.",
            });
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseHttpsRedirection();
        app.MapControllers();
        app.Run();
    }
}