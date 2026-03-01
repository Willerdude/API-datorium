using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication.Data;
using WebApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace WebApplication.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubjectsController : ControllerBase
{
    private readonly SchoolContext _context;
    public SubjectsController(SchoolContext context) => _context = context;

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<Subject>>> GetAll()
    {
        return await _context.Subjects.ToListAsync();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Subject>> Create(Subject subject)
    {
        _context.Subjects.Add(subject);
        await _context.SaveChangesAsync();
        return Ok(subject);
    }
}
