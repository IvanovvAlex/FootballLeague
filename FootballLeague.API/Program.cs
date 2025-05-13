using FootballLeague.API.Middlewares;
using FootballLeague.API.ServiceExtensions;
using FootballLeague.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient; // For SqlConnection

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Retrieve connection string
string? connectionString = builder.Configuration.GetConnectionString("DbConnnectionString")
    ?? builder.Configuration["ConnectionStrings:DbConnnectionString"];
if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Warning: Connection string is missing. The app will not run, please provide a connection string in the appsettings.json file.");
    return;
}
else
{
    // Validate the connection string
    try
    {
        using (SqlConnection sqlConnection = new SqlConnection(connectionString))
        {
            sqlConnection.Open();
            Console.WriteLine("Database connection successful.");
            sqlConnection.Close();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: Unable to connect to the database. Please check your connection string. Details: {ex.Message}");
        return;
    }

    // Register services
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));

    builder.Services.AddCustomServices();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddControllers()
        .AddJsonOptions(options => options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull);
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    WebApplication app = builder.Build();

    app.UseMiddleware<ExceptionHandlerMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Ensure the database is created
    using (IServiceScope scope = app.Services.CreateScope())
    {
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();

        DataSeeder dataSeeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
        await dataSeeder.SeedAsync();
    }

    app.UseHttpsRedirection();
    app.MapControllers();

    app.Run();
}