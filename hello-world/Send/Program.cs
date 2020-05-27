using System;
using RabbitMQ.Client;

namespace Send
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() 
            {
                HostName = "localhost"
            };

            using(var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                
            }
        }
    }
}
