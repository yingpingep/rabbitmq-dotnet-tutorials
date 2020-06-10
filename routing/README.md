# [RabbitMQ toturial 4 work queue](https://www.rabbitmq.com/tutorials/tutorial-four-dotnet.html)

延續 toturial 3，以實例說明如何使用 Direct exchange，來將訊息送到綁定在特定 `routing key` 的 queue。

## Diagram

```
+----+   routingKey: a  +----------+   routingKey: a   +-------+      +----+
| P1 | ---------------> | exchange | ----------------> | queue | ---> | c1 |
+----+                  |   logs   |                   |  qa1  |      +----+
                        +----------+                   +-------+
                        ↑          |
+----+   routingKey: b  |          |   routingKey: b   +-------+      +----+
| P2 | -----------------┘          ├-----------------> | queue | ---> | c2 |
+----+                             |                   |  qb2  |      +----+
                                   |                   +-------+             
                                   |
                                   |   routingKey: b   +-------+      +----+
                                   └-----------------> | queue | ---> | c3 |
                                                       |  qb3  |      +----+
                                                       +-------+
```

---

## Prerequisites

請先使用以下指令建立一個可用的 RabbitMQ Server。

```
docker run -it --rm --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
``` 

---

## Demo Introduction

```cs
// EmitLog.cs
// Direct exchange
channel.BasicPublish(exchange: "direct_logs",
                     routingKey: severity,
                     basicProperties: null,
                     body: body);
```

```cs
// EmitLog.cs
// Fanout exchange
channel.BasicPublish(exchange: "fanout_logs",
                     routingKey: "",
                     basicProperties: null,
                     body: body);
```

上面兩段程式碼分別展示了 Direct exchange 與 Fanout exchange 在使用階段的不同，Fanout exchange 會直接將訊息送到指定的 exchange 並完全忽略給予的 routing key。

```cs
// ReceiveLogs.cs
// Direct exchange
channel.QueueBind(queue: queueName,
                  exchange: "direct_logs",
                  routingKey: key);
```

```cs
// ReceiveLogs.cs
// Fanout exchange
channel.QueueBind(queue: queueName,
                  exchange: "fanout_logs",
                  routingKey: "");
```

consumer 的部分也與 producer 類似，在進行 `QueueBind` 時，採用 Fanout exchange 做法的 consumer 會忽略 routing key 的值，而直接從 `fanout_logs` 這個 exchange 取得訊息。

一般而言 Fanout exchange 適用於 multicast 的情境，但實際上 Direct exchange 的設計也可以透過 binding 到相同的 routing key 及 exchange 來達成 Fanout exchange 的效果。

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