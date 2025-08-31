using Autofac;
using Autofac.Extensions.DependencyInjection;
using MediatR;
using Microsoft.OpenApi.Models;
using PashaInsuranceFiltering.Application.Features.CQRS.Commands.UploadCommands;
using PashalinsuranceFiltering.DependencyInjection.DependencyResolvers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


// Autofac provider
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// MediatR (infrastructure)
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<UploadChunkCommand>();
    
});

// Autofac - custom module
builder.Host.ConfigureContainer<ContainerBuilder>(container =>
{
    container.RegisterModule(new AutofacBusinessModule(builder.Configuration));
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Filtering API", Version = "v1" });
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection(); 
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "v1"));
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.Run();
