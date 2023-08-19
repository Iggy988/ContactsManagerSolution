using System.ComponentModel.DataAnnotations;

namespace Enttities;

/// <summary>
/// Domain Model for storing Country details
/// </summary>
public class Country
{
    [Key]
    public Guid CountryID { get; set; }
    public string? CountryName { get; set; }

    //from parent model we can access detail collection
    public virtual ICollection<Person>? Persons { get; set;} //load all persons with coresponding country id
}