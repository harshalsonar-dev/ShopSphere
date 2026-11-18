using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShopSphere.Application.Common.Models;
using ShopSphere.Application.Features.Products.DTOs;
using ShopSphere.Application.Features.Products.Queries;

namespace ShopSphere.Web.Controllers.Api;

[ApiController]
[Route("api/v1/categories")]
[Produces("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CategoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCategoriesQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<CategoryDto>>.Ok(result, "Categories retrieved successfully."));
    }
}
