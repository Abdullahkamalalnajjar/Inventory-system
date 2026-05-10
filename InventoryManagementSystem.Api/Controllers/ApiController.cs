using Microsoft.AspNetCore.Mvc;
using InventoryManagementSystem.Api.Extensions;
using InventoryManagementSystem.Domain.Common.Results;

namespace InventoryManagementSystem.Api.Controllers;

[ApiController]
public abstract class ApiController : ControllerBase
{
    protected ActionResult Problem(List<Error> errors) => this.ToActionResult(errors);
}
