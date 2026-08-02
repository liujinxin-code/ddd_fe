using Application.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Open.Controllers
{
    [Route("api/[controller]/")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        public CurrentUser CurrentUser = default!;


    }
}
