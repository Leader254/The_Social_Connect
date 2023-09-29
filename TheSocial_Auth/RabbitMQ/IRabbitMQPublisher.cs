namespace TheSocial_Auth.RabbitMQ
{
    public interface IRabbitMQPublisher
    {
        void PublishMessage(object message, string queue_topic_name);
    }
}
