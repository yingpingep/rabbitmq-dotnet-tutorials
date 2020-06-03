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

                if (args.Length < 1) {
                    Console.WriteLine("Usage: {0} [info] [warning] [error]",
                                      Environment.GetCommandLineArgs()[0]);
                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();
                    Environment.ExitCode = 1;
                    return;
                }

                foreach (var key in args)
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
    }
}
