using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            IConnectionFactory factory = new ConnectionFactory();
            IList<string> hostnames = new List<string>() {
                "IP1",
                "IP2"
            };
            string queueName = "ha-all-test";

            using (IConnection connection = factory.CreateConnection(hostnames))
            using (IModel channel = connection.CreateModel())
            {
                EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) => {                    
                    try
                    {
                        string message = Encoding.UTF8.GetString(ea.Body.ToArray());

                        Console.WriteLine(" [x] Got message: {0}", message);
                        
                        if (ea.Redelivered) {
                            Console.WriteLine(" => This message had been delivered!");
                            channel.BasicReject(ea.DeliveryTag, false);
                            return;
                        }

                        Thread.Sleep(5000);
                        Console.WriteLine("[UwU] Going to ack!");
                        channel.BasicAck(ea.DeliveryTag, false);                        
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(" => Exception happen: {0}\n", e.Message);
                    }

                    Console.WriteLine("[UwU] Going to receive another message.\n");                        
                };

                channel.BasicConsume(queueName, false, consumer);
                Console.WriteLine("Press [Enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}
