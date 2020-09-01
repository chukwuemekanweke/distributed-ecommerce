using System;
using System.Collections.Generic;
using System.Linq;
using StackExchange.Redis;

using System.Threading.Tasks;

namespace Basket.API.Data.Interfaces
{
    public interface IBasketContext
    {
        IDatabase  Redis { get; }
    }
}
