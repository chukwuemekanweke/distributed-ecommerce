using Catalog.API.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Catalog.API.Data.Interfaces
{
    public  class CatalogContextSeed
    {


        public static async Task  SeedData(IMongoCollection<Product> productCollection)
        {

            bool exist = await  productCollection.Find(p=>true).AnyAsync();

            if (!exist)
            {
               await productCollection.InsertManyAsync(GetPreconfiguredProducts());
            }

        }

        private static IEnumerable<Product> GetPreconfiguredProducts()
        {
            List<Product> products = new List<Product>
            {

                new Product(){Name="IPhone X", Summary="asd", Description="Na wa o", Category="Smart Phone", ImageFile="product_1.png", Price=950.00M }

            };
            return products;
        }
    }
}