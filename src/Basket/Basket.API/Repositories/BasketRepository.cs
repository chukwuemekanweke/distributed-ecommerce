using Basket.API.Data.Interfaces;
using Basket.API.Entities;
using Basket.API.Repositories.Interfaces;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Basket.API.Repositories
{
    public class BasketRepository : IBasketRepository
    {

        private readonly IBasketContext _context;

        public BasketRepository(IBasketContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<bool> DeleteBasket(string userName)
        {
            bool deleted = await _context.Redis.KeyDeleteAsync(userName);
            return deleted;
        }

        public async Task<BasketCart> GetBasket(string userName)
        {
            RedisValue result = await _context.Redis.StringGetAsync(userName);
            if(result.IsNullOrEmpty)
            {
                return null;
            }
            else
            {
                return JsonConvert.DeserializeObject<BasketCart>(result);
            }
        }

        public async Task<BasketCart> UpdateBasket(BasketCart basket)
        {
            string serializedBasket = JsonConvert.SerializeObject(basket);
            bool updated = await _context.Redis.StringSetAsync(basket.UserName, serializedBasket);

            if(!updated)
            {
                return null;
            }
            else
            {
               return await GetBasket(basket.UserName);
            }
        }
    }
}
