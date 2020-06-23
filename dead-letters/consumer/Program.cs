using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory();
            using(var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var exchangeName = "dead-letters";
                var queueName = "grave";
                channel.ExchangeDeclare(exchange: exchangeName,
                                        type: ExchangeType.Direct,
                                        durable: true,
                                        autoDelete: false);

                channel.QueueDeclare(queue: queueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false);

                // 負責處理被送到 grave 的 dead-letters
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) => {
                    var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                    Console.WriteLine($"Get message from {queueName}: '{message}'");
                    channel.BasicAck(ea.DeliveryTag, false);
                };

                channel.QueueBind(queue: queueName,
                                  exchange: exchangeName,
                                  routingKey: "grave");

                channel.BasicConsume(queue: queueName,
                                     consumer: consumer);
                
                Console.WriteLine("Enter to exit...");
                Console.ReadLine();
            }
        }
    }
}
