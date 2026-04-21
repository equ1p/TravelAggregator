using Microsoft.EntityFrameworkCore;
using TravelAggregator.Application.Interfaces;
using TravelAggregator.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer("Server=localhost,1433;Database=TravelDb;User Id=sa;Password=TravelAggregator!123;TrustServerCertificate=True;"));

builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

var mySqlConn = "Server=localhost;Port=3306;Database=AuditLogsDb;Uid=root;Pwd=TravelAggregator!123;";
builder.Services.AddDbContext<AuditDbContext>(options =>
    options.UseMySql(mySqlConn, ServerVersion.AutoDetect(mySqlConn)));

builder.Services.AddSingleton(new MongoTravelCache("mongodb://admin:admin@localhost:27017", "TravelCacheDb"));

var app = builder.Build();

app.MapControllers();
app.Run();
