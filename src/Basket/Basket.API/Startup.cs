using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Basket.API.Data;
using Basket.API.Data.Interfaces;
using Basket.API.Repositories;
using Basket.API.Repositories.Interfaces;
using EventBusRabbitMQ;
using EventBusRabbitMQ.Producer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace Basket.API
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

            services.AddSingleton<ConnectionMultiplexer>(sp =>
            {
                string redisConnectionString = Configuration.GetConnectionString("Redis");
                var configuration = ConfigurationOptions.Parse(redisConnectionString, true);
                return ConnectionMultiplexer.Connect(configuration);
            });

            services.AddControllers();

            services.AddScoped<IBasketContext, BasketContext>();
            services.AddTransient<IBasketRepository, BasketRepository>();
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


            services.AddAutoMapper(typeof(Startup));



            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Basket API", Version = "v1" });
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

            app.UseSwaggerUI(c => {

                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Basket API v1");
            });
        }
    }
}
