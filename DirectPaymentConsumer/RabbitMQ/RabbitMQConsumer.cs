using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DirectPaymentConsumer.RabbitMQ
{
    public class RabbitMQConsumer
    {
        private static ConnectionFactory _factory;
        private static IConnection _connection;
        private static IModel _channel;
        private static Random _random;

        private const string ExchangeName = "Topic_Exchange";
        private const string RPCQueue = "rpc_queue";

        public void CreateConnection()
        {
            _factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: RPCQueue, durable: false, exclusive: false, autoDelete: false,arguments: null); ;
            _channel.BasicQos(0, 1, false);
            _random = new Random();
        }

        public void Close()
        {
            _connection.Close();
        }

        public void ProcessMessages()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, e) =>
            {
                string response = null;
                var props = e.BasicProperties;
                var replyProps = _channel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;

                Console.WriteLine("----------------------------------------------------------");
                try
                {
                    response = MakePayment(e);
                    Console.WriteLine("Correlation ID = {0}", props.CorrelationId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" ERROR : " + ex.Message);
                    response = "";
                }
                finally
                {
                    if (response != null)
                    {
                        var responseBytes = Encoding.UTF8.GetBytes(response);
                        _channel.BasicPublish(exchange:"",routingKey: props.ReplyTo,basicProperties: replyProps,body: responseBytes);
                    }
                    _channel.BasicAck(e.DeliveryTag, false);
                }
                Console.WriteLine("----------------------------------------------------------");
                Console.WriteLine("");
            };
            _channel.BasicConsume(queue: RPCQueue,
                                 autoAck: false,
                                 consumer: consumer);
            Console.ReadLine();
        }

        private string MakePayment(BasicDeliverEventArgs ea)
        {
            var payment = (CardPayment)ea.Body.ToArray().DeSerialize(typeof(CardPayment));
            var response = _random.Next(1000, 100000000).ToString(CultureInfo.InvariantCulture);
            Console.WriteLine("Payment -  {0} : £{1} : Auth Code <{2}> ", payment.CardNumber, payment.Amount, response);

            return response;
        }
    }
}
