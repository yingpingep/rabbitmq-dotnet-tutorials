using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace publisher_confirms
{
    class Program
    {
        private const int MESSAGE_COUNT = 20;
        private static IConnection connection;
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory();
            connection = factory.CreateConnection();
            
            // SynchronousWaitForConfirms();
            AsynchronousWaitConfirm();

            connection.Close();
        }


        #region 同步等待確認
        private static void SynchronousWaitForConfirms() 
        {
            using (IModel channel = connection.CreateModel()) {
                var queueName = channel.QueueDeclare().QueueName;
                // 必要，使用這個方法後 brokder 才會送 confirm 給 producer
                channel.ConfirmSelect();

                var timer = new Stopwatch();                
                int i = 0;
                timer.Start();                                
                while(i < MESSAGE_COUNT)
                {
                    bool timeout = false;
                    var body = Encoding.UTF8.GetBytes(i.ToString());
                    channel.BasicPublish(exchange: "",
                                        routingKey: queueName,
                                        basicProperties: null,
                                        body: body);
                                        
                    channel.WaitForConfirms(new TimeSpan(0, 0, 0, 0, 10), out timeout);

                    if (timeout) {                        
                        Console.WriteLine("Timeout!");
                        break;
                    }

                    i += 1;
                }

                timer.Stop();                
                Console.WriteLine($"Published {i:N0} messages individually in {timer.ElapsedMilliseconds:N0} ms");                                                
            }
        }   
        #endregion

        #region 非同步等待確認
        private static void AsynchronousWaitConfirm() 
        {
            using (var channel = connection.CreateModel())
            {
                var args = new Dictionary<string, object>();
                args.Add("x-max-length", 10);
                args.Add("x-overflow", "reject-publish");
                var queueName = channel.QueueDeclare(arguments: args).QueueName;
                channel.ConfirmSelect();
                int count = 0;

                // 收到 nack 的 messages 會被放到這個 Queue 中，等待一次重送的機會
                var republishMessages = new ConcurrentQueue<string>();

                // 單純拿來記錄以發送的 messages
                var outstandingConfirms = new ConcurrentDictionary<ulong, string>();
                
                // 處理沒有成功被 confirm 的 message
                // 僅會嘗試重送一次
                void rePublishMessages()
                {
                    while(republishMessages.TryDequeue(out string body))
                    {
                        channel.BasicPublish(exchange: "", 
                                             routingKey: queueName,
                                             basicProperties: null,
                                             body: Encoding.UTF8.GetBytes(body));
                        Interlocked.Add(ref count, 1);     
                    }
                };

                // 把收到 ack / nack 的 messages 從 outstandingConfirms 清掉
                void cleanOutstandingConfirms(ulong sequenceNumber, bool multiple, bool ack)
                {    
                    if (multiple)
                    {
                        var confirmed = outstandingConfirms.Where(k => k.Key <= sequenceNumber);
                        foreach (var entry in confirmed)
                        {
                            if (ack) 
                            {
                                republishMessages.Enqueue(entry.Value);                    
                            }
                            outstandingConfirms.TryRemove(entry.Key, out _);
                        }
                        return;
                    }

                    if (ack)
                    {
                        outstandingConfirms.TryGetValue(sequenceNumber, out string body);
                        republishMessages.Enqueue(body);
                    }
                    outstandingConfirms.TryRemove(sequenceNumber, out _);       
                }

                // Broker 回傳 confirms 會透過 ack 通知 publisher，哪筆 message 已被收下
                channel.BasicAcks += (sender, ea) => {
                    Console.WriteLine($"Message has been  ack-ed. Sequence number: {ea.DeliveryTag}, multiple: {ea.Multiple}");
                    cleanOutstandingConfirms(ea.DeliveryTag, ea.Multiple, true);
                };

                // 或是透過 nack 告知 publisher，哪筆 message 被拒絕
                channel.BasicNacks += (sender, ea) =>
                {
                    outstandingConfirms.TryGetValue(ea.DeliveryTag, out string body);
                    Console.WriteLine($"Message has been nack-ed. Sequence number: {ea.DeliveryTag}, multiple: {ea.Multiple}");
                    
                    cleanOutstandingConfirms(ea.DeliveryTag, ea.Multiple, false);      
                    rePublishMessages();
                };

                var timer = new Stopwatch();
                timer.Start();
                for (int i = 0; i < MESSAGE_COUNT; i++)
                {
                    var body = i.ToString();
                    outstandingConfirms.TryAdd(channel.NextPublishSeqNo, i.ToString());
                    channel.BasicPublish(exchange: "", 
                                         routingKey: queueName,
                                         basicProperties: null,
                                         body: Encoding.UTF8.GetBytes(body));                    
                }                

                if (!WaitUntil(60, () => outstandingConfirms.IsEmpty)) 
                {
                    throw new Exception("All messages could not be confirmed in 60 seconds");
                }                


                timer.Stop();
                Console.WriteLine($"Published {MESSAGE_COUNT + count:N0} messages and handled confirm asynchronously {timer.ElapsedMilliseconds:N0} ms");
            }
        }

        private static bool WaitUntil(int numberOfSeconds, Func<bool> condition)
        {
            int waited = 0;
            while(!condition() && waited < numberOfSeconds * 1000)
            {
                Thread.Sleep(100);
                waited += 100;
            }

            return condition();
        }
    #endregion
  }
}
