using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Taoist.Archives.project
{
    public class StaticFileConfigure
    {
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,string path)
        {
            //app.UseStaticFiles();//使用默认文件夹wwwroot

            var dir = new DirectoryBrowserOptions();
            dir.FileProvider = new PhysicalFileProvider(path);
            app.UseDirectoryBrowser(dir);

            //更改默认文件夹 (StaticFileOptions方法)
            var staticfile = new StaticFileOptions();
            staticfile.FileProvider = new PhysicalFileProvider(path);//指定目录，这里指C盘，也可以是其他目录
            app.UseStaticFiles(staticfile);//使用默认文件夹wwwroot
                                           //手动设置MIME Type,或者设置一个默认值， 以解决某些文件MIME Type文件识别不到，出现404错误
            staticfile.ServeUnknownFileTypes = true;
            staticfile.DefaultContentType = "application/x-msdownload";//设置默认MIME Type
            var provider = new FileExtensionContentTypeProvider();//使用一组默认映射创建新的提供程序
            provider.Mappings.Add(".log", "text/plain");//手动设置对应MIME Type
            staticfile.ContentTypeProvider = provider; //将文件映射到内容类型
            //app.Run(async (context) =>
            //{
            //    await ("Hello World!");
            //});
            //return staticfile;
        }
    }
    public class UseDirectoryBrowser
    {
        public static string PhysicalPath { get; set; } = "c://";
        public void stop() {
            Windows_WebServer.stop();
        }
        public bool start()
        {
            stop();
            if (Directory.Exists(PhysicalPath))//如果不存在就创建file文件夹
            {
                Windows_WebServer.start(System.Net.IPAddress.Parse(Painter.NetworkIP), Painter.NetworkPort, 1, PhysicalPath);
                return true;
            }
            return false;
        }

        public entity Configure(string path)
        {
            PhysicalPath = path; start();
            return new entity()
            {
                url = $"http://{System.Net.IPAddress.Parse(Painter.NetworkIP)}:{ Painter.NetworkPort}"
            };
        }
        
    }

    public class entity {
        public string url { get; set; }
    }
}
