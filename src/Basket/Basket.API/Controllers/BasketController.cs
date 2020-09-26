using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Basket.API.Entities;
using Basket.API.Repositories.Interfaces;
using EventBusRabbitMQ.Common;
using EventBusRabbitMQ.Events;
using EventBusRabbitMQ.Producer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Basket.API.Controllers
{
    [Route("api/v1/basket")]
    [ApiController]
    public class BasketController : ControllerBase
    {


        private readonly IBasketRepository _basketRepository;
        private readonly IMapper _mapper;
        private readonly EventBusRabbitMqProducer _eventBus;

        public BasketController(IBasketRepository basketRepository, IMapper mapper, EventBusRabbitMqProducer eventBus)
        {
            _basketRepository = basketRepository ?? throw new ArgumentNullException(nameof(basketRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        [HttpGet]
        [ProducesResponseType(typeof(BasketCart), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetBasket(string username)
        {
            BasketCart basket = await _basketRepository.GetBasket(username);
            return Ok(basket);

        }

        [HttpPost]
        [ProducesResponseType(typeof(BasketCart), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateBasket([FromBody]BasketCart basketCart )
        {
            BasketCart basket = await _basketRepository.UpdateBasket(basketCart);
            return Ok(basket);

        }

        [HttpDelete("{username}")]
        [ProducesResponseType(typeof(BasketCart), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteBasket(string username)
        {
            bool isSuccessful = await _basketRepository.DeleteBasket(username);
            return Ok(new { Successful = isSuccessful });

        }


        [HttpPost("[action]")]
        [ProducesResponseType( (int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]

        public async Task<IActionResult> Checkout(BasketCheckout checkout)
        {
            BasketCart basket = await _basketRepository.GetBasket(checkout.userName);

            if (basket == null)
            {
                return BadRequest();
            }


            var basketRemoved = await _basketRepository.DeleteBasket(basket.UserName);
            if(!basketRemoved)
            {
                return BadRequest();
            }

            //var eventMessage = _mapper.Map <BasketCheckoutEvent>(basket);
            var eventMessage = new BasketCheckoutEvent { 
            
                AddressLine = checkout.AddressLine,
                CardName = checkout.CardName,
                CardNumber = checkout.CardNumber,
                Country = checkout.Country,
                CVV = checkout.CVV,
                EmailAddress = checkout.EmailAddress,
                Expiration = checkout.Expiration,
                FirstName = checkout.FirstName,
                LastName = checkout.LastName,
                PaymentMethod = checkout.PaymentMethod,
                State = checkout.State,
                TotalPrice = checkout.TotalPrice,
                userName = checkout.userName,
                ZipCode = checkout.ZipCode
            
            };


            eventMessage.RequestId = Guid.NewGuid();
            eventMessage.TotalPrice = basket.TotalPrice;

            try
            {
                _eventBus.PublishBasketCheckout(EventBusConstants.BasketCheckoutQueue, eventMessage);
            }
            catch(Exception ex)
            {
                return BadRequest();

            }

            return Accepted(basket);

        }



    }
}