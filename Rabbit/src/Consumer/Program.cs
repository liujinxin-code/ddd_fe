using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMq.Common;
using System.Text;

// 用法:
//   dotnet run --project src/Consumer                          -> 队列 testtopic.queue，绑定 "#" 收全部
//   dotnet run --project src/Consumer "order.*"                -> 只收 order. 开头的消息
//   dotnet run --project src/Consumer "*.created" "q.app1"     -> 具名队列 q.app1，只收 *.created
//   多个订阅者请用不同队列名（如 q.app1 / q.app2）实现“每人一份”的发布订阅。

var config = new RabbitMqConfig();

var bindingKey = args.Length > 0 ? args[0] : "#";
var queueName = args.Length > 1 ? args[1] : config.QueueName;

using var connection = ConnectionFactoryBuilder.Connect(config);
using var channel = connection.CreateModel();

// 声明交换机（与生产者保持一致，避免生产者先启动报错）
channel.ExchangeDeclare(exchange: config.ExchangeName, type: config.ExchangeType, durable: true);

// 具名 + 持久化队列：消费者离线时消息会在队列里排队，上线后照收（不再丢消息）
channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);
channel.QueueBind(queue: queueName, exchange: config.ExchangeName, routingKey: bindingKey);

Console.WriteLine($"消费者已连接 {config.HostName}:{config.Port}");
Console.WriteLine($"队列='{queueName}'  绑定键='{bindingKey}'  (CTRL+C 退出)");
Console.WriteLine(new string('-', 60));

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (_, ea) =>
{
    var message = Encoding.UTF8.GetString(ea.Body.ToArray());
    Console.WriteLine($" [x] 收到: '{message}'  (routingKey={ea.RoutingKey})");
};

channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

// 优雅等待 CTRL+C
var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};
try
{
    await Task.Delay(Timeout.Infinite, cts.Token);
}
catch (TaskCanceledException)
{
    Console.WriteLine("\n正在退出...");
}
