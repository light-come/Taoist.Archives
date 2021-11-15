using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Taoist.Archives.shared;

namespace Taoist.Archives
{
    [ApiController]
    [Produces("application/json")]
    [Route("[controller]/[action]")]
    public class FileOl : Controller
    {
        // POST api/values
        [HttpPost]
        public void Archives_Body([FromBody] string value)
        {

        }

        [HttpPost]
        public async Task<IActionResult> Archives_IForm(IFormCollection files)
        {
            await new Task(() =>
            {
                Console.WriteLine("1");
            });
            if (null == null)
            {
                return NotFound(); //Or, return Request.CreateResponse(HttpStatusCode.NotFound)
            }
            return Ok(new Ol());
        }
        [HttpPost]
        public async Task<IActionResult> Archives_IFormFile(IFormFile file)
        {
            await new Task(() =>
            {

            });
            if (null == null)
            {
                return NotFound(); //Or, return Request.CreateResponse(HttpStatusCode.NotFound)
            }
            return Ok(new Ol());
        }
        [HttpPost]
        public async Task<IActionResult> Archives_BodyIForm([FromBody] string type, IFormCollection files)
        {
            await new Task(() =>
            {

            });
            if (null == null)
            {
                return NotFound(); //Or, return Request.CreateResponse(HttpStatusCode.NotFound)
            }
            return Ok(new Ol());
        }
        [HttpPost]
        public async Task<IActionResult> Archives_TxtIForm(string type, IFormCollection files)
        {
            await new Task(() =>
            {

            });
            if (null == null)
            {
                return NotFound(); //Or, return Request.CreateResponse(HttpStatusCode.NotFound)
            }
            return Ok(new Ol());
        }
    }
}
