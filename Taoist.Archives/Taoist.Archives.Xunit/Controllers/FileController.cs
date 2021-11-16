using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taoist.Archives.project;
using Taoist.Archives.Xunit;

namespace NUnit.Netnr.Fast.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("sys/")]
    public class FileController : Controller
    {

        [HttpGet,Route("get/root_")]//camera
        public IEnumerable<List<Directorys.DirectoryStructure.Files>> Get(bool depth = false)
        {
            yield return Directorys.DirectoryStructure.Get(Web.PhysicalPath, depth);
        }

    }
}
