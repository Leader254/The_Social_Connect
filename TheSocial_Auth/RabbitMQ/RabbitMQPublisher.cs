using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace TheSocial_Auth.RabbitMQ
{
    public class RabbitMQPublisher : IRabbitMQPublisher
    {

        private IConnection _connection;

        public void PublishMessage(object message, string queue_topic_name)
        {
            if (checkConnection())
            {
                var channel = _connection.CreateModel();
                channel.QueueDeclare(queue_topic_name, false, false, false, null);
                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);
                channel.BasicPublish(exchange: "", routingKey: queue_topic_name, null, body: body);
            }
        }

        public void createConnection()
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = "localhost",
                    UserName = "guest",
                    Password = "guest"
                };
                _connection = factory.CreateConnection();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public bool checkConnection()
        {
            if(_connection != null )
            {
                return true;
            }
            createConnection();
            return true;
        }
    }
}
