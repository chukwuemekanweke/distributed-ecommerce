using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using EventBusRabbitMQ;
using EventBusRabbitMQ.Producer;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Ordering.API.Extensions;
using Ordering.API.RabbitMQ;
using Ordering.Application.Handlers;
using Ordering.Core.Repositories;
using Ordering.Core.Repositories.Base;
using Ordering.Infrastructure.Data;
using Ordering.Infrastructure.Repositories;
using Ordering.Infrastructure.Repositories.Base;
using RabbitMQ.Client;

namespace Ordering.API
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

            string connectionString = Configuration.GetConnectionString("OrderConnection"); 
            services.AddControllers();


            services.AddDbContext<OrderContext>(x => x.UseSqlServer(connectionString), ServiceLifetime.Singleton);

           
            //required for mediator, that's why we are registering this type of
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));


            //for mediator
            services.AddScoped(typeof(IOrderRepository), typeof(OrderRepository));
            //for rabbit mq
            services.AddTransient<IOrderRepository, OrderRepository>();


            services.AddAutoMapper(typeof(Startup));
            services.AddMediatR(typeof(CheckoutOrderHandler).GetTypeInfo().Assembly);


            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Order API", Version = "v1" });
            });


            services.AddTransient<EventBusRabbitMQConsumer>();
            services.AddSingleton<IRabbitMQConnection>(sp => {

                string userName = Configuration["EventBus:UserName"];
                string password = Configuration["EventBus:Password"];

                var factory = new ConnectionFactory()
                {
                    HostName = Configuration["EventBus:HostName"]

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


            app.UseRabbitListener();


            app.UseSwagger();

            app.UseSwaggerUI(c => {

                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order API v1");
            });


        }
    }
}
