
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enttities;
/// <summary>
/// Person domain model class
/// </summary>
public class Person
{
    //primary key
    [Key]
    public Guid PersonID { get; set; }
    //nvarchar(40)
    [StringLength(40)]
    public string? PersonName { get; set; }
    [StringLength(40)]
    public string? Email { get; set; }
    public DateTime? DateOfBirth { get; set; }
    [StringLength(10)]
    public string? Gender { get; set; }
    //uniqueidentifier
    public Guid? CountryID { get; set; }
    [StringLength(200)]
    public string? Address { get; set; }
    //bit
    public bool ReceiveNewsLetters { get; set; }
    //nvarchar - svi moguci characteri
    public string? TIN { get; set; }

    //in detail model class we can access master class
    [ForeignKey("CountryID")]
    public virtual Country? Country { get; set; }

    public override string ToString()
    {
        return $"Person ID: {PersonID}," +
            $" Person Name: {PersonName}," +
            $" Email: {Email}," +
            $" Date of Birth: {DateOfBirth?.ToString("dd MMMM yyyy")}," +
            $" Gender: {Gender}," +
            $" Country ID: {CountryID}," +
            $" Country: {Country?.CountryName}," +
            $" Address: {Address}," +
            $" Receive News Letters: {ReceiveNewsLetters}";
    }

}
