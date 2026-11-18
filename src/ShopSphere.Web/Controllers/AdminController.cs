using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopSphere.Application.Features.Orders.Commands;
using ShopSphere.Application.Features.Orders.Queries;
using ShopSphere.Application.Features.Products.Commands;
using ShopSphere.Application.Features.Products.Queries;
using ShopSphere.Domain.Enums;

namespace ShopSphere.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var productsResult = await _mediator.Send(new GetProductsQuery(PageSize: 100), cancellationToken);
        var orders = await _mediator.Send(new GetAllOrdersQuery(), cancellationToken);

        ViewBag.TotalOrders = orders.Count;
        ViewBag.TotalRevenue = orders.Where(o => o.OrderStatus != OrderStatus.Cancelled).Sum(o => o.TotalAmount);
        ViewBag.LowStockCount = productsResult.Items.Count(p => p.StockQuantity < 10);

        return View(productsResult.Items);
    }

    [HttpGet]
    public async Task<IActionResult> CreateProduct(CancellationToken cancellationToken)
    {
        var categories = await _mediator.Send(new GetCategoriesQuery(), cancellationToken);
        ViewBag.Categories = categories;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProduct(CreateProductCommand command, CancellationToken cancellationToken)
    {
        try
        {
            await _mediator.Send(command, cancellationToken);
            TempData["SuccessMessage"] = $"Product '{command.Name}' created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            var categories = await _mediator.Send(new GetCategoriesQuery(), cancellationToken);
            ViewBag.Categories = categories;
            return View(command);
        }
    }

    [HttpGet]
    public async Task<IActionResult> EditProduct(int id, CancellationToken cancellationToken)
    {
        var product = await _mediator.Send(new GetProductByIdQuery(id), cancellationToken);
        if (product == null)
        {
            TempData["ErrorMessage"] = "Product not found.";
            return RedirectToAction(nameof(Index));
        }

        var categories = await _mediator.Send(new GetCategoriesQuery(), cancellationToken);
        ViewBag.Categories = categories;

        var command = new UpdateProductCommand(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.StockQuantity,
            product.CategoryId);

        return View(command);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProduct(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        try
        {
            await _mediator.Send(command, cancellationToken);
            TempData["SuccessMessage"] = $"Product '{command.Name}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            var categories = await _mediator.Send(new GetCategoriesQuery(), cancellationToken);
            ViewBag.Categories = categories;
            return View(command);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _mediator.Send(new DeleteProductCommand(id), cancellationToken);
            TempData["SuccessMessage"] = "Product deleted successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Orders(CancellationToken cancellationToken)
    {
        var orders = await _mediator.Send(new GetAllOrdersQuery(), cancellationToken);
        return View(orders);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus status, CancellationToken cancellationToken)
    {
        try
        {
            await _mediator.Send(new UpdateOrderStatusCommand(orderId, status), cancellationToken);
            TempData["SuccessMessage"] = $"Order #{orderId} status updated to '{status}'.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Orders));
    }
}
