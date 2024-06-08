using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;
using System.Text;
using System.Text.Json;

var factory = new ConnectionFactory();
factory.Uri = new Uri("amqps://wzrfvrnm:Ty8g7emkfwJ0BqZdd2HoZ92oSbJ23FeH@beaver.rmq.cloudamqp.com/wzrfvrnm");

using var connection = factory.CreateConnection();
var channel = connection.CreateModel();
//channel.QueueDeclare("hello-queue", true, false, false);
//channel.ExchangeDeclare("logs-fanout", ExchangeType.Fanout, true);
//var randomQueue = channel.QueueDeclare().QueueName;
//var randomQueue = "log-database-save-queue";
//channel.QueueDeclare(randomQueue, true, false, false);
//channel.QueueBind(randomQueue, "logs-fanout", string.Empty);
var queueName = channel.QueueDeclare().QueueName;
Dictionary<string, object> headers = new();
headers.Add("format", "pdf");
headers.Add("shape", "a5");
headers.Add("x-match", "any");
//var routeKey = "Info.#";
channel.QueueBind(queueName, "exchange-header", string.Empty, headers);
channel.BasicQos(0, 1, false);
var consumer = new EventingBasicConsumer(channel);
channel.BasicConsume(queueName, false, consumer);
Console.WriteLine("Logs are listened");
consumer.Received += (object? sender, BasicDeliverEventArgs e) =>
{
    var message = Encoding.UTF8.GetString(e.Body.ToArray());
    var product = JsonSerializer.Deserialize<Product>(message);
    Console.WriteLine("Incoming message: " + product);
    channel.BasicAck(e.DeliveryTag, false);
    //File.AppendAllText("log-critical.txt", message + "\n");
    Thread.Sleep(1000);
};

Console.ReadLine();