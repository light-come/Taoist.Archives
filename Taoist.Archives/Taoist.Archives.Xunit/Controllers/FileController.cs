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
 
        public static string GIS_json { get; set; } = "{}";

        [HttpGet, Route("get/gis_")]//camera
        public object GetStructure()
        {
            try
            {
                String Uri = "http://" + Painter.NetworkIP + ":" + Painter.NetworkPort + "/杭州-七堡排涝站/模型文件 20211210/qibao/";
                GIS_json = Windows_Gis.Getjson(@"C:\模型数据\杭州-七堡排涝站\模型文件 20211210\qibao", Uri);//C:\模型数据\杭州-七堡排涝站\模型文件 20211210\qibao
                return (new
                {
                    list = JsonConvert.DeserializeObject<JObject>(GIS_json)
                });
            }
            catch (Exception ex)
            {
                return (new
                {
                    messerr = ex.Message
                });
            }

        }

    }
}
