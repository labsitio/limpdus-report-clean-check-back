using Microsoft.AspNetCore.Mvc;

namespace LimpidusMongoDB.Api.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    public abstract class BaseV1Controller : ControllerBase
    {
    }
}
