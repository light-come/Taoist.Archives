using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Taoist.Archives.project;

namespace Taoist.Archives.Xunit
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            //注册Swagger生成器，定义一个和多个Swagger 文档
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v3", new OpenApiInfo { Title = "Taoist.Archives.Xunit", Version = "v3" });
                //Determine base path for the application.  
                var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
                //Set the comments path for the swagger json and ui.  
                //var xmlPath = Path.Combine(basePath, "Temporary directory analysis.xml");
                //c.IncludeXmlComments(xmlPath);
            });


            {
                #region 跨域
                //services.AddCors(options =>
                //{
                //    options.AddPolicy(AllowSpecificOrigin,
                //        builder =>
                //        {
                //            builder.AllowAnyMethod()
                //                .AllowAnyOrigin()
                //                .AllowAnyHeader();
                //        });
                //});
                //10.6.201.4:8082

                //services
                //.AddCors(builder =>
                //{
                //    builder.AddDefaultPolicy(configure =>
                //    {
                //        configure.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                //    });
                //});


                services.AddCors(options =>
                {
                    options.AddPolicy(AllowSpecificOrigin,

                        builder => builder.AllowAnyOrigin()

                        .WithMethods("GET", "POST", "HEAD", "PUT", "DELETE", "OPTIONS")

                        );

                });

                #endregion
                //配置返回Json
                services.AddControllersWithViews().AddNewtonsoftJson();
            }


        }
        private readonly string AllowSpecificOrigin = "AllowSpecificOrigin";
        //Web.StaticFileConfigure staticFileConfigure = new Web.StaticFileConfigure();
        Web.UseDirectoryBrowser useDirectoryBrowser = new Web.UseDirectoryBrowser();
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            //staticFileConfigure.Configure(app, @"D:\数据\模型数据");
            var c =   useDirectoryBrowser.Configure(@"C:\ZP\资源代理");
#if DEBUG
            System.Diagnostics.Process.Start("explorer", c.url);
            //System.Diagnostics.Process.Start("explorer", " http://localhost:5000/");
#endif

            //启用中间件服务生成Swagger作为JSON终结点
            app.UseSwagger();
            //启用中间件服务对swagger-ui，指定Swagger JSON终结点
            app.UseSwaggerUI(c =>
            {//ISC_API/
                c.SwaggerEndpoint("/swagger/v3/swagger.json", "Taoist.Archives.Xunit");
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            //设置远程
            app.UseRouting();
            //CORS 中间件必须配置为在对 UseRouting 和 UseEndpoints的调用之间执行。 配置不正确将导致中间件停止正常运行。
            app.UseCors(AllowSpecificOrigin);
            //app.UseAuthorization();//使用授权服务
            //app.UseAuthentication();//使用认证服务
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
