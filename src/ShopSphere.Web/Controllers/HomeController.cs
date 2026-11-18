using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShopSphere.Application.Features.Products.Queries;

namespace ShopSphere.Web.Controllers;

public class HomeController : Controller
{
    private readonly IMediator _mediator;

    public HomeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> Index(
        string? searchTerm,
        int? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        int pageIndex = 1,
        int pageSize = 6,
        CancellationToken cancellationToken = default)
    {
        var productsQuery = new GetProductsQuery(searchTerm, categoryId, minPrice, maxPrice, pageIndex, pageSize);
        var productsResult = await _mediator.Send(productsQuery, cancellationToken);

        var categories = await _mediator.Send(new GetCategoriesQuery(), cancellationToken);

        ViewBag.Categories = categories;
        ViewBag.SearchTerm = searchTerm;
        ViewBag.CategoryId = categoryId;
        ViewBag.MinPrice = minPrice;
        ViewBag.MaxPrice = maxPrice;

        return View(productsResult);
    }

    public IActionResult Error()
    {
        return View();
    }
}
