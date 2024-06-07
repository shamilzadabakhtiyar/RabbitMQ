using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory();
factory.Uri = new Uri("amqps://wzrfvrnm:Ty8g7emkfwJ0BqZdd2HoZ92oSbJ23FeH@beaver.rmq.cloudamqp.com/wzrfvrnm");

using var connection = factory.CreateConnection();
var channel = connection.CreateModel();
//channel.QueueDeclare("hello-queue", true, false, false);
channel.ExchangeDeclare("logs-direct", ExchangeType.Direct, true);

Enum.GetNames(typeof(LogNames)).ToList().ForEach(x =>
{
    var routeKey = $"route-{x}";
    var queueName = $"direct-queue-{x}";
    channel.QueueDeclare(queueName, true, false, false);
    channel.QueueBind(queueName, "logs-direct", routeKey);
});

Enumerable.Range(1, 50).ToList().ForEach(x =>
{
    LogNames log = (LogNames)new Random().Next(0, 4);
    string message = $"log-type: {log}";
    var messageBody = Encoding.UTF8.GetBytes(message);
    var routeKey = $"route-{log}";
    channel.BasicPublish("logs-direct", routeKey, null, messageBody);
    Console.WriteLine($"Log sent: {message}");
});

Console.ReadLine();

public enum LogNames
{
    Critical,
    Error,
    Warning,
    Info
}