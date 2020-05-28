# RabbitMQ toturial 1 HelloWorld!

說明如何透過官方的 package 建立基本的連線。
此範例中只有一個 Producer (Send 專案)、Consumer (Receive) 專案。

## Diagram

```
+---+      +---------+      +---+
| P | ---> |q|u|e|u|e| ---> | C |
+---+      +---------+      +---+
```

---

## Prerequisites

請先使用以下指令建立一個可用的 RabbitMQ Server。
```
docker run -it --rm --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

## Run demo

該範例的程式碼都有在建立連線後宣告 queue 資訊，所以啟動的先後順序並沒有差異。

> 請使用不同的 terminal 執行

Run the consumer:

```
cd Receive
dotnet run
```

Run the producer:

```
cd Send
dotnet run
```

兩個專案都啟動後應該會在啟用 Consumer 的 terminal 看到:

```
Press [enter] to exit.
[x] Receive Hello, World!
```

並在啟用 Producer 的 terminal 看到:

```
[x] Send Hello, World!
Press [enter] to exit.
```