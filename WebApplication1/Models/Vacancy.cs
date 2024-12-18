// Models/Vacancy.cs
using System.Collections.Generic;

public class Vacancy
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Date { get; set; }
    public string Salary { get; set; }
    public string Description { get; set; }

    public List<Response> Responses { get; set; } = new List<Response>();
}
