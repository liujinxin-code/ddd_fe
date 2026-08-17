using RabbitMQ.Client;

namespace RabbitMq.Common;

/// <summary>
/// 统一的 ConnectionFactory 构造器：生产者 / 消费者共用同一套连接参数。
/// 开启了自动重连，断网后能自动恢复。
/// </summary>
public static class ConnectionFactoryBuilder
{
    public static ConnectionFactory Create(RabbitMqConfig config)
    {
        return new ConnectionFactory
        {
            HostName = config.HostName,
            Port = config.Port,
            UserName = config.UserName,
            Password = config.Password,
            VirtualHost = config.VirtualHost,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
            RequestedHeartbeat = TimeSpan.FromSeconds(30)
        };
    }

    /// <summary>
    /// 创建连接，并对常见错误给出更友好的提示（尤其是账号密码问题）。
    /// </summary>
    public static IConnection Connect(RabbitMqConfig config)
    {
        try
        {
            return Create(config).CreateConnection();
        }
        catch (RabbitMQ.Client.Exceptions.AuthenticationFailureException ex)
        {
            throw new InvalidOperationException(
                $"RabbitMQ 登录被拒绝：请检查用户名/密码是否正确（当前用户名='{config.UserName}'）。" +
                $"可用环境变量 RABBITMQ_USERNAME / RABBITMQ_PASSWORD 覆盖。详情: {ex.Message}", ex);
        }
        catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException ex)
        {
            throw new InvalidOperationException(
                $"无法连接 RabbitMQ（{config.HostName}:{config.Port}）：请确认服务已启动且端口可达。" +
                $"详情: {ex.Message}", ex);
        }
    }
}
