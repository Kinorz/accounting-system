using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;

namespace AccountingSystem.Api.Controllers;

[ApiController]
[Route("common")]
public class CommonController : ControllerBase
{
    public record HogeResponse(string Name, int Age);
    public record FugaRequest(int Id);
    public record FugaResponse(string Name, int Age);

    [HttpGet("hoge")]
    public IActionResult GetHoge()
    {
        var response = new HogeResponse("Taro", 20);
        return Ok(response);
    }

    [HttpPost("fuga")]
    public IActionResult GetFuga(FugaRequest request)
    {
        if (request.Id <= 0)
        {
            return BadRequest("Id must be positive.");
        }
        if (request.Id == 13)
        {
            return StatusCode(500, "Unlucky Id caused server error.");
        }
        if (request.Id == 7)
        {
            return StatusCode(403, "Access to Id 7 is forbidden.");
        }
        if (request.Id == 42)
        {
            return StatusCode(418, "I'm a teapot.");
        }
        if (request.Id == 99)
        {
            return StatusCode(429, "Too many requests for Id 99.");
        }
        if (request.Id == 100)
        {
            return StatusCode(301, "Id 100 has been moved permanently.");
        }
        if (request.Id == 101)
        {
            return StatusCode(302, "Id 101 has been found at a different location.");
        }
        if (request.Id == 102)
        {
            return StatusCode(307, "Id 102 has been temporarily redirected.");
        }
        if (request.Id == 103)
        {
            return StatusCode(308, "Id 103 has been permanently redirected.");
        }
        if (request.Id == 150)
        {
            return StatusCode(204);
        }
        if (request.Id % 2 == 0)
        {
            return NotFound();
        }
        var response = new FugaResponse("Hana", 30);
        return Ok(response);
    }

    public static int Add(int a, int b)
    {
        return a + b;
    }

    public static int AddChecked(int a, int b)
    {
        return checked(a + b);
    }
}
