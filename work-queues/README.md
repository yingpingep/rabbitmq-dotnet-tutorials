# RabbitMQ toturial 2 work queue

Work Queues (或稱 Task Queues) 的核心概念，是將複雜工作放進 queue 等待被完成，避免立刻執行需要消耗大量資源與時間的工作。

## Diagram

```
                             +----+
                        ┌--> | C1 |
+---+      +---------+  |    +----+
| P | ---> |q|u|e|u|e|--|
+---+      +---------+  |    +----+
                        └--> | C2 |
                             +----+      
                     
```

---

## Prerequisites

請先使用以下指令建立一個可用的 RabbitMQ Server。

```
docker run -it --rm --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

## Demo Introduction

在此範例中的 comsumer 是 Worker 專案，值得注意的是委派到 `consumer.Received` 的 arrow function:

```csharp
// 略...
consumer.Received += (model, eventArgs) => {
    var body = eventArgs.Body;
    var message = Encoding.UTF8.GetString(body.ToArray());
    Console.Write(" [x] Received '{0}'", message);

    int dots = message.Split(".").Length - 1;

    // 有幾個 . 就等幾秒
    Thread.Sleep(dots * 1000);
    Console.WriteLine(" => Finish!");
};
...略
```

當收到 producer 的 message 後，會去計算 message 中的 dot 數量，並依此計算等待的秒數，模擬處理複雜工作時所花費的時間。

測試時也可以開啟兩個以上的 consumer，觀察在預設情況下 consumer 會如何執行在佇列中的工作。


## Run demo

> 該範例的程式碼都有在建立連線後宣告 queue 資訊，所以啟動的先後順序並沒有差異。  
> 請使用不同的 terminal 執行。

Run the consumer:

```
cd Worker
dotnet run
```

Run the producer:

```
cd NewTask
dotnet run "Message..."   # consumer 收到訊息後會等待 3 秒
```