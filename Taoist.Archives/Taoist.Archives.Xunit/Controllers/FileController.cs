using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace NUnit.Netnr.Fast.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("[controller]/[action]")]
    public class FileController : Controller
    {
        [HttpGet]
        [Route("get")]//camera
        public string Get() {
            return "hello";
        }
    }
}
