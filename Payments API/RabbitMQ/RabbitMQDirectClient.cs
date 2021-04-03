using Payments_API.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments_API.RabbitMQ
{
    public class RabbitMQDirectClient
    {
        private static IConnection _connection;
        private static IModel _channel;
        private static string _replyQueueName;
        private static EventingBasicConsumer _consumer;
        private readonly BlockingCollection<string> respQueue = new BlockingCollection<string>();

        private const string RPCReplyQueue = "rpc_reply";

        public void CreateConnection()
        {
            var factory = new ConnectionFactory { HostName = "localhost", UserName = "guest", Password = "guest" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _replyQueueName=_channel.QueueDeclare(queue: RPCReplyQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);

            _consumer = new EventingBasicConsumer(_channel);
        }
        public void Close()
        {
            _connection.Close();
        }

        public string MakePayment(CardPayment payment)
        {
            var correlationId = Guid.NewGuid().ToString();
            var props = _channel.CreateBasicProperties();
            props.ReplyTo = _replyQueueName;
            props.CorrelationId = correlationId;

            _channel.BasicPublish(exchange: "",routingKey:"rpc_queue",basicProperties: props,body: payment.Serialize());

            _consumer.Received += (model, e) =>
            {
                if (e.BasicProperties.CorrelationId == correlationId)
                {
                    var body = e.Body.ToArray();
                    var authCode = Encoding.UTF8.GetString(body);
                    respQueue.Add(authCode);
                }
            };
            _channel.BasicConsume(queue: _replyQueueName, autoAck: true, consumer: _consumer);
            return respQueue.Take();
        }
    }
}
