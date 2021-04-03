using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentCardConsumer.RabbitMQ
{
    public class RabbitMQConsumer
    {
        private static ConnectionFactory _factory;
        private static IConnection _connection;

        private const string ExchangeName = "Topic_Exchange";
        private const string CardPaymentQueueName = "CardPaymentTopic_Queue";

        public void CreateConnection()
        {
            _factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };
        }

        public void Close()
        {
            _connection.Close();
        }

        public void ProcessMessages()
        {
            using (_connection = _factory.CreateConnection())
            {
                using (var channel = _connection.CreateModel())
                {
                    Console.WriteLine("Listening for Topic <payment.cardpayment>");
                    Console.WriteLine("-----------------------------------------");
                    Console.WriteLine();

                    channel.ExchangeDeclare(exchange: ExchangeName, ExchangeType.Topic);
                    channel.QueueDeclare(queue:CardPaymentQueueName,durable: true,exclusive: false,autoDelete: false,arguments: null);

                    channel.QueueBind(queue: CardPaymentQueueName,exchange: ExchangeName,routingKey:"payment.cardpayment");

                    channel.BasicQos(0, 1, false);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, e) =>
                    {
                        var message = (CardPayment)e.Body.ToArray().DeSerialize(typeof(CardPayment));
                        var routingKey = e.RoutingKey;
                        Console.WriteLine(" [x] Received '{0}':'{1}'",routingKey,message);
                    };
                    channel.BasicConsume(queue: CardPaymentQueueName,
                                         autoAck: true,
                                         consumer: consumer);
                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();
                }
            }
        }
    }
}
