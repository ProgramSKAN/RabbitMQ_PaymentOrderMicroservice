using Payments_API.Models;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payments_API.RabbitMQ
{
    public class RabbitMQClient
    {
        private const string ExchangeName = "Topic_Exchange";
        private const string CardPaymentQueueName = "CardPaymentTopic_Queue";
        private const string PurchaseOrderQueueName = "PurchaseOrderTopic_Queue";
        private const string AllQueueName = "AllTopic_Queue";

        private static ConnectionFactory _factory;
        private static IConnection _connection;
        private static IModel _channel;

        public RabbitMQClient()
        {
            CreateConnection();
        }

        private static void CreateConnection() {

            _factory = new ConnectionFactory { HostName = "localhost", UserName = "guest", Password = "guest" };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: ExchangeName, ExchangeType.Topic);

            _channel.QueueDeclare(queue: CardPaymentQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueDeclare(queue: PurchaseOrderQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueDeclare(queue: AllQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);


            _channel.QueueBind(queue: CardPaymentQueueName, exchange: ExchangeName, routingKey: "payment.card");
            _channel.QueueBind(queue: PurchaseOrderQueueName, exchange: ExchangeName, routingKey: "payment.purchaseorder");
            _channel.QueueBind(queue: AllQueueName, exchange: ExchangeName, routingKey: "payment.*");
        }
        public void Close()
        {
            _connection.Close();
        }


        public void SendPayment(CardPayment payment)
        {
            SendMessage(payment.Serialize(),"payment.card");
            Console.WriteLine(" Payment Sent {0}, £{1}", payment.CardNumber, payment.Amount);
        }

        public void SendPurchaseOrder(PurchaseOrder purchaseOrder)
        {
            SendMessage(purchaseOrder.Serialize(), "payment.purchaseorder");
            Console.WriteLine(" Purchase Order Sent {0}, £{1}, {2}, {3}",purchaseOrder.CompanyName, purchaseOrder.AmountToPay,purchaseOrder.PaymentDayTerms, purchaseOrder.PurchaseOrderNumber);
        }

        private void SendMessage(byte[] message,string routingKey)
        {
            _channel.BasicPublish(exchange: ExchangeName, routingKey: routingKey, basicProperties: null, body: message);
        }
    }
}
