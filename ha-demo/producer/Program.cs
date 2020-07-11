using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;

namespace producer
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory();
            List<string> hostnames = new List<string>() {
                "IP1",
                "IP2"
            };

            string queueName = "ha-all-test";
            int count = 0;

            using (var connection = factory.CreateConnection(hostnames))
            using (var channel = connection.CreateModel())
            {
                Console.WriteLine("Press [Enter] to push messages...");
                while (Console.ReadKey().Key == ConsoleKey.Enter)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        string message = $"hello !!! message {count}";
                        byte[] body = Encoding.UTF8.GetBytes(message);
                        channel.BasicPublish(
                            exchange: "",
                            routingKey: queueName,
                            mandatory: false,
                            basicProperties: null,
                            body: body
                        );            
                        Console.WriteLine("{0} send", message);
                        count += 1;
                    }                    
                    Console.WriteLine("Press [Enter] to push next 10 messages...");
                }
            }
        }
    }
}
