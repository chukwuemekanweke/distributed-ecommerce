using AutoMapper;
using EventBusRabbitMQ;
using EventBusRabbitMQ.Common;
using EventBusRabbitMQ.Events;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Ordering.Application.Commands;
using Ordering.Application.Mapper;
using Ordering.Core.Entities;
using Ordering.Core.Repositories;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ordering.API.RabbitMQ
{
    public class EventBusRabbitMQConsumer
    {


        private readonly IRabbitMQConnection _connection;
        private readonly IMapper _mapper;
        private IServiceProvider _serviceProvider;

        public EventBusRabbitMQConsumer(IServiceProvider serviceProvider, IRabbitMQConnection connection,
            IMapper mapper)
        {
            _serviceProvider = serviceProvider;

            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        //public CheckoutOrderHandler()
        //{
        //}


        public void Consume()
        {
            var channel = _connection.CreateModel();
            channel.QueueDeclare(queue: EventBusConstants.BasketCheckoutQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += ReceivedEvent;

            channel.BasicConsume(queue: EventBusConstants.BasketCheckoutQueue, autoAck: false, consumer: consumer,noLocal:false,exclusive:false,arguments:null);


        }

        private async void ReceivedEvent(object sender, BasicDeliverEventArgs e)
        {
            if(e .RoutingKey == EventBusConstants.BasketCheckoutQueue)
            {
                var message = Encoding.UTF8.GetString(e.Body.Span);
                var basketCheckoutEvent = JsonConvert.DeserializeObject<BasketCheckoutEvent>(message);

                var command = _mapper.Map<CheckoutOrderCommand>(basketCheckoutEvent);
                using (var scope = _serviceProvider.CreateScope())
                {
                    var handler = scope.ServiceProvider.GetRequiredService<IMediator> ();
                    await handler.Send(command);
                }
               
            }
        }



        public void Disconnect()
        {
            _connection.Dispose();
        }

    }
}
