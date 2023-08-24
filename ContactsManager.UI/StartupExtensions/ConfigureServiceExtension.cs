using ContactsManager.Core.Domain.IdentityEntities;
using CRUDExample.Filters.ActionFilters;
using Enttities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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

        // da mozemo inject u bilo koju class
        services.AddTransient<PersonsListActionFilter>();

        //service za di dbContext i UseSqlServer za db connection
        services.AddDbContext<ApplicationDbContext>(opts =>
        {
            //builder.Configuration... => configuration
            opts.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });
        

        //Enable Identity in this Project
        //create data
        //application layer level
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequiredLength = 9;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequiredUniqueChars = 1; //Cr83ttdw$
        })
            //store data
            .AddEntityFrameworkStores<ApplicationDbContext>()
            //to generate OTP(one time password) and sent email to user, to user reenter that password to confirm user account
            .AddDefaultTokenProviders()
            //repository layer level
            .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()

            .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid>>();

        //Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=PersonsDatabase;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False

        services.AddAuthorization(options =>
        {
            //enforces authorization policy(user must be authenticated) for all the action methods
            options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            // kreiranje custom Autorization policy
            options.AddPolicy("NotAuthorized", policy =>
            {
                //ovo ce biti executed kad we appied NotAuthorized code
                policy.RequireAssertion(context =>
                {
                    //return true; //true- user ima pristup/false - nema
                    // ako user nije loged in IsAuthenticated ce biti false
                    return !context.User.Identity.IsAuthenticated; //vraca false
                });
            });
        });

        services.ConfigureApplicationCookie(options =>
        {
            // ako user nije loged in, automatski se redirectuje na ovaj url (povezano sa AddAuthorization)
            options.LoginPath = "/Account/Login";
        });

        services.AddHttpLogging(opt =>
        {
            opt.LoggingFields = HttpLoggingFields.RequestProperties | HttpLoggingFields.ResponsePropertiesAndHeaders;
        });

        return services;
    }
}
