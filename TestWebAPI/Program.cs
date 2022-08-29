using Sang.AspNetCore.SignAuthorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSignAuthorization(opt => {
    opt.sToken = "you-api-token";
});

app.MapControllers();

app.Run();
