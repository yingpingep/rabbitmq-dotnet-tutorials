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
            IList<string> hostnames = new List<string>();
            foreach (var host in args)
            {
                hostnames.Add(host);
            }
            string queueName = "ha-all-test";

            using (IConnection connection = factory.CreateConnection(hostnames))
            using (IModel channel = connection.CreateModel())
            {
                EventingBasicConsumer consumer = new EventingBasicConsumer(channel);                

                consumer.ConsumerCancelled += (ModuleHandle, EventArgs) => {
                    Console.WriteLine("\n!!! ************************************ !!!");
                    Console.WriteLine("!!! Got cancell notification from broker !!!");
                    Console.WriteLine("!!! ************************************ !!!\n");
                    
                    // foreach (var tag in EventArgs.ConsumerTags)
                    // {
                    //     channel.BasicCancel(tag);                        
                    // }
                };

                consumer.Received += (model, ea) => {                    
                    try
                    {
                        string message = Encoding.UTF8.GetString(ea.Body.ToArray());

                        Console.WriteLine(" [x] Got message: {0}", ea.Body);
                        
                        if (ea.Redelivered) {
                            Console.WriteLine(" => This message had been delivered!");
                            channel.BasicAck(ea.DeliveryTag, false);
                        } else {
                            Console.WriteLine("[UwU] Going to ack!");
                            channel.BasicAck(ea.DeliveryTag, false);
                        }
                        Console.WriteLine("\n[UwU] Going to receive another message.\n");                   

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("\n => Exception happen: {0}\n", e.Message);
                        Thread.Sleep(5000);
                        Console.WriteLine(" => 5 seconds later...\n");
                    }
                    
                    Thread.Sleep(500);                    
                };

                channel.BasicQos(0, 1, false);
                channel.BasicConsume(
                    queue: queueName,
                    autoAck: false,
                    consumer: consumer);
                Console.WriteLine("Press [Enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}
