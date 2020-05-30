# [RabbitMQ toturial 2 work queue](https://www.rabbitmq.com/tutorials/tutorial-two-dotnet.html) part 2

Work Queues (或稱 Task Queues) 的核心概念，是將複雜工作放進 queue 等待被完成，避免立刻執行需要消耗大量資源與時間的工作。

Part 2 的重點在於說明如何透過參數告知 RabbitMQ 指定的 queue、messages 在 RabbitMQ crash 或是關閉之後是否要被保留，並於下次 RabbitMQ 可用時還原、如何避免 RabbitMQ 只使用 Round-robin 的方式發送 message 到 consumer，導致發生負載不平衡的情形，以及如何手動回傳 acknowldgement 給 RabbitMQ。

## Diagram

```
                             +----+
                        ┌--> | C1 |
+---+      +---------+  |    +----+
| P | ---> |q|u|e|u|e|--|
+---+      +---------+  |    +----+
                        ├--> | C2 |
                        |    +----+      
                        |
                        |    +----+
                        └--> | C3 | 
                             +----+                                         
```

---

## Prerequisites

請先使用以下指令建立一個可用的 RabbitMQ Server。

```
docker run -it --rm --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
``` 

---

## Demo Introduction

### Message acknowldgement

在前兩個範例中我們都會將 `BasicConsumer` 方法中的 `autoAck` 設為 `true`，代表每當 RabbitMQ 發送了一個 message 給 consumer，consumer 收到後會立刻回傳一個 acknowldgement (之後都會稱為 ack)，通知 RabbitMQ 這個 message 已經確認被收下，RabbitMQ 就會將這個 message 從 queue 中移除。  

如果 consumer 要處理的工作都不複雜，在短時間內就可以完成，那麼 autoAck 並不會造成什麼問題，因為 RabbitMQ 移除 message 的時候 consumer 可能也完成了相對的工作。但如果 consumer 要處理的是更為複雜、沒有辦法在收到 message 之後馬上完成，就有可能會在回應了 ack 之後遺失 message 與相對的工作。

為了避免這種情況，我們可以將 `autoAck` 改回預設的 `false` 並在處理完工作之後手動回傳 ack 給 RabbitMQ:

```cs
// ... 略
consumer.Received += (model, eventArgs) => {
    // ... 略

    // 手動傳送 Ack
    channel.BasicAck(deliveryTag: eventArgs.DeliveryTag,
                     multiple: false);
};
// ... 略
```

> consumer 預設並不會回傳 ack，所以要記得一定要在 delegate 方法中回傳 ack。否則已經被處理完的 message 還是會留在 queue 中並不斷地重新被發送、不斷地重新被處理。

### Message durability

為了讓 RabbitMQ 重啟後還可以繼續處理原有的 Queue，避免資料的遺失，我們需要針對建立 Queue 及 messages 方式做一些改變。  

Queue declare 的部分，要將 `durable` 從上一個範例的 `false` 改為 `true`。若 consumer 跟 producer 都有做 queue declare 的話，要將這些變動分別套用到兩個專案:

```cs
// ...略
channel.QueueDeclare(queue: "task_queue", 
                     durable: true, 
                     exclusive: false, 
                     autoDelete: false, 
                     arguments: null);
// ...略
```

> 若先前已執行了 Turtorial 2 Part 1 的話，會看到 `queue declare` 失敗的訊息，原因是先前執行 Part 1 範例程式碼時，已經建立的叫做 task_queue 的 queue。而 RabbitMQ 不允許重複定義一個名稱相同但參數不同的 queue。  

另外也要將 message 標註為 `persistent`，這個變動只需要在 producer 的專案修改就好:

```cs
// ...略
var properties = channel.CreateBasicProperties();
properties.Persistent = true;
channel.BasicPublish(exchange: "",
                     routingKey: "task_queue",
                     basicProperties: properties,
                     body: body);
// ...略
```

> 值得注意的是，雖然我們將 messages 標註為 persistent，但就官方說法，這並不 100% 的保證所有送到 queue 的 messages 就一定不會遺失，實際上 RabbitMQ 並不會在收到一個新的 message 時就將它保留到硬碟上，而是每隔一段時間做一次，所以會有一小段的時間 RabbitMQ 只有接收 messages 並保存 cache，而非儲存到硬碟。

---

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