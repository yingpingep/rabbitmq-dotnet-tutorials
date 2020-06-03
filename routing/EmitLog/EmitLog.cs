using System;
using System.Linq;
using System.Text;
using RabbitMQ.Client;

namespace EmitLog
{
    class EmitLog
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "direct_logs",
                                        type: "direct");

                var severity = (args.Length > 0) ? args[0] : "info";
                Console.WriteLine(" Press [Enter] to exit.");

                while (true)
                {
                    Console.Write(" Enter new message: ");
                    var message = Console.ReadLine();

                    if (message == "") {                        
                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                        Console.WriteLine(" Exiting...                    ");
                        break;
                    }

                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(exchange: "direct_logs",
                                         routingKey: severity,
                                         basicProperties: null,
                                         body: body);
                    Console.WriteLine(" [x] Sent '{0}':'{1}'", severity, message);
                }
            }
        }
    }
}
