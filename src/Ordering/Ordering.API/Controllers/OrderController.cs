using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ordering.Application.Commands;
using Ordering.Application.Queries;
using Ordering.Application.Responses;

namespace Ordering.API.Controllers
{
    [Route("api/v1/order")]
    [ApiController]
    public class OrderController : ControllerBase
    {



        private readonly IMediator _mediator;

        public OrderController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }


        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<OrderResponse>),(int)HttpStatusCode.OK)]

        public async Task<IActionResult> GetOrdersByUserName(string userName)
        {
            var query = new GetOrderByUsernameQuery(userName);

            IEnumerable<OrderResponse> orders = await _mediator.Send(query);

            return Ok(orders);

        }



        //testing purpose
        [HttpPost]
        [ProducesResponseType(typeof(OrderResponse), (int)HttpStatusCode.OK)]

        public async Task<IActionResult> GetOrdersByUserName([FromBody] CheckoutOrderCommand command)
        {

            OrderResponse result = await _mediator.Send(command);

            return Ok(result);

        }














    }
}