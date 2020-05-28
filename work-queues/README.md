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

該範例架構延續 [toturial 1](https://github.com/yinnping/rabbitmq-dotnet-tutorials/tree/master/hello-world)，只有變動少量的程式碼，讓 consumer 模擬需要花時間處理複雜工作的情境。 

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