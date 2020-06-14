# [RabbitMQ toturial 5 topic](https://www.rabbitmq.com/tutorials/tutorial-five-dotnet.html)

延續 toturial 3 & 4，以實例說明如何使用 Topic exchange，來將訊息送到綁定在特定 `routing key` 的 queue。

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

Topic exchange 的效果類似於 Direct exchange 及 Direct exchange 的綜合體，它會以什麼方式發布訊息到 queue 都是由 routing key 來決定。

Topic exchange 的 Routing key 有三個需要特別注意的符號，分別是

1. `.`：小數點，用來分隔兩個不同的單字舉例來說，`example.key` 是一個合法的 routing key，exchange 會將所有 `example.key` 的訊息丟到綁定的 queue 中。其效果就與 Direct exchange 相同。
2.  `*`：星星符號，表示至少有一個字，舉例來說，`*.key`，就代表 exchange 會將 `exampleA.key`、`exampleC.key` 等符合 `*.key` 的訊息丟到綁定的 queue，而 `key`、`example.b.key` 這類型的 routing key 就不會被送到綁定在 `*.key` 的 queue 中。
3. `#`：井字號，表示可有可無，舉例來說，`#.#.key`，代表只要收到的訊息，其 routing key 的被 `.` 分割的最後一個字是 `key`，exchange 就會將該訊息丟到綁定的 queue。

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