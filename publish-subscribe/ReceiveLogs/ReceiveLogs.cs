using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace ReceiveLogs
{
    class ReceiveLogs
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory();

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                // 宣告名稱為 logs、type 為 Fanout 的 exchange
                channel.ExchangeDeclare(exchange: "logs", 
                                        type: ExchangeType.Fanout);

                // 建立具有唯一名稱、會自動刪除的 queue            
                var queueName = channel.QueueDeclare().QueueName;

                // 將剛剛取得的 queue name binding 到指定的 exchange
                channel.QueueBind(queue: queueName,
                                  exchange: "logs",
                                  routingKey: "");

                Console.WriteLine(" [*] Waiting for logs.");

                var consumer  = new EventingBasicConsumer(channel);
                consumer.Received += (model, eventArgs) => {
                    var body = eventArgs.Body;
                    var message = Encoding.UTF8.GetString(body.ToArray());
                    Console.WriteLine(" [x] {0}", message);

                    channel.BasicAck(deliveryTag: eventArgs.DeliveryTag,
                                     multiple: false);
                };
                
                channel.BasicConsume(queueName, false, consumer);
                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }

        }
    }
}
