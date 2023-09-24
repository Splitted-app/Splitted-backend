using CsvConversion;
using CsvConversion.Readers;
using Splitted_backend.Extensions;
using Splitted_backend.Models.Entities;
using System.Text;

//public static class MainClass
//{
//    public static void Main(string [] args)
//    {
//        string path = "C:\\Users\\Mateusz\\Desktop\\Programowanko\\Praca in¿ynierska\\CSvki\\Santander-2.csv";
//        BaseCsvReader reader = new SantanderCsvReader(path);
//        var transactions = reader.GetTransactions();
//        int i = 1;
//        foreach (var item in transactions)
//        {
//            Console.WriteLine(i++ + " " + item.TransactionType + " " + item.Description);
//        }
//    }
//}

 

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureServices(builder.Configuration);
builder.Services.ConfigureAuthentication(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(config => config.EnableAnnotations());

var app = builder.Build();

app.UseCors("Allowed origins");
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
