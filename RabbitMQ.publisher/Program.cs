using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory();
factory.Uri = new Uri("amqps://wzrfvrnm:Ty8g7emkfwJ0BqZdd2HoZ92oSbJ23FeH@beaver.rmq.cloudamqp.com/wzrfvrnm");

using var connection = factory.CreateConnection();
var channel = connection.CreateModel();
//channel.QueueDeclare("hello-queue", true, false, false);
channel.ExchangeDeclare("logs-topic", ExchangeType.Topic, true);

Random random = new();

//Enum.GetNames(typeof(LogNames)).ToList().ForEach(x =>
//{
//    LogNames log1 = (LogNames)new Random().Next(0, 4);
//    LogNames log2 = (LogNames)new Random().Next(0, 4);
//    LogNames log3 = (LogNames)new Random().Next(0, 4);

//    var routeKey = $"{log1}{log2}{log3}";
//    var queueName = $"direct-queue-{x}";
//    channel.QueueDeclare(queueName, true, false, false);
//    channel.QueueBind(queueName, "logs-direct", routeKey);
//});

Enumerable.Range(1, 50).ToList().ForEach(x =>
{
    LogNames log1 = (LogNames)new Random().Next(0, 4);
    LogNames log2 = (LogNames)new Random().Next(0, 4);
    LogNames log3 = (LogNames)new Random().Next(0, 4);

    string message = $"log-type: {log1}-{log2}-{log3}";
    var messageBody = Encoding.UTF8.GetBytes(message);

    var routeKey = $"{log1}.{log2}.{log3}";
    channel.BasicPublish("logs-topic", routeKey, null, messageBody);
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