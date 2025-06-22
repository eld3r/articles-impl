using Articles.Services;
using Articles.Services.DTO;
using Microsoft.AspNetCore.Mvc;

namespace Articles.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SectionsController(ISectionsService sectionService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<SectionDto>>> GetAll()
    {
        var sections = await sectionService.GetAllSections();
        return Ok(sections);
    }
    
    [HttpGet("{id:long}")]
    public async Task<ActionResult<SectionDetailedDto>> GetById(long id)
    {
        var section = await sectionService.GetDetailedSection(id);
        if (section == null)
            return NotFound();

        return Ok(section);
    }
}