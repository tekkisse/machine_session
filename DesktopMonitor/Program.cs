using DesktopMonitor;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<RedisOptions>(
    builder.Configuration.GetSection("Redis"));

// Create Redis connection
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var opts = sp.GetRequiredService<IOptions<RedisOptions>>().Value;

    var config = new ConfigurationOptions
    {
        EndPoints = { $"{opts.Host}:{opts.Port}" },
        Password = opts.Password,
        Ssl = opts.UseTls,
        DefaultDatabase = opts.Database,
        AbortOnConnectFail = false
    };

    return ConnectionMultiplexer.Connect(config);
});

builder.Services.AddHostedService<RabbitMqConsumerService>();

builder.Services.AddSingleton<SessionStateService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
