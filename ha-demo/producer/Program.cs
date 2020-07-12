using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace producer
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory();
            List<string> hostnames = new List<string>();
            foreach (var host in args)
            {
                hostnames.Add(host);
            }

            string queueName = "ha-all-test";
            int count = 0;

            using (var connection = factory.CreateConnection(hostnames))
            using (var channel = connection.CreateModel())
            {
                while (count < 1000)
                {
                    try
                    {
                        byte[] body = Enumerable.Repeat((byte)0x20, 1000).ToArray();
                        channel.BasicPublish(
                            exchange: "",
                            routingKey: queueName,
                            mandatory: false,
                            basicProperties: null,
                            body: body
                        );
                        Console.WriteLine("{0} send", count);
                        count += 1;     
                        Thread.Sleep(300);
                    }
                    catch (AlreadyClosedException ace)
                    {
                        Console.WriteLine(ace.Message);
                        Thread.Sleep(10000);
                        Console.WriteLine("10 seconds later...");
                    }
                    
                }
            }
        }
    }
}
