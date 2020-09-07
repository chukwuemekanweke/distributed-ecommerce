using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ordering.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ordering.Infrastructure.Data
{
    public class OrderContextSeed
    {
        public const int MAX_RETRY = 3;

       public static async Task SeedAsync(OrderContext orderContext, ILoggerFactory loggerFactory, int? retry = 0)
        {

            int retryForAvailability = retry.Value;

            try
            {
                await orderContext.Database.EnsureCreatedAsync();

                orderContext.Database.Migrate();


                if(!orderContext.Orders.Any())
                {
                    orderContext.Orders.AddRange(GetPreConfiguredOrders());
                    await orderContext.SaveChangesAsync();
                }


            }
            catch(Exception ex)
            {

                if (retryForAvailability < MAX_RETRY)
                {
                    ++retryForAvailability;
                    var log = loggerFactory.CreateLogger<OrderContextSeed>();
                    log.LogError(ex.Message);
                    await SeedAsync(orderContext, loggerFactory, retryForAvailability);
                }


                throw;
            }

        }

        private static IEnumerable<Order> GetPreConfiguredOrders()
        {
            return new List<Order>
           {
               new Order(){userName="Nweke", FirstName="Emeka", LastName="Nweke", EmailAddress="emekanweke604@gmail.com", AddressLine="Enugu", CardName="Nweke Emeka", CardNumber="xxxxxxxxx", Country="Remote", CVV="12345", Expiration="No Date",
                                                        PaymentMethod=1, State="Remote", ZipCode="042"}

           };
        }
    }
}
