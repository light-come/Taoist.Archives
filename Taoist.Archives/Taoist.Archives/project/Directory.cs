using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Taoist.Archives.shared;
/// <summary>
/// 资源处理
/// </summary>
namespace Taoist.Archives.project
{
    /// <summary>
    /// 内置服务器文件解析类 (未与AspNetCore.StaticFiles进行交叉联动
    /// </summary>
    public static class Directorys
    {
        public static string SHA1(string s)
        {
            try
            {
                FileStream file = new FileStream(s, FileMode.Open);
                SHA1 sha1 = new SHA1CryptoServiceProvider();
                byte[] retval = sha1.ComputeHash(file);
                file.Close();

                StringBuilder sc = new StringBuilder();
                for (int i = 0; i < retval.Length; i++)
                {
                    sc.Append(retval[i].ToString("x2"));
                }
                return  (sc).ToString();
            }
            catch (Exception ex)
            {
                return (ex.Message);
            }
        }
        public static string MD5(string s)
        {
            try
            {
                FileStream file = new FileStream(s, FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retval = md5.ComputeHash(file);
                file.Close();

                StringBuilder sc = new StringBuilder();
                for (int i = 0; i < retval.Length; i++)
                {
                    sc.Append(retval[i].ToString("x2"));
                }
                return  (sc).ToString();
            }
            catch (Exception ex)
            {
                return  (ex.Message);
            }
        }


        /// <summary>
        /// 文件读取类
        /// </summary>
        public static class DirectoryStructure {
            /// <summary>
            /// 获取根目录结构 未启动内置服务器时uri资源链接不起作用
            /// </summary>
            public static List<Files> root_ { get; set; } = Get(Web.PhysicalPath, false);
           
            public class Files
            {
                public string id { get; set; }
                public string uri { get; set; }
                public string text { get; set; }
                public state state { get; set; }
                public List<Files> children { get; set; } = new List<Files>() { };
                public long szie { get; set; }
                public string time { get; set; }
                public string type { get; set; }
            }
            public class state
            {
                public bool opened { get; set; }
            }
            //以上字段为树形控件中需要的属性
            //获得指定路径下所有文件名
            public static List<Files> getFileName(List<Files> list, string filepath)
            {
                var uri = "http://" + Painter.NetworkIP + ":" + Painter.NetworkPort;
                DirectoryInfo root = new DirectoryInfo(filepath);
                foreach (FileInfo f in root.GetFiles())
                {
                  
                    //var str = Path.GetFileName(f.FullName).ToLower();
                    //if ("tileset.json" == str)//约束文件格式
                    {
                        list.Add(new Files
                        {
                            id = SHA1(f.FullName),
                            uri = f.FullName.Replace(Web.PhysicalPath, uri).Replace("\\", "/"),
                            text = f.Name,
                            time = f.LastWriteTimeUtc.ToString("yyyy/MM/dd HH:mm:ss zz"),
                            szie = f.Length,
                            state = new state { opened = false },
                            type = "jstree-file"
                        });
                    }
                }
                return list;
            }

            /// <summary>
            /// 获得指定路径下的所有子目录名
            /// </summary>
            /// <param name="list">文件列表</param>
            /// <param name="path">文件夹路径</param>
            /// <param name="all">是否深度索引</param>
            /// <returns></returns>
            public static List<Files> GetallDirectory(List<Files> list, string path,bool all = false)
            {
                DirectoryInfo root = new DirectoryInfo(path);
                var dirs = root.GetDirectories();
                if (dirs.Count() != 0)
                {
                    foreach (DirectoryInfo d in dirs)
                    {
                        list.Add(new Files
                        {
                            text = d.Name,
                            time = d.LastWriteTimeUtc.ToString("yyyy/MM/dd HH:mm:ss zz"),
                            state = new state { opened = false },
                            type = "files",
                            children = !all ? new List<Files>() : GetallDirectory(new List<Files>(), d.FullName, all)
                        });
                    }
                }
                list = getFileName(list, path);
                return list;
            }
            /// <summary>
            /// 获取到模型路径及值
            /// </summary>
            /// <returns></returns>
            // GET api/values
            public static List<Files> Get(string _path, bool all = false)
            {
                if (String.IsNullOrEmpty(_path))
                {
                    return null;
                }
                string path = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(_path));

                List<Files> GetAllPath()
                {
                    //获取当前系统的根路径           
                    string rootpath = path;
                    var list = GetallDirectory(new List<Files>(), rootpath,all).ToArray();
                    return list.ToList();
                }
                var data = GetAllPath();

                return data;
            }

            #region 查找目录下包含子目录的全部文件


            public static List<string> fileList = new List<string>();
            /// <summary>
            /// 获得目录下所有文件或指定文件类型文件地址  isFullName是确定文件名称还是路径
            /// </summary>
            public static string[] GetFile(string fullPath, bool isFullName = true)
            {
                var uri = "http://" + Painter.NetworkIP + ":" + Painter.NetworkPort;
                try
                {
                    fileList.Clear();

                    DirectoryInfo dirs = new DirectoryInfo(fullPath); //获得程序所在路径的目录对象
                    DirectoryInfo[] dir = dirs.GetDirectories();//获得目录下文件夹对象
                    FileInfo[] file = dirs.GetFiles();//获得目录下文件对象
                    int dircount = dir.Count();//获得文件夹对象数量
                    int filecount = file.Count();//获得文件对象数量

                    //循环文件夹
                    for (int i = 0; i < dircount; i++)
                    {
                        string pathNode = fullPath + "\\" + dir[i].Name;
                        GetMultiFile(pathNode, isFullName);
                    }

                    //循环文件
                    for (int j = 0; j < filecount; j++)
                    {
                        if (isFullName)
                        {
                            fileList.Add(file[j].FullName);
                        }
                        else
                        {
                            fileList.Add(file[j].Name);
                        }
                    }
                    List<String> a = new List<String>();
                    //筛选字符过滤非3dtiles或其他
                    for (int i = 0; i < fileList.Count; i++)
                    {
                        bool bl = false;
                        if (isFullName)
                        {
                            bl = fileList[i].Split('\\')[fileList[i].Split('\\').Length - 1].Split('.')[0] != "tileset" ? false : true;
                        }
                        else
                        {
                            bl = fileList[i].Split('.')[0] != "tileset" ? false : true;
                        }

                        if (bl)
                        {
                            a.Add(fileList[i].Replace(Web.PhysicalPath, uri).Replace("\\", "/"));
                        }

                    }
                    return a.ToArray();
                }
                catch (Exception ex)
                {
                    // ex.Message + "\r\n出错的位置为：Form1.PaintTreeView()";
                }

                return null;
            }

            private static bool GetMultiFile(string path, bool isFullName = true)
            {
                if (Directory.Exists(path) == false)
                { return false; }

                DirectoryInfo dirs = new DirectoryInfo(path); //获得程序所在路径的目录对象
                DirectoryInfo[] dir = dirs.GetDirectories();//获得目录下文件夹对象
                FileInfo[] file = dirs.GetFiles();//获得目录下文件对象
                int dircount = dir.Count();//获得文件夹对象数量
                int filecount = file.Count();//获得文件对象数量
                int sumcount = dircount + filecount;

                if (sumcount == 0)
                { return false; }

                //循环文件夹
                for (int j = 0; j < dircount; j++)
                {
                    string pathNodeB = path + "\\" + dir[j].Name;
                    GetMultiFile(pathNodeB, isFullName);
                }

                //循环文件
                for (int j = 0; j < filecount; j++)
                {
                    if (isFullName)
                    {
                        fileList.Add(file[j].FullName);
                    }
                    else
                    {
                        fileList.Add(file[j].Name);
                    }
                }


                return true;
            }

            #endregion
        }

        public static class DirectoryFileStore {
            public static async Task<IO.IActionResult.Ok> FileStore(IFormCollection ifc) {

                string webRootPath = Web.PhysicalPath;
                string fileFolder = Path.Combine(webRootPath, ".WebCache");

                if (!Directory.Exists(webRootPath))
                    Directory.CreateDirectory(webRootPath);

                IO.IActionResult.Ok iio = new IO.IActionResult.Ok();
                 var data = new IO.IActionResult.data
                 {
                     count = ifc.Files.Count,
                     size = 0
                 };

                try
                {
                    if (ifc.Files.Count >= 1)
                    {
                        foreach (var item in ifc.Files)
                        {
                            if (item.Length > 0)
                            {
                                data.size = item.Length;

                                var fileName = DateTime.Now.ToString("yyyyMMddHHmmss") +
                                               Path.GetExtension(item.FileName);
                                var filePath = Path.Combine(fileFolder, fileName);

                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                   
                                    await item.CopyToAsync(stream);
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    iio.code = "1";
                    iio.msg = ex.Message;
                }
                iio.data = data;
                return iio;
            }

            public static async Task<IO.IActionResult.Ok> FileStore(List<IFormFile> ff)
            {
                string RootPath = Web.PhysicalPath;
                long size = ff.Sum(f => f.Length);
                var fileFolder = Path.Combine(RootPath, ".WebCache");

                if (!Directory.Exists(fileFolder))
                    Directory.CreateDirectory(fileFolder);

                foreach (var formFile in ff)
                {
                    if (formFile.Length > 0)
                    {
                        var fileName = DateTime.Now.ToString("yyyyMMddHHmmss") +
                                       Path.GetExtension(formFile.FileName);
                        var filePath = Path.Combine(fileFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await formFile.CopyToAsync(stream);
                        }
                    }
                }
                //return Ok(new { count = files.Count, size });

                IO.IActionResult.Ok iio = new IO.IActionResult.Ok()
                {
                    data = new IO.IActionResult.data
                    {
                        count = ff.Count,
                        size = size
                    }
                };
                return iio;
            }

            public static async Task<IO.IActionResult.Ok> FileStore()
            {
                string RootPath = Web.PhysicalPath;
                var fileFolder = Path.Combine(RootPath, ".WebCache");

                if (!Directory.Exists(fileFolder))
                    Directory.CreateDirectory(fileFolder);

                var data = Directorys.DirectoryStructure.Get(fileFolder, true);
                IO.IActionResult.Ok iio = new IO.IActionResult.Ok()
                {
                    data = new {
                        list = data,
                        count = data.Count
                    },
                };
                return iio;
            }
            public static async Task<IO.IActionResult.Ok> FileStore(string id)
            {
                string RootPath = Web.PhysicalPath;
                var fileFolder = Path.Combine(RootPath, ".WebCache");

                if (!Directory.Exists(fileFolder))
                    Directory.CreateDirectory(fileFolder);
                IO.IActionResult.Ok iio = new IO.IActionResult.Ok();
                DirectoryInfo root = new DirectoryInfo(fileFolder);
                int i = 1;

                iio.data = new
                {
                    count = 0
                };

                foreach (FileInfo f in root.GetFiles())
                {
                    if (SHA1(f.FullName) == id)
                    {
                        try
                        {
                            File.Delete(f.FullName);//删除指定文件
                            iio.data = new
                            {
                                count = i++,
                                id
                            };
                        }
                        catch (Exception ex)
                        {
                            iio.code = "1";
                            iio.msg = ex.Message;
                            iio.data = new {
                                count = 0,
                                id
                            };
                            return iio;
                        }
                    }
                }
                return iio;
            }


        }
    }
}
