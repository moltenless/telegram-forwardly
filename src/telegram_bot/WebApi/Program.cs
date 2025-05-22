using Microsoft.EntityFrameworkCore;
using TelegramForwardly.DataAccess.Context;
using TelegramForwardly.DataAccess.Repositories;
using TelegramForwardly.DataAccess.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ForwardlyContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null
        )
    )
);

builder.Services.AddScoped<IClientCurrentStatesRepository, ClientCurrentStatesRepository>();

builder.Services.AddControllers();

// https://aka.ms/aspnetcore/swashbuckle

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

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ForwardlyContext>();
    await context.Database.MigrateAsync();
}

app.MapControllers();

await app.RunAsync();
