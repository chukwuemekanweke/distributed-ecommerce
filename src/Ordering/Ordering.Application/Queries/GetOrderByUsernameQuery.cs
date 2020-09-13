using MediatR;
using Ordering.Application.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ordering.Application.Queries
{
    public class GetOrderByUsernameQuery  : IRequest<IEnumerable<OrderResponse>>
    {


        public string UserName { get; set; }

        public GetOrderByUsernameQuery(string userName)
        {
            UserName = userName ?? throw new ArgumentNullException(nameof(userName));
        }








    }
}
