using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.CreateExcel;
using RabbitMQ.CreateExcel.Models;
using RabbitMQ.CreateExcel.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddDbContext<AdventureWorks2019Context>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
});

builder.Services.AddSingleton(sp => new ConnectionFactory()
{
    Uri = new(builder.Configuration.GetConnectionString("RabbitMQ")),
    DispatchConsumersAsync = true
});
builder.Services.AddSingleton<RabbitMQClientService>();

var host = builder.Build();
host.Run();
