using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory();
factory.Uri = new Uri("amqps://wzrfvrnm:Ty8g7emkfwJ0BqZdd2HoZ92oSbJ23FeH@beaver.rmq.cloudamqp.com/wzrfvrnm");

using var connection = factory.CreateConnection();
var channel = connection.CreateModel();
//channel.QueueDeclare("hello-queue", true, false, false);
//channel.ExchangeDeclare("logs-fanout", ExchangeType.Fanout, true);
//var randomQueue = channel.QueueDeclare().QueueName;
var randomQueue = "log-database-save-queue";
channel.QueueDeclare(randomQueue, true, false, false);
channel.QueueBind(randomQueue, "logs-fanout", string.Empty);
channel.BasicQos(0, 1, false);
var consumer = new EventingBasicConsumer(channel);
channel.BasicConsume(randomQueue, false, consumer);
Console.WriteLine("Logs are listened");
consumer.Received += (object? sender, BasicDeliverEventArgs e) =>
{
    var message = Encoding.UTF8.GetString(e.Body.ToArray());
    Console.WriteLine("Incoming message: " + message);
    channel.BasicAck(e.DeliveryTag, false);
    Thread.Sleep(1000);
};

Console.ReadLine();