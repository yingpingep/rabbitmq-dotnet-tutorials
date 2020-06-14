using System;
using RabbitMQ.Client;
using System.Text;
using System.Linq;

namespace EmitLog
{
    class EmitLogTopic
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
                channel.ExchangeDeclare(exchange: "topic_exchange",
                                        type: ExchangeType.Topic);

                // Using "anonymous.info" as a default routing key.
                var routingKey = (args.Length > 0) ? args[0] : "anonymous.info";

                // Using "Helo, world!" as a default message.
                var message = (args.Length > 1)
                            ? string.Join(" ", args.Skip(1).ToArray())
                            : "Hello, world!";

                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish("topic_exchange", routingKey, null, body);
                Console.WriteLine(" [x] Sent '{0}':'{1}'", routingKey, message);
            }
        }
    }
}
