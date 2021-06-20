using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;
using API.SignalR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services,IConfiguration config)
        {
             services.AddDbContext<DataContext>(options =>
            {
                // options.UseSqlServer(config.GetConnectionString("DefaultConnection"));
                options.UseSqlite(config.GetConnectionString("DefaultConnection"));
            });
            services.AddSingleton<PresenceTracker>();
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IPhotoService,PhotoService>();
            services.AddScoped<ILikesRespository,LikesRepository>();
            services.AddScoped<IMessageRepository,MessageRepository>();
            services.AddScoped<LogUserActivity>();
            services.AddScoped<IUserRepository,UserRepository>();
            services.AddScoped<apiservices>();
            services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);
            return services;

        }
        
    }
}