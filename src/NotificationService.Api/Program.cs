using NotificationService.Application;
using NotificationService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ===== Services Registration =====

// Application Layer (MediatR, FluentValidation, Behaviors)
builder.Services.AddApplication();

// Infrastructure Layer (DbContext, Repositories, External Services)
builder.Services.AddInfrastructure(builder.Configuration);

// API Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ===== HTTP Pipeline =====

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
