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
        public void Configure(IApplicationBuilder app)
        {
            //app.UseStaticFiles();//使用默认文件夹wwwroot

            var dir = new DirectoryBrowserOptions();
            dir.FileProvider = new PhysicalFileProvider(@"D:\数据\模型数据");
            app.UseDirectoryBrowser(dir);

            //更改默认文件夹 (StaticFileOptions方法)
            var staticfile = new StaticFileOptions();
            staticfile.FileProvider = new PhysicalFileProvider(@"D:\数据\模型数据");//指定目录，这里指C盘，也可以是其他目录
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
        public static dbConfigure configure = new dbConfigure();
        public void stop() {
            Windows_WebServer.stop();
        }
        public void start()
        {
            //string index = Taoist.Archives.Resource.index;


            if (!string.IsNullOrEmpty(configure.path) && !string.IsNullOrEmpty(configure.ip))
                Windows_WebServer.start(System.Net.IPAddress.Parse(configure.ip), Painter.NetworkPort, 1, configure.path);
        }

        public Identity Configure(dbConfigure configure)
        {
            UseDirectoryBrowser.configure = configure; start();
            return new Identity()
            {
                url = $"http://{System.Net.IPAddress.Parse(configure.ip)}:{ Painter.NetworkPort}"
            };
        }
        
    }
    /// <summary>
    /// 
    /// </summary>
    public class dbConfigure
    {
        public  string ip { get; set; }
        public  string path { get; set; }
    }
    public class Identity {
        public string url { get; set; }
    }
}
