using Confluent.Kafka;

namespace Application.Common.Interfaces;

public interface IKafkaProducer
{
    Task ProduceAsync(string topic, Message<string, string> message);
}