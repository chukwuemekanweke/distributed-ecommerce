using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.API.Data;
using Catalog.API.Data.Interfaces;
using Catalog.API.Repositiories;
using Catalog.API.Repositiories.Interfaces;
using Catalog.API.Settings;
using EventBusRabbitMQ;
using EventBusRabbitMQ.Producer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;

namespace Catalog.API
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
            services.AddControllers();

            services.Configure<CatalogDatabaseSettings>(Configuration.GetSection(nameof(CatalogDatabaseSettings)));


            CatalogDatabaseSettings catalogueDatabaseSettings = services.BuildServiceProvider().GetRequiredService<IOptions<CatalogDatabaseSettings>>().Value;

            services.AddSingleton<ICatalogDatabaseSettings>(catalogueDatabaseSettings);

            services.AddScoped<ICatalogContext, CatalogContext>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddTransient<EventBusRabbitMqProducer>();
            string userName = Configuration["EventBus:UserName"];
            string password = Configuration["EventBus:Password"];
            string hostName = Configuration["EventBus:HostName"];

            services.AddSingleton<IRabbitMQConnection>(sp => {
                var factory = new ConnectionFactory()
                {
                    HostName = hostName

                };

                if (!string.IsNullOrWhiteSpace(userName))
                {
                    factory.UserName = userName;
                }

                if (!string.IsNullOrWhiteSpace(password))
                {
                    factory.Password = password;
                }

                return new RabbitMQConnection(factory);

            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Catalog API", Version = "v1" });
            });



        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();

            app.UseSwaggerUI(c=> {

                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog API v1");
            });

        }
    }
}
