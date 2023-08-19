using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters;

//This class can be applyed as attribute to controller or action method
//This class can play as filter
public class SkipFilter: Attribute, IFilterMetadata
{

}
