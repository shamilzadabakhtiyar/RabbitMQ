using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.CreateExcel.Models;
using RabbitMQ.CreateExcel.Services;
using Shared;
using System.Data;
using System.Text;
using System.Text.Json;

namespace RabbitMQ.CreateExcel
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly RabbitMQClientService _rabbitMQClientService;
        private readonly IServiceProvider _serviceProvider;
        private IModel _channel;

        public Worker(ILogger<Worker> logger, RabbitMQClientService rabbitMQClientService, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _rabbitMQClientService = rabbitMQClientService;
            _serviceProvider = serviceProvider;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _channel = _rabbitMQClientService.Connect();
            _channel.BasicQos(0, 1, false);
            return base.StartAsync(cancellationToken);
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            _channel.BasicConsume(RabbitMQClientService.QueueName, false, consumer);
            consumer.Received += Consumer_Received;
            return Task.CompletedTask;
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            var createdExcelMessage = JsonSerializer.Deserialize<CreateExcelMessage>(Encoding.UTF8.GetString(@event.Body.ToArray()));

            using var ms = new MemoryStream();
            var wb = new XLWorkbook();
            var ds = new DataSet();
            ds.Tables.Add(await GetTable("products"));
            wb.Worksheets.Add(ds);
            wb.SaveAs(ms);

            MultipartFormDataContent multipartFormDataContent = new();
            multipartFormDataContent.Add(new ByteArrayContent(ms.ToArray()), "file", Guid.NewGuid().ToString() + ".xlsx");

            var baseUrl = "https://localhost:44340/api/files";
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync($"{baseUrl}?fileId={createdExcelMessage.FileId}", multipartFormDataContent);
                if (response.IsSuccessStatusCode)
                {
                    _channel.BasicAck(@event.DeliveryTag, false);
                    _logger.LogInformation($"File id {createdExcelMessage.FileId} created successfully");
                }
            }
        }

        private async Task<DataTable> GetTable(string tableName)
        {
            List<Models.Product> products;
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AdventureWorks2019Context>();
                products = await context.Products.ToListAsync();
            }

            DataTable table = new(tableName);

            table.Columns.Add(nameof(Models.Product.ProductId), typeof(int));
            table.Columns.Add(nameof(Models.Product.Name), typeof(string));
            table.Columns.Add(nameof(Models.Product.ProductNumber), typeof(string));
            table.Columns.Add(nameof(Models.Product.Color), typeof(string));

            products.ForEach(x =>
            {
                table.Rows.Add(x.ProductId, x.Name, x.ProductNumber, x.Color);
            });

            return table;
        }
    }
}
