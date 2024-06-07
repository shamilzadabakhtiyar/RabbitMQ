using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory();
factory.Uri = new Uri("amqps://wzrfvrnm:Ty8g7emkfwJ0BqZdd2HoZ92oSbJ23FeH@beaver.rmq.cloudamqp.com/wzrfvrnm");

using var connection = factory.CreateConnection();
var channel = connection.CreateModel();
//channel.QueueDeclare("hello-queue", true, false, false);
channel.ExchangeDeclare("logs-fanout", ExchangeType.Fanout, true);
Enumerable.Range(1, 50).ToList().ForEach(x =>
{
    string message = $"log {x}";
    var messageBody = Encoding.UTF8.GetBytes(message);
    channel.BasicPublish("logs-fanout", string.Empty, null, messageBody);
    Console.WriteLine($"Message sent: {message}");
});

Console.ReadLine();