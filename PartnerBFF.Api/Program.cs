using FluentValidation;
using FluentValidation.AspNetCore;
using PartnerBFF.Application;
using PartnerBFF.Persistence;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<TransactionRequestValidator>();

var rabbitMqHost = builder.Configuration["RabbitMq:HostName"] ?? "localhost";
var rabbitMqPublisher = await RabbitMqPublisher.CreateAsync(rabbitMqHost);
builder.Services.AddSingleton<IMessagePublisher>(rabbitMqPublisher);

builder.Services.AddHttpClient<IPartnerVerificationService, PartnerVerificationService>(client => 
    {
        client.BaseAddress = new Uri("http://localhost:5167/");
    })
    .AddResilienceHandler("partner-verification-pipeline", (pipelineBuilder, context) =>
    {
        pipelineBuilder.AddPipeline(ResiliencePolicyFactory.CreatePartnerVerificationPipeline());
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
