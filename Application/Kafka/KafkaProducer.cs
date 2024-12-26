using Application.Common.Interfaces;
using Confluent.Kafka;

namespace Application.Kafka;

public class KafkaProducer : IKafkaProducer
{
    private readonly IProducer<string, string> _producer;
    
    public KafkaProducer(IAppSettings appSettings)
    {
        var config = new ConsumerConfig
        {
            GroupId = "test-consumer-group",
            BootstrapServers = appSettings.KafkaHost,
            MessageMaxBytes = 10485760,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public Task ProduceAsync(string topic, Message<string, string> message)
    {
        return _producer.ProduceAsync(topic, message);
    }
}