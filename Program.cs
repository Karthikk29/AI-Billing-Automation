using BillingAutomation.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register application services
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<IAIService, AIService>();

// MongoDB Configuration
builder.Services.AddSingleton<MongoDB.Driver.IMongoClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connString = config["MongoDB:ConnectionString"];
    return new MongoDB.Driver.MongoClient(connString);
});

builder.Services.AddScoped<MongoDB.Driver.IMongoDatabase>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var client = sp.GetRequiredService<MongoDB.Driver.IMongoClient>();
    var dbName = config["MongoDB:DatabaseName"];
    return client.GetDatabase(dbName);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
