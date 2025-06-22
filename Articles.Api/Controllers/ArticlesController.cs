using Articles.Services;
using Articles.Services.DTO;
using Microsoft.AspNetCore.Mvc;

namespace Articles.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArticlesController(IArticlesService articleService) : ControllerBase
{
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ArticleDto>> Get(long id)
    {
        var article = await articleService.GetById(id);
        if (article == null)
            return NotFound();

        return Ok(article);
    }

    [HttpPost]
    public async Task<ActionResult<ArticleDto>> Create([FromBody] CreateArticleRequest request)
    {
        var article = await articleService.Create(request);
        return CreatedAtAction(nameof(Get), new { id = article.Id }, article);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateArticleRequest request)
    {
        var success = await articleService.Update(request);
        if (!success)
            return NotFound();

        return NoContent();
    }
}
