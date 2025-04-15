using Confluent.Kafka;

namespace Application.Common.Interfaces.Utils;

public interface IKafkaProducer
{
    Task ProduceAsync(string topic, Message<string, string> message);
}