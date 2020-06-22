using System;
using System.Collections.Concurrent;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace client
{
  class Rpc
  {
    public class RpcClient
    {
      private readonly IConnection connection;
      private readonly IModel channel;
      private readonly string replyQueueName;
      private readonly EventingBasicConsumer consumer;
      private readonly BlockingCollection<string> respQueue = new BlockingCollection<string>();
      private readonly IBasicProperties props;

      public RpcClient()
      {
        #region 建立與 RabbitQM 連線的所有準備
        var factory = new ConnectionFactory() { HostName = "localhost" };
        connection = factory.CreateConnection();
        channel = connection.CreateModel();
        replyQueueName = channel.QueueDeclare().QueueName;
        #endregion

        #region 處理送出 message 時需要一並提供給 Server 的 properties
        props = channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueueName;
        #endregion

        #region 處理從 Server 回傳的結果
        consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
          var body = ea.Body.ToArray();
          var response = Encoding.UTF8.GetString(body);
          if (ea.BasicProperties.CorrelationId == correlationId)
          {
            respQueue.Add(response);
          }
        };

        channel.BasicConsume(
            queue: replyQueueName,
            autoAck: true,
            consumer: consumer);
        #endregion
      }

      /**
       * 送出 RPC message       
       */
      public string Call(string message)
      {        
        var messageBytes = Encoding.UTF8.GetBytes(message);
        channel.BasicPublish(
            exchange: "",
            routingKey: "rpc_queue",
            basicProperties: props,
            body: messageBytes);        

        var result = respQueue.Take();
        return result;
      }

      // 關閉與 RabbitMQ 的連線
      public void Close()
      {
        connection.Close();
      }
    }

    public static void Main()
    {
      var rpcClient = new RpcClient();

      for (int i = 0; i <= 40; i++)
      {
        Console.WriteLine(" [x] Requesting fib({0})", i);
        var response = rpcClient.Call($"{i}");
        Console.WriteLine(" [.] Got '{0}'", response);
      }


      rpcClient.Close();
    }
  }
}
