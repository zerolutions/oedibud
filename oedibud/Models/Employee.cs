namespace oedibud.Models;

public class Employee
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Group { get; set; } = "";

    public int Level { get; set; }

    public DateTime HireDate { get; set; }
}
