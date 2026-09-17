using Safaricom.Daraja.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDaraja(builder.Configuration);

var app = builder.Build();

app.MapControllers();

app.Run();
