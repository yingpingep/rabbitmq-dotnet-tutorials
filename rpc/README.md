# [RabbitMQ toturial 6 Remote Procedure call (RPC)](https://www.rabbitmq.com/tutorials/tutorial-six-dotnet.html)

Tutorial 6 示範了如何透過 RabbitMQ 完成可靠的 Remote Procedure Call。

## Diagram

```
                    Request
             replay_to=amqp.gen-X        Default
+--------+  correlate_id=random-gen   +----------+   routingKey: rpc_queue   +-----------+      +--------+
| Client | -------------------------> | exchange | ------------------------> | rpc_queue | ---> | Server |
+--------+                            +----------+                           +-----------+      +--------+
    ↑                                                                                                |
    |                    +------------+                            +----------+                      |
    └------------------- | amqp.gen-X | <------------------------- | exchange | <--------------------┘
                         +------------+    routingKey:amqp.gen-X   +----------+
                                                                      Default

``` 

---

## Prerequisites

請先使用以下指令建立一個可用的 RabbitMQ Server。

```
docker run -it --rm --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
``` 

---

## Demo Introduction

透過 RabbitMQ 實作 Remote Procedure Call 的基本概念就如同上面的圖一樣，在 Client 建立一個接收遠端程式執行結果訊息的 queue A，再將要執行的程式參數送到另一條獨立的 queue B；Server 端的 consumer 則從乘載參數的 queue B 取得參數開始執行程式，最後將結果回傳到 Client 指定的 queue A。

在範例 RPCClient.cs 中，我們在 publish 訊息到 queue 時，也夾帶了 `CorrelationId`、`ReplyTo` 這兩個屬性：
```cs
props = channel.CreateBasicProperties();
var correlationId = Guid.NewGuid().ToString();
props.CorrelationId = correlationId;
props.ReplyTo = replyQueueName;

//...

var messageBytes = Encoding.UTF8.GetBytes(message);
channel.BasicPublish(
    exchange: "",
    routingKey: "rpc_queue",
    basicProperties: props,
    body: messageBytes);
```

除了我們使用的這兩個屬性之外，AMQP 0-9-1 還提供了另外 12 種其他的屬性，下面只列出平常實務上較常用的屬性：

1. Persistent: 將訊息標註為 `Persistent` 時，RabbitMQ 會將該訊息保存在硬碟上，直到有 consumer 將其收下並回應。不過 RabbitMQ 的文件也有提到，將訊息標註為 `Persistent` 還是有可能會遺失訊息，因為 RabbitMQ 並不是隨時都會將訊息從記憶體轉存到硬碟上。
2. DeliveryMode: 這個參數的效果與上一個相同。
3. ContentType: MIME content type，若是要送 JSON 格式的物件就可以使用 `application/json`。
4. ReplyTo: 提供給 consmer 作為回覆的 queue。
5. CorrelationId: 識別用的 ID，實務上可能也會有許多的 producer 共享同一條 queue 來接收遠端執行的結果，在這種情境下，producer 就可以透過 `CorrelationId` 來辨識現在收到的這個結果，是不是從當初送出去的那條訊息送回的。

其他參數可以參考官方提供的 [C# API Reference](http://rabbitmq.github.io/rabbitmq-dotnet-client/api/RabbitMQ.Client.IBasicProperties.html)。

---

## Run demo

> 該範例的程式碼僅在 consumer 建立連線後宣告 queue 資訊，所以需要先啟動 consumer 的專案。  
> 請使用不同的 terminal 執行。

Run the consumer:

```
cd ReceiveLogs
// dotnet run [routing key]
dotnet run warning
```

Run the producer:

```
cd EmitLog
// dotnet run [routing key] [message]
// default routing key is info
dotnet run warning messageeeee
```