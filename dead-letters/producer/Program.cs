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

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "dead-letters",
                                        type: ExchangeType.Direct,
                                        durable: true,
                                        autoDelete: false);
                
                Dictionary<string, Object> optionArgs = new Dictionary<string, object>();
                                
                // 指定 dead-letter-exchange 為 dead-letters
                // 並重新指定 routing-key 為 grave                
                optionArgs.Add("x-dead-letter-exchange", "dead-letters");
                optionArgs.Add("x-dead-letter-routing-key", "grave");

                // message 在 queue 只能存活 10 秒
                optionArgs.Add("x-message-ttl", 10000);                

                channel.QueueDeclare(queue: "first_queue",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: optionArgs);


                for (int i = 0; i < 10; i++)
                {
                    string message = $"This is a {i} message!";
                    channel.BasicPublish(exchange: "",
                                        routingKey: "first_queue",
                                        basicProperties: null,
                                        body: Encoding.UTF8.GetBytes(message));                    
                }

                Console.WriteLine("Message send.");
            }
        }
    }
}
