using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ReceiveLogs
{
    class ReceiveLogs
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory();

            using (var connection = factory.CreateConnection())           
            using (var channel = connection.CreateModel())
            {            
                var queueName = channel.QueueDeclare().QueueName;
                channel.ExchangeDeclare(exchange: "direct_logs",
                                        type: ExchangeType.Direct);            

                string[] keys = getKeys(args);

                if (keys.Length == 1) {
                    Console.WriteLine("Using default routing key [info].");
                }

                foreach (var key in keys)
                {
                    channel.QueueBind(queue: queueName,
                                      exchange: "direct_logs",
                                      routingKey: key);
                }

                Console.WriteLine(" [*] Waiting for messages.");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) => {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body.ToArray());
                    var routingKey = ea.RoutingKey;

                    Console.WriteLine(" [x] Received '{0}':'{1}'",
                                      routingKey, message);
                    
                    channel.BasicAck(ea.DeliveryTag, false);
                };

                channel.BasicConsume(queue: queueName, consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
            
        }

        private static string[] getKeys(string[] args)
        {            
            return args.Length < 1 
                ? new string[1] {"info"}
                : args;
        }
    }
}
