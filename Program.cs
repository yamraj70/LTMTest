using LTMPorjectTes.Implimentaion;
using LTMPorjectTes.Interface;
using LTMPorjectTes.Models;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllers();
builder.Services.Configure<Settings>(
    builder.Configuration.GetSection("HackerNewsApi"));

builder.Services.AddMemoryCache();

builder.Services.AddHttpClient<IService, Services>();
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
