using CRUDExample.Filters.ActionFilters;
using Enttities;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Repositories;
using RepositoryContracts;
using ServiceContracts;
using Services;

namespace CRUDExample;

public static class ConfigureServiceExtension
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<ResponseHeaderActionFilter>();

        //it adds controllers and views as services
        services.AddControllersWithViews(options => {
            //options.Filters.Add<ResponseHeaderActionFilter>(5);

            var logger = services.BuildServiceProvider().GetRequiredService<ILogger<ResponseHeaderActionFilter>>();

            options.Filters.Add(new ResponseHeaderActionFilter(logger)
            {
                Key = "My-Key-From-Global",
                Value = "My-Value-From-Global",
                Order = 2
            });
        });


        //add services into IoC container
        services.AddScoped<ICountriesRepository, CountriesRepository>();
        services.AddScoped<IPersonsRepository, PersonsRepository>();

        services.AddScoped<ICountriesGetterService, CountriesGetterService>();
        services.AddScoped<ICountriesAdderService, CountriesAdderService>();
        services.AddScoped<ICountriesUploaderService, CountriesUploaderService>();

        services.AddScoped<IPersonsGetterService, PersonsGetterServiceWithFewExcelFields>();
        services.AddScoped<PersonsGetterService, PersonsGetterService>();

        services.AddScoped<IPersonsAdderService, PersonsAdderService>();
        services.AddScoped<IPersonsDeleterService, PersonsDeleterService>();
        services.AddScoped<IPersonsUpdaterService, PersonsUpdaterService>();
        services.AddScoped<IPersonsSorterService, PersonsSorterService>();

        //service za di dbContext i UseSqlServer za db connection
        services.AddDbContext<ApplicationDbContext>(opts =>
        {
            //builder.Configuration... => configuration
            opts.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });
        // da mozemo inject u bilo koju class
        services.AddTransient<PersonsListActionFilter>();

        //Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=PersonsDatabase;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False

        services.AddHttpLogging(opt =>
        {
            opt.LoggingFields = HttpLoggingFields.RequestProperties | HttpLoggingFields.ResponsePropertiesAndHeaders;
        });

        return services;
    }
}
