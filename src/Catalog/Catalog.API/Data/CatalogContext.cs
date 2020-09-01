using Catalog.API.Data.Interfaces;
using Catalog.API.Entities;
using Catalog.API.Settings;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Catalog.API.Data
{
    public class CatalogContext : ICatalogContext
    {

        public CatalogContext(ICatalogDatabaseSettings settings)
        {
            connectToDatabase(settings);
        }


        private void connectToDatabase(ICatalogDatabaseSettings settings)
        {
            MongoClient client = new MongoClient($"{settings.ConnectionString}/{settings.DatabaseName}");
            IMongoDatabase database = client.GetDatabase(settings.DatabaseName);

            Products = database.GetCollection<Product>(settings.CollectionName);

            CatalogContextSeed.SeedData(Products);

        }




        public IMongoCollection<Product> Products { get; private set; }




    }
}
