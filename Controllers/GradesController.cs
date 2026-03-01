using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication.Data;
using WebApplication.Models;

namespace WebApplication.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GradesController : ControllerBase
{
    private readonly SchoolContext _context;
    public GradesController(SchoolContext context) => _context = context;

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<Grade>>> GetAll()
    {
        return await _context.Grades.ToListAsync();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Grade>> Create(Grade grade)
    {
        _context.Grades.Add(grade);
        await _context.SaveChangesAsync();
        return Ok(grade);
    }
}
