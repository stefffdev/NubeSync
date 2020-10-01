using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nube.SampleService.Hubs;
using NubeSync.Core;
using NubeSync.Server;
using NubeSync.Service.Data;
using NubeSync.Service.DTO;

namespace NubeSync.Service
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // UNCOMMENT THIS IF YOU WANT TO ACTIVATE AUTHENTICATION
            //services.AddProtectedWebApi(Configuration);

            services.AddDbContext<DataContext>(opt => opt.UseSqlServer("Server=(localdb)\\v11.0;Database=nube-sample;Trusted_Connection=True;MultipleActiveResultSets=true"));
            services.AddControllers();
            services.AddSignalR();

            services.AddCors(o => o.AddPolicy("AllowedOrigins", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            services.AddTransient<IAuthentication, Authentication>();
            services.AddTransient<IOperationService>(s => new OperationService(typeof(TodoItem)));
            services.AddTransient<IChangeTracker, ChangeTracker>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("AllowedOrigins");

            // UNCOMMENT THIS IF YOU WANT TO ACTIVATE AUTHENTICATION
            //app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<UpdateHub>("/updateHub");
            });
        }
    }
}
