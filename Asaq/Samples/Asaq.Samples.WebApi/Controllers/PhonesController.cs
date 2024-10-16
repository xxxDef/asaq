using Asaq.Core;
using Asaq.Samples.WebApi.Model;
using Microsoft.AspNetCore.Mvc;

namespace Asaq.Samples.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class PhonesController : ControllerBase
{
    private readonly ILogger<PhonesController> logger;
    private readonly FakeDatabase db = new();

    public PhonesController(ILogger<PhonesController> logger)
    {
        this.logger = logger;
    }

    public record PhonesWhere(string[]? Name, int? Year, Developer? Company, double? Weight);
    public record PhonesOrder(int? Name, int? Year, int? Company, int? Weight);

    [HttpGet("Filter")]
    public IEnumerable<Phone> Get([FromQuery] WhereQuery<PhonesWhere> where, [FromQuery] OrderQuery<PhonesOrder> order)
    {
        var phones = db.Phones
            .ApplyWhere(where)
            .ApplyOrder(order);

        return phones;
    }
    
    [HttpGet("Test")]
    public WhereQuery<PhonesWhere> Test([FromQuery] WhereQuery<PhonesWhere> where, [FromQuery] OrderQuery<PhonesOrder> order)
    {
        return where;
    }

}
