using CRUDExample.Filters;
using CRUDExample.Filters.ActionFilters;
using CRUDExample.Filters.AuthorizationFilter;
using CRUDExample.Filters.ExceptionFilters;
using CRUDExample.Filters.ResourceFilters;
using CRUDExample.Filters.ResultFilters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rotativa.AspNetCore;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace CRUDExample.Controllers;

[Route("[controller]")]
//parameterize action filter
//[TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] { "My-Key-From-Class-Controller", "My-Value-From-Class-Controller", 3 }, Order = 3)]  //logger(ne stavljamo), key, value 
[ResponseHeaderFilterFactory("My-Key-From-Class-Controller", "My-Value-From-Class-Controller", 3)]
//[TypeFilter(typeof(HandleExceptionFilter))]
[TypeFilter(typeof(PersonAlwaysRunResultFilter))]

public class PersonsController : Controller
{
    //private fields
    private readonly IPersonsGetterService _personsGetterService;
    private readonly IPersonsAdderService _personsAdderService;
    private readonly IPersonsSorterService _personsSorterService;
    private readonly IPersonsDeleterService _personsDeleterService;
    private readonly IPersonsUpdaterService _personsUpdaterService;

    private readonly ICountriesGetterService _countriesService;
    private readonly ILogger<PersonsController> _logger;

    //constructor
    public PersonsController(IPersonsGetterService personsGetterService, IPersonsAdderService personsAdderService, IPersonsDeleterService personsDeleterService, IPersonsUpdaterService personsUpdaterService, IPersonsSorterService personsSorterService, ICountriesGetterService countriesService, ILogger<PersonsController> logger)
    {
        _personsGetterService = personsGetterService;
        _personsAdderService = personsAdderService;
        _personsUpdaterService = personsUpdaterService;
        _personsDeleterService = personsDeleterService;
        _personsSorterService = personsSorterService;

        _countriesService = countriesService;
        _logger = logger;
    }

    //Url: persons/index
    [Route("[action]")]
    [Route("/")]
    [ServiceFilter(typeof(PersonsListActionFilter), Order = 4)]
    //[TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] { "MyKey-FromAction", "MyValue-From-Action", 1 }, Order = 1)]

    [ResponseHeaderFilterFactory("MyKey-FromAction", "MyValue-From-Action", 1)]

    [SkipFilter]
    [TypeFilter(typeof(PersonsListResultFilter))]
    public async Task<IActionResult> Index(string searchBy, string? searchString, string sortBy = nameof(PersonResponse.PersonName), SortOrderOptions sortOrder = SortOrderOptions.ASC)
    {
        _logger.LogInformation("Index action method of PersonsController");

        _logger.LogDebug($"searchBy: {searchBy}, searchString: {searchString}, sortBy: {sortBy}, sortOrder: {sortOrder}");

        //Search
        List<PersonResponse> persons = await _personsGetterService.GetFilteredPersons(searchBy, searchString);

        // ne treba posto smo u action filteru pristupili viewData (LAKSE JE U CONTROLLER NEGO U FILTERIMA)
        //ViewBag.CurrentSearchBy = searchBy;
        //ViewBag.CurrentSearchString = searchString;

        //Sort
        List<PersonResponse> sortedPersons = await _personsSorterService.GetSortedPersons(persons, sortBy, sortOrder);
        // ne treba posto smo u action filteru pristupili viewData
        //ViewBag.CurrentSortBy = sortBy;
        //ViewBag.CurrentSortOrder = sortOrder.ToString();

        return View(sortedPersons); //Views/Persons/Index.cshtml
    }


    //Executes when the user clicks on "Create Person" hyperlink (while opening the create view)
    //Url: persons/create
    [Route("[action]")]
    [HttpGet]
    //[TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] { "My-Key-From-Method", "My-Value-From-Method", 4 })] //svaki put moze druga vrijednost kad invoke PersonsListActionFilter
    [ResponseHeaderFilterFactory("My-Key-From-Method", "My-Value-From-Method", 4)]
    public async Task<IActionResult> Create()
    {
        List<CountryResponse> countries = await _countriesService.GetAllCountries();
        ViewBag.Countries = countries.Select(temp =>
          new SelectListItem() { Text = temp.CountryName, Value = temp.CountryID.ToString() }
        );

        //new SelectListItem() { Text="Harsha", Value="1" }
        //<option value="1">Harsha</option>
        return View();
    }

    [HttpPost]
    //Url: persons/create
    [Route("[action]")]
    [TypeFilter(typeof(PersonCreateAndEditPostActionFilter))]
    [TypeFilter(typeof(FeatureDisabledResourceFilter), Arguments = new object[] { false})]
    public async Task<IActionResult> Create(PersonAddRequest personRequest)
    {

        //call the service method
        PersonResponse personResponse = await _personsAdderService.AddPerson(personRequest);

        //navigate to Index() action method (it makes another get request to "persons/index"
        return RedirectToAction("Index", "Persons");
    }

    //for loading edit view
    [HttpGet]
    [Route("[action]/{personID}")] //Eg: /persons/edit/1
    [TypeFilter(typeof(TokenResultFilter))]
    public async Task<IActionResult> Edit(Guid personID)
    {
      PersonResponse? personResponse = await _personsGetterService.GetPersonByPersonID(personID);
      if (personResponse == null)
      {
        return RedirectToAction("Index");
      }

      PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();

      List<CountryResponse> countries = await _countriesService.GetAllCountries();
      ViewBag.Countries = countries.Select(temp =>
      new SelectListItem() { Text = temp.CountryName, Value = temp.CountryID.ToString() });

      return View(personUpdateRequest);
    }

    //for handling submit button
    [HttpPost]
    [Route("[action]/{personID}")]
    [TypeFilter(typeof(PersonCreateAndEditPostActionFilter))]
    [TypeFilter(typeof(TokenAuthorizationFilter))]
    //[TypeFilter(typeof(PersonAlwaysRunResultFilter))]
    public async Task<IActionResult> Edit(PersonUpdateRequest personRequest)
    {
        PersonResponse? personResponse = await _personsGetterService.GetPersonByPersonID(personRequest.PersonID);

        if (personResponse == null)
        {
            return RedirectToAction("Index");
        }
        //personRequest.PersonID = Guid.NewGuid(); da testiramo error handling
        PersonResponse updatedPerson = await _personsUpdaterService.UpdatePerson(personRequest);
        return RedirectToAction("Index", "Persons");
        
    }

    [HttpGet]
    [Route("[action]/{personID}")]
    public async Task<IActionResult> Delete(Guid? personID)
    {
      PersonResponse? personResponse = await _personsGetterService.GetPersonByPersonID(personID);
      if (personResponse == null)
        return RedirectToAction("Index");

      return View(personResponse);
    }

    [HttpPost]
    [Route("[action]/{personID}")]
    public async Task<IActionResult> Delete(PersonUpdateRequest personUpdateResult)
    {
      PersonResponse? personResponse = await _personsGetterService.GetPersonByPersonID(personUpdateResult.PersonID);
      if (personResponse == null)
        return RedirectToAction("Index");

     await _personsDeleterService.DeletePerson(personUpdateResult.PersonID);
      return RedirectToAction("Index");
    }
    //ROTATIVA  package
    [Route("PersonsPDF")]
    public async Task<IActionResult> PersonsPDF()
    {
        //Get list of persons - mora se slagati sa modelom u view (IEnumerable)
        List<PersonResponse> persons = await _personsGetterService.GetAllPersons();
        //return view as pdf
        return new ViewAsPdf("PersonsPDF", persons, ViewData)
        {
            PageMargins = new Rotativa.AspNetCore.Options.Margins()
            {
                Right = 20, Top = 20, Bottom = 20, Left = 20
            },
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape
        };
    }

    [Route("PersonsCSV")]
    public async Task<IActionResult> PersonsCSV()
    {
        MemoryStream memoryStream =  await _personsGetterService.GetPersonsCSV();
        return File(memoryStream, "application/octet-stream", "persons.csv");
    }

    [Route("PersonsExcel")]
    public async Task<IActionResult> PersonsExcel()
    {
        MemoryStream memoryStream = await _personsGetterService.GetPersonsExcel();
        return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "persons.xlsx");
    }
}

