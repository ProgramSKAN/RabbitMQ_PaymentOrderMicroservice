using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace PurchaseOrderConsumer.RabbitMQ
{
    public class RabbitMQConsumer
    {
        private static ConnectionFactory _factory;
        private static IConnection _connection;

        private const string ExchangeName = "Topic_Exchange";
        private const string PurchaseOrderQueueName = "PurchaseOrderTopic_Queue";

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
                    Console.WriteLine("Listening for Topic <payment.purchaseorder>");
                    Console.WriteLine("-----------------------------------------");
                    Console.WriteLine();

                    channel.ExchangeDeclare(exchange: ExchangeName, ExchangeType.Topic);
                    channel.QueueDeclare(queue: PurchaseOrderQueueName, durable: true,exclusive: false,autoDelete: false,arguments: null);

                    channel.QueueBind(queue: PurchaseOrderQueueName, exchange: ExchangeName,routingKey:"payment.purchaseorder");

                    channel.BasicQos(0, 10, false);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, e) =>
                    {
                        var message = (PurchaseOrder)e.Body.ToArray().DeSerialize(typeof(PurchaseOrder));
                        var routingKey = e.RoutingKey;
                        Console.WriteLine(" [x] Received '{0}':'{1}'",routingKey,message);
                    };
                    channel.BasicConsume(queue: PurchaseOrderQueueName,
                                         autoAck: true,
                                         consumer: consumer);
                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();
                }
            }
        }
    }
}
