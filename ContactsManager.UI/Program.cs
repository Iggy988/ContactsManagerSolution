using Services;
using ServiceContracts;
using Enttities;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;
using Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.HttpLogging;
using Serilog;
using CRUDExample.Filters.ActionFilters;
using CRUDExample;
using CRUDExample.Middleware;

var builder = WebApplication.CreateBuilder(args);

//Logging
//builder.Host.ConfigureLogging(loggingProvider => 
//{
//    loggingProvider.ClearProviders();
//    // dodajemo logging provider koji zelimo exoplicitno
//    loggingProvider.AddConsole(); 
//    loggingProvider.AddDebug();
//    loggingProvider.AddEventLog();
//});

//Serilog
builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider services, LoggerConfiguration loggerConfiguration) =>
{
    loggerConfiguration
    //read configuration settings from built -in IConfiguration
    .ReadFrom.Configuration(context.Configuration)
    //read out current app services and make them available to serilog
    .ReadFrom.Services(services);
});

builder.Services.ConfigureServices(builder.Configuration);



var app = builder.Build();


//create application pipeline
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    // redirection url
    app.UseExceptionHandler("/Error");
    app.UseExceptionHandlingMiddleware();
}

app.UseSerilogRequestLogging();

//http logging
app.UseHttpLogging();


if (builder.Environment.IsEnvironment("Test")==false)
    Rotativa.AspNetCore.RotativaConfiguration.Setup(rootPath:"wwwroot", wkhtmltopdfRelativePath:"Rotativa");

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();

//make the auto-generated Program accessible programatically
public partial class Program { }
