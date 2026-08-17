using RabbitMQ.Client;
using RabbitMq.Common;
using System.Text;

// 用法:
//   dotnet run --project src/Producer                 -> 用默认 routingKey "order.created" 发 10 条
//   dotnet run --project src/Producer order.paid 5    -> routingKey=order.paid，发 5 条
//   dotnet run --project src/Producer "user.#" 3      -> routingKey=user.#，发 3 条

var config = new RabbitMqConfig();

var routingKey = args.Length > 0 ? args[0] : "order.created";
var count = args.Length > 1 && int.TryParse(args[1], out var c) && c > 0 ? c : 10;

using var connection = ConnectionFactoryBuilder.Connect(config);
using var channel = connection.CreateModel();

// 声明交换机（幂等，已存在且类型一致则跳过）
channel.ExchangeDeclare(exchange: config.ExchangeName, type: config.ExchangeType, durable: true);

// 生产者也声明并绑定持久化队列：这样即使消费者从未运行过，队列+绑定也已存在，
// 生产者先发消息也不会丢（消息缓存在 testtopic.queue，消费者上线后照收）。
channel.QueueDeclare(queue: config.QueueName, durable: true, exclusive: false, autoDelete: false);
channel.QueueBind(queue: config.QueueName, exchange: config.ExchangeName, routingKey: config.BindingKey);

Console.WriteLine($"生产者已连接 {config.HostName}:{config.Port}");
Console.WriteLine($"交换机='{config.ExchangeName}' (topic)  路由键='{routingKey}'  条数={count}");
Console.WriteLine($"预建队列='{config.QueueName}'  绑定键='{config.BindingKey}'");
Console.WriteLine(new string('-', 60));

for (var i = 0; i < count; i++)
{
    var message = $"[{i + 1}/{count}] hello from producer @ {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}";
    var body = Encoding.UTF8.GetBytes(message);

    var props = channel.CreateBasicProperties();
    props.Persistent = true;                       // 消息持久化，Broker 重启不丢
    props.MessageId = Guid.NewGuid().ToString();
    props.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

    channel.BasicPublish(
        exchange: config.ExchangeName,
        routingKey: routingKey,
        basicProperties: props,
        body: body);

    Console.WriteLine($" [x] 已发送: '{message}'  (routingKey={routingKey})");
    await Task.Delay(500);                         // 放慢一点，方便肉眼观察
}

Console.WriteLine(new string('-', 60));
Console.WriteLine("发送完毕。");
