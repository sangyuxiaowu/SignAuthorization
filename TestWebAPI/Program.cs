using Sang.AspNetCore.SignAuthorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSignAuthorization();

app.MapControllers();

app.Run();
