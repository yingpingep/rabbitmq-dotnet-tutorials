using System;
using System.Text;
using RabbitMQ.Client;

namespace EmitLog
{
    class EmitLog
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory();
            
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                // 宣告 Fanout type 的 exchange
                // 並命名為 logs
                channel.ExchangeDeclare(exchange: "logs",
                                        type: ExchangeType.Fanout);
                
                var message = GetMessage(args);
                var body = Encoding.UTF8.GetBytes(message);
                
                // 推送訊息到名稱為 logs 的 exchange
                channel.BasicPublish(exchange: "logs",
                                     routingKey: "",
                                     basicProperties: null,
                                     body: body);

                Console.WriteLine(" [x] Sent {0}", message);
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        private static string GetMessage(string[] args)
        {
            return (args.Length > 0)
                ? string.Join(" ", args)
                : "info: Hello World!";
        }
    }
}
