using System.Text.Json.Serialization;

public class Response
{
    public int Id { get; set; }
    public string Phone { get; set; }
    public string Telegram { get; set; }

    public int VacancyId { get; set; }

    [JsonIgnore]
    public Vacancy? Vacancy { get; set; }
}