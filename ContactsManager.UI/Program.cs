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
using Microsoft.AspNetCore.Builder;

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

// prebacujemo app na https
app.UseHsts();// force browser to use https
app.UseHttpsRedirection();

app.UseSerilogRequestLogging();

//http logging
app.UseHttpLogging();


if (builder.Environment.IsEnvironment("Test")==false)
    Rotativa.AspNetCore.RotativaConfiguration.Setup(rootPath:"wwwroot", wkhtmltopdfRelativePath:"Rotativa");

app.UseStaticFiles();


app.UseRouting();// Identifing action method based route
//moramo dodati Authentication middleware - cita auth cookie i ekstraktuje user name i dr. podatke -> budu dostupni u user propertiju
app.UseAuthentication();// Reading Identity cookie
app.UseAuthorization(); //Validates access permition of the user
app.MapControllers(); // Execute filter pipline (action + filters)

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}" //exixts -obavezan element
        //Admin/Home/Index == Admin(kad stavimo default value)
        );

    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller}/{action}/{id?}"
        );
});

app.Run();

//make the auto-generated Program accessible programatically
public partial class Program { }
