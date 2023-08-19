using CRUDExample.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace CRUDExample.Filters.ActionFilters;

public class PersonsListActionFilter : IActionFilter
{
    private readonly ILogger<PersonsListActionFilter> _logger;

    public PersonsListActionFilter(ILogger<PersonsListActionFilter> logger)
    {
        _logger = logger;
    }

    //before action method
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // da bi mogli prebaciti actionarguments u OnActionExecuted
        context.HttpContext.Items["arguments"] = context.ActionArguments;

        _logger.LogInformation("{FilterName}.{MethodName} method", nameof(PersonsListActionFilter), nameof(OnActionExecuting));

        //parameter validation(searchBy)
        if (context.ActionArguments.ContainsKey("searchBy"))
        {
            string? searchBy = Convert.ToString(context.ActionArguments["searchBy"]);
            //validat the searchBy parameter value
            if (!string.IsNullOrEmpty(searchBy))
            {
                var searchByOptions = new List<string>()
                {
                    nameof(PersonResponse.PersonName),
                    nameof(PersonResponse.Email),
                    nameof(PersonResponse.DateOfBirth),
                    nameof(PersonResponse.Gender),
                    nameof(PersonResponse.CountryID),
                    nameof(PersonResponse.Address)
                };
                //ako imamo bar jedan matching parameter
                //reset the searchBy paramer value
                if (searchByOptions.Any(temp => temp == searchBy) == false)
                {
                    _logger.LogInformation("searchBy actual value {searchBy}", searchBy);
                    context.ActionArguments["searchBy"] = nameof(PersonResponse.PersonName);
                    _logger.LogInformation("searchBy updated value {searchBy}", context.ActionArguments["searchBy"]);
                }
            }
        }
        
    }
    //after action method
    public void OnActionExecuted(ActionExecutedContext context)
    {
        _logger.LogInformation("{FilterName}.{MethodName} method", nameof(PersonsListActionFilter), nameof(OnActionExecuted));

        // za pristup ViewData(posto ne mozemo direktno pristupiti preko context)
        PersonsController personsController = (PersonsController)context.Controller;

        // da bi mogli prebaciti actionarguments u OnActionExecuted indirektno preko OnActionExecuting
        IDictionary<string, object?>? parameters = (IDictionary<string, object?>?)context.HttpContext.Items["arguments"];

        if (parameters != null)
        {
            if (parameters.ContainsKey("searchBy"))
            {
                personsController.ViewData["CurrentSearchBy"] = Convert.ToString(parameters["searchBy"]);
            }

            if (parameters.ContainsKey("searchString"))
            {
                personsController.ViewData["CurrentSearchString"] = Convert.ToString(parameters["searchString"]);
            }

            if (parameters.ContainsKey("sortBy"))
            {
                personsController.ViewData["CurrentSortBy"] = Convert.ToString(parameters["sortBy"]);
            }
            else
            {
                personsController.ViewData["CurrentSortBy"] = nameof(PersonResponse.PersonName);
            }

            if (parameters.ContainsKey("sortOrder"))
            {
                personsController.ViewData["CurrentSortOrder"] = Convert.ToString(parameters["sortOrder"]);
            }
            else
            {
                personsController.ViewData["CurrentSortOrder"] = nameof(SortOrderOptions.ASC);
            }
        }

        personsController.ViewBag.SearchFields = new Dictionary<string, string>()
        {
            { nameof(PersonResponse.PersonName), "Person Name" },
            { nameof(PersonResponse.Email), "Email" },
            { nameof(PersonResponse.DateOfBirth), "Date of Birth" },
            { nameof(PersonResponse.Gender), "Gender" },
            { nameof(PersonResponse.CountryID), "Country" },
            { nameof(PersonResponse.Address), "Address" }
        };


    }


}
