using Microsoft.AspNetCore.Mvc;

namespace HotelStay.Api.Controllers;

[ApiController]
public sealed class HealthController : ControllerBase
{
    [HttpGet("/health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<object> Get() => Ok(new { status = "Healthy" });
}
