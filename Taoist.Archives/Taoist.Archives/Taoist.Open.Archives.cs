using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Taoist.Archives.shared;

namespace Taoist.Archives
{
    [ApiController]
    [Produces("application/json")]
    [Route("TOA/[action]")]
    public class Taoist_Open_Archives : Controller
    {

        [HttpGet]
        public async Task<IActionResult> _Get()
        {
            Task<IO.IActionResult.Ok> task = project.Directorys.DirectoryFileStore.FileStore();
            if (task.Result.code == "0")
                return Ok(task.Result);
            else
                return NotFound();
        }
        [HttpDelete]
        public async Task<IActionResult> _Delete([FromBody] Ol.Archives.Delete data)
        {
            Task<IO.IActionResult.Ok> task = project.Directorys.DirectoryFileStore.FileStore(data.id);
            if (task.Result.code == "0")
                return Ok(task.Result);
            else
                return NotFound();
        }
        [HttpPost]
        public async Task<IActionResult> _IForm(IFormCollection fc)
        {
            Task<IO.IActionResult.Ok> task = project.Directorys.DirectoryFileStore.FileStore(fc);
            if (task.Result.code == "0")
                return Ok(task.Result);
            else
                return NotFound();
        }
    

        [HttpPost]
        public async Task<IActionResult> _IFormFile(List<IFormFile> ff)
        {
            //await project.Directorys.DirectoryFileStore.FileStore(ff);

            Task<IO.IActionResult.Ok> task = project.Directorys.DirectoryFileStore.FileStore(ff);
            if(task.Result.code == "0")
                return Ok(task.Result);
            else
                return NotFound();
            //Task<int> t = project.Directorys.DirectoryFileStore.FileStore(ff);
            //_ = Task.Run(async () =>
            //{
            //    var bl = await t;
            //});
            //return Ok(new Ol());
        }
        //[HttpPost]
        //public async Task<IActionResult> _BodyIForm([FromBody] string type, IFormCollection files)
        //{
        //}
        //[HttpPost]
        //public async Task<IActionResult> _TxtIForm(string type, IFormCollection files)
        //{
        //}
    }


}
