using EventBusRabbitMQ.Events;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace EventBusRabbitMQ.Producer
{
    public class EventBusRabbitMqProducer
    {

        private readonly IRabbitMQConnection _connection;

        public EventBusRabbitMqProducer(IRabbitMQConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }



        public void PublishBasketCheckout(string queueName, BasketCheckoutEvent publishModel)
        {

            using (var channel = _connection.CreateModel())
            {
                channel.QueueDeclare(queueName, false, false, false, null);
                var message = JsonConvert.SerializeObject(publishModel);
                var body = Encoding.UTF8.GetBytes(message);

                IBasicProperties properties = channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.DeliveryMode = 2;

                channel.ConfirmSelect();
                channel.BasicPublish("", queueName, true, properties, body);
                channel.WaitForConfirmsOrDie();


                  channel.BasicAcks += (sender, eventArgs) =>
                 {
                     Debug.WriteLine("Sent RabbitMQ");
                 };

                channel.ConfirmSelect();

            }


        }


    }
}
