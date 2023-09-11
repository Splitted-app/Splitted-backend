using CsvConversion;
using CsvConversion.Readers;
using Splitted_backend.Extensions;
using Splitted_backend.Models.Entities;
using System.Text;

public static class MainClass
{
    public static void Main(string [] args)
    {
        string path = "C:\\Users\\Mateusz\\Desktop\\Programowanko\\Praca in¿ynierska\\CSvki\\Pko1.csv";
        BaseCsvReader reader = new PkoCsvReader(path);
        var transactions = reader.GetTransactions();
        int i = 1;
        foreach (var item in transactions)
        {
            Console.WriteLine(i++ + " " + item.TransactionType + " " + item.Description);
        }
    }
}

 

//var builder = WebApplication.CreateBuilder(args);

//builder.Services.ConfigureServices(builder.Configuration);
//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();
//app.UseAuthorization();
//app.MapControllers();
//app.Run();
