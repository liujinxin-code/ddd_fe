namespace RabbitMq.Common;

/// <summary>
/// RabbitMQ 连接配置。默认值已经帮你填好了服务器 / 密码 / Topic。
/// 任意字段都可用环境变量覆盖，方便在不改代码的前提下切换账号：
///   RABBITMQ_HOST / RABBITMQ_PORT / RABBITMQ_USERNAME / RABBITMQ_PASSWORD /
///   RABBITMQ_VHOST / RABBITMQ_EXCHANGE / RABBITMQ_QUEUE
/// 注意：明文密码仅用于本地测试，生产环境请改用 Secret 管理。
/// </summary>
public class RabbitMqConfig
{
    public string HostName { get; set; } = Env("RABBITMQ_HOST", "49.234.58.209");
    public int Port { get; set; } = int.Parse(Env("RABBITMQ_PORT", "5672"));
    public string UserName { get; set; } = Env("RABBITMQ_USERNAME", "admin");
    public string Password { get; set; } = Env("RABBITMQ_PASSWORD", "Liu1779370304");
    public string VirtualHost { get; set; } = Env("RABBITMQ_VHOST", "ddd");
    public string ExchangeName { get; set; } = Env("RABBITMQ_EXCHANGE", "TestTopic");

    /// <summary>交换机类型：topic / direct / fanout / headers</summary>
    public string ExchangeType { get; set; } = Env("RABBITMQ_EXCHANGE_TYPE", "topic");

    /// <summary>消费者队列名（具名 + 持久化，使消息在消费者离线时也能排队）。每个订阅者用不同名字即为独立一份。</summary>
    public string QueueName { get; set; } = Env("RABBITMQ_QUEUE", "testtopic.queue");

    /// <summary>生产者侧为队列建立的绑定键（默认 # 即接收全部）。与消费者绑定键保持一致可避免重复投递。</summary>
    public string BindingKey { get; set; } = Env("RABBITMQ_BINDING_KEY", "#");

    private static string Env(string key, string fallback) =>
        Environment.GetEnvironmentVariable(key) ?? fallback;
}
