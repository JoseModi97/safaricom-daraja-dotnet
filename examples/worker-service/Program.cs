using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Safaricom.Daraja.Models;
using WorkerServiceExample;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton(new DarajaConfig()); // reads DARAJA_CONSUMER_KEY / DARAJA_CONSUMER_SECRET / DARAJA_ENVIRONMENT
builder.Services.AddSingleton(sp => new Safaricom.Daraja.DarajaClient(sp.GetRequiredService<DarajaConfig>()));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
