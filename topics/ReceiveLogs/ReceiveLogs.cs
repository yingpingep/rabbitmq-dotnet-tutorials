using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ReceiveLogs
{
    class ReceiveLogsTopic
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory()
            { 
                HostName = "localhost"
            };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                string exchangeName = "topic_exchange";
                channel.ExchangeDeclare(exchange: exchangeName,
                                        type: ExchangeType.Topic);
                var queueName = channel.QueueDeclare().QueueName;

                var keys = new List<string>();
                if (args.Length < 1)
                {
                    Console.WriteLine("Using \"anonumous.info\" as a default routing key.");
                    keys.Add("anonymous.info");
                }
                keys.AddRange(args);

                foreach (var key in keys)
                {
                    // Binding routing key to exchange and queue
                    channel.QueueBind(queue: queueName,
                                      exchange: exchangeName,
                                      routingKey: key);
                }

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine(" [x] Received '{0}':'{1}'",
                                      routingKey,
                                      message);
                    
                    channel.BasicAck(ea.DeliveryTag, false);
                };

                channel.BasicConsume(queueName, autoAck: false, consumer: consumer);
                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}
