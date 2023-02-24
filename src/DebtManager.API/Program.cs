using DebtManager.API.Models;
using DebtManager.Application.Common.Interfaces;
using DebtManager.Infrastructure.Contexts;
using DebtManager.Infrastructure.Repositories;
using DebtManager.Application.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DebtManagerContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DebtManagerDb"));
});
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddOptions<AuthSettings>().BindConfiguration("Authentication");
builder.Services.AddOptions<NotificationSettings>().BindConfiguration("Notification");
builder.Services.AddHttpClient<INotificationService, NotificationService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.ConfigureApi();

app.Run();