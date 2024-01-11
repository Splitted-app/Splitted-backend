using CsvConversion;
using CsvConversion.Readers;
using Splitted_backend.Extensions;
using Splitted_backend.Models.Entities;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureServices(builder.Configuration);
builder.Services.ConfigureDbContexts(builder.Configuration);
builder.Services.ConfigureAuthentication(builder.Configuration);
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(config => config.EnableAnnotations()) ;

var app = builder.Build();

app.UseCors("Allowed origins");
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
