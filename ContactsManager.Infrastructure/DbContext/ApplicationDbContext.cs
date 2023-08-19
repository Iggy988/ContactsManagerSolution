using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Enttities;

public class ApplicationDbContext : DbContext
{
    //sve sto je uneseno u options u Program.cs (DbContextOptionsBuilder) bice proslijedjeno ovdje u options preko base
    public ApplicationDbContext(DbContextOptions options):base(options)
    {
        
    }

    //db set per model class
    // ako ih oznacimo sa virtual omogucavamo mocking child classes to override db sets
    public virtual DbSet<Country> Countries { get; set; }
    public virtual DbSet<Person> Persons { get; set; }

    //overridujemo metodu za spajanje dbSeta sa model class
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // za spajanje dbseta za tablu(dajemo ime table)
        modelBuilder.Entity<Country>().ToTable("Countries");
        modelBuilder.Entity<Person>().ToTable("Persons");

        //Seed to Countries
        //modelBuilder.Entity<Country>().HasData(new Country { CountryID = Guid.NewGuid(), CountryName = "Sample" });

        string countriesJson = File.ReadAllText("countries.json");
        List<Country> countries = JsonSerializer.Deserialize<List<Country>>(countriesJson);

        foreach(Country country in countries)
        {
            modelBuilder.Entity<Country>().HasData(country);
        }

        //Seed to Persons
        //modelBuilder.Entity<Country>().HasData(new Country { CountryID = Guid.NewGuid(), CountryName = "Sample" });

        string personsJson = File.ReadAllText("persons.json");
        List<Person> persons = JsonSerializer.Deserialize<List<Person>>(personsJson);

        foreach (Person person in persons)
        {
            modelBuilder.Entity<Person>().HasData(person);
        }

        //Fluent API
        // biramo odredjeni property u koji zelimo da dodamo table type npr.
        modelBuilder.Entity<Person>().Property(temp=> temp.TIN)
            .HasColumnName("TaxIdentificationNumber")
            .HasColumnType("varchar(8)")
            .HasDefaultValue("ABC12345");

        //modelBuilder.Entity<Person>().HasIndex(temp => temp.TIN)
        //    .IsUnique();

        modelBuilder.Entity<Person>().HasCheckConstraint("CHK_TIN", "len([TaxIdentificationNumber]) = 8");


        //Table Relations
        //modelBuilder.Entity<Person>(entity =>
        //{
        //    entity.HasOne<Country>(c => c.Country).WithMany(p => p.Persons).HasForeignKey(p => p.CountryID);
        //});
    }

    


    public List<Person> sp_GetAllPersons()
    {
        return Persons.FromSqlRaw("EXECUTE [dbo].[GetAllPersons]").ToList();
    }

    //kad koristimo stored procedure
    public int sp_InsertPerson(Person person)
    {
        // to supply parasmeters to stored procedure
        SqlParameter[] parameters = new SqlParameter[] {
        new SqlParameter("@PersonID", person.PersonID),
        new SqlParameter("@PersonName", person.PersonName),
        new SqlParameter("@Email", person.Email),
        new SqlParameter("@DateOfBirth", person.DateOfBirth),
        new SqlParameter("@Gender", person.Gender),
        new SqlParameter("@CountryID", person.CountryID),
        new SqlParameter("@Address", person.Address),
        new SqlParameter("@ReceiveNewsLetters", person.ReceiveNewsLetters)
      };
        return Database.ExecuteSqlRaw("EXECUTE [dbo].[InsertPerson] @PersonID, @PersonName, @Email, @DateOfBirth, @Gender, @CountryID, @Address, @ReceiveNewsLetters", parameters);
    }
}
