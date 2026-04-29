using Microsoft.EntityFrameworkCore;
using TravelAggregator.Application.Interfaces;
using TravelAggregator.Infrastructure.Persistence;
using TravelAggregator.Infrastructure.Adapters;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var sqlConn = builder.Configuration.GetConnectionString("SqlServerConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(sqlConn));

builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

var mySqlConn = builder.Configuration.GetConnectionString("MySqlConnection");
builder.Services.AddDbContext<AuditDbContext>(options =>
    options.UseMySql(mySqlConn, ServerVersion.AutoDetect(mySqlConn)));

var mongoConn = builder.Configuration.GetConnectionString("MongoDbConnection");
builder.Services.AddSingleton(new MongoTravelCache(mongoConn, "TravelCacheDb"));

builder.Services.AddHttpClient<ITravelProvider, DuffelAdapter>()
    .AddTransientHttpErrorPolicy(policyBuilder =>
        policyBuilder.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
    .AddTransientHttpErrorPolicy(policyBuilder =>
        policyBuilder.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));

var app = builder.Build();

app.MapControllers();
app.Run();
