using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class VacanciesController : ControllerBase
{
    private readonly AppDbContext _context;
    // 124
    public VacanciesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetVacancies()
    {
        var vacancies = await _context.Vacancies
            .Include(v => v.Responses)
            .ToListAsync();
        return Ok(vacancies);
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<IActionResult> CreateVacancy([FromBody] Vacancy vacancy)
    {
        _context.Vacancies.Add(vacancy);
        await _context.SaveChangesAsync();
        return Ok(vacancy);
    }

    [Authorize(Roles = "users")]
    [HttpPost("{id}/apply")]
    public async Task<IActionResult> ApplyToVacancy(int id, [FromBody] Response response)
    {
        var vacancy = await _context.Vacancies.FindAsync(id);
        if (vacancy == null) return NotFound("Vacancy not found");

        response.VacancyId = vacancy.Id;
        _context.Responses.Add(response);
        await _context.SaveChangesAsync();
        return Ok(response);
    }
}
