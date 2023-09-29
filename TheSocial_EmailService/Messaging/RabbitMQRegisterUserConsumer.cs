using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using TheSocial_EmailService.Models.Dtos;
using TheSocial_EmailService.SendMail;

namespace TheSocial_EmailService.Messaging
{
    public class RabbitMQRegisterUserConsumer : BackgroundService
    {
        private readonly IConnection _connection;
        private IModel _channel;
        private IConfiguration _configuration;
        private readonly SendMailService _sendMailService;

        public RabbitMQRegisterUserConsumer(IConfiguration configuration)
        {
            _configuration = configuration;
            _sendMailService = new SendMailService();
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(_configuration.GetSection("QueuesandTopic:RegisterUser").Get<string>(), false, false, false, null);
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (sender, e) =>
            {
                var content = Encoding.UTF8.GetString(e.Body.ToArray());
                var userMessage = JsonConvert.DeserializeObject<UserMessage>(content);

                sendMail(userMessage).GetAwaiter().GetResult();
                _channel.BasicAck(e.DeliveryTag, false);
            };
            _channel.BasicConsume(_configuration.GetSection("QueuesandTopic:RegisterUser").Get<string>(), false, consumer);
            return Task.CompletedTask;
        }
        private async Task sendMail(UserMessage userMessage)
        {
            //send email
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append($"<h1>Hi {userMessage.Name},</h1>");
                sb.Append($"<h2>Thank you for registering with us.</h2>");
                sb.Append("<img src=\"https://cdn.pixabay.com/photo/2023/04/20/10/19/coding-7939372_1280.jpg\" width=\"1000\" height=\"600\">");
                sb.Append($"<img src=\"https://cdn.pixabay.com/photo/2014/06/04/16/41/thank-you-362164_1280.jpg\" width=\"1000\" height=\"600\">");
                sb.Append($"<h2>You are so much welcome.</h2>");
                sb.AppendLine("<br/>");
                //new line
                sb.Append($"<p>Regards,</p>");
                sb.Append($"<p>TheSocial App</p>");

                await _sendMailService.SendMail(userMessage, sb.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
