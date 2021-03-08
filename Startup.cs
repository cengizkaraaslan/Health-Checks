using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace HealthChecksExample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddDbContext<MyDbContext>(o => o.UseSqlServer(Configuration["ConnectionString"]));

            services.AddHealthChecks()
                .AddDiskStorageHealthCheck(s => s.AddDrive("C:\\", 1024))
                .AddProcessAllocatedMemoryHealthCheck(512)
                .AddProcessHealthCheck("ProcessName", p => p.Length > 0)
                .AddWindowsServiceHealthCheck("someservice", s => true)
                .AddUrlGroup(new Uri("https://localhost:44318/weatherforecast"), "Example endpoint")
                .AddSqlServer(Configuration["ConnectionString"]);

            services
                .AddHealthChecksUI(s =>
                {
                    s.AddHealthCheckEndpoint("endpoint1", "https://localhost:44318/health");
                })
            //.AddSqliteStorage("Data Source = healthchecks.db"); for sql lite
            .AddSqlServerStorage(Configuration["ConnectionString"])
            .AddInMemoryStorage();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecksUI();

                endpoints.MapHealthChecks("/health", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
            });
        }
    }

}
