using API.Extensions;
using API.Middleware;
using API.SignalR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace API
{
    public class Startup
    {
        private readonly IConfiguration _config;

        public Startup(IConfiguration config)
        {
            _config = config;

        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddApplicationServices(_config);
            services.AddIdentityServices(_config);  
            services.AddSignalR(); 
            
            
            services.AddCors();
            // services.AddSwaggerGen(c =>
            // {
            //     c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
            // });
            // Enable Swagger
            // services.AddSwaggerGen(swagger =>
            // {
            //     //This is to generate the Default UI of Swagger Documentation
            //     swagger.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
            //     // To Enable authorization using Swagger (JWT)
            //     swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            //     {
            //         Name = "Authorization",
            //         Type = SecuritySchemeType.ApiKey,
            //         Scheme = "Bearer",
            //         BearerFormat = "JWT",
            //         In = ParameterLocation.Header,
            //         Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer Token' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
            //     });
            //     swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
            //     {
            //         {
            //               new OpenApiSecurityScheme
            //                 {
            //                     Reference = new OpenApiReference
            //                     {
            //                         Type = ReferenceType.SecurityScheme,
            //                         Id = "Bearer"
            //                     }
            //                 },
            //                 new string[] {}
            //         }
            //     });
            // });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseMiddleware<ExceptionMiddleware>();
            // app.UseSwagger();
            // app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(x=>x.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithOrigins("http://localhost:4200"));

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<PresenceHub>("hubs/presence");
                endpoints.MapHub<MessageHub>("hubs/message");
                endpoints.MapFallbackToController("Index","Fallback");
            });
        }
    }
}
