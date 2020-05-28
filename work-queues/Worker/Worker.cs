using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Worker
{
    class Worker
    {
        static void Main(string[] args)
        {
            IConnectionFactory factory = new ConnectionFactory();
            
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare("task_queue", false, false, false, null);
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, eventArgs) => {
                    var body = eventArgs.Body;
                    var message = Encoding.UTF8.GetString(body.ToArray());
                    Console.Write(" [x] Received '{0}'", message);

                    int dots = message.Split(".").Length - 1;

                    // 有幾個 . 就等幾秒
                    Thread.Sleep(dots * 1000);
                    Console.WriteLine(" => Finish!");
                };

                channel.BasicConsume("task_queue", true, consumer);
                

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}
