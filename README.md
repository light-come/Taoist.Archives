# Taoist.Archives
net core 3.1 api的附带插销，用于管理上传文件
## 📗 帮助
Wiki:

* 文档（代码和功能）
* 文件资源代理服务器
* 文件增删改管理

> （Demo）
>> Startup.cs
>> ```
>> <!--下面是Configure 配置 Startup.cs-->
>>>>        Web.UseDirectoryBrowser useDirectoryBrowser = new Web.UseDirectoryBrowser();
>>        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
>>        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
>>        {
>>
>>                    staticFileConfigure.Configure(app, @"D:\数据\模型数据");
>>                    var c =   useDirectoryBrowser.Configure(@"D:\数据\模型数据");
>>                    #if DEBUG
>>                      System.Diagnostics.Process.Start("explorer", c.url);
>>                      System.Diagnostics.Process.Start("explorer", " http://localhost:5000/");
>>                    #endif
>>                    ...
>>        }
>> 
>> ``
