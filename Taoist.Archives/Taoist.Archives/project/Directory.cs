using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Taoist.Archives.project
{
    class Directorys
    {

        private static dbConfigure _config = UseDirectoryBrowser.configure;
        class DirectoryStructure {
            public static List<FileNames> _json { get; set; } = Get();
            public static List<FileNames> Structure()
            {
                try
                {
                    _json = Get();
                    return _json;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return null;
                }

            }

            public class FileNames
            {
                public int id { get; set; }
                public string uri { get; set; }

                public string text { get; set; }
                public state state { get; set; }
                public List<FileNames> children { get; set; }
                public string icon { get; set; }
            }
            public class state
            {
                public bool opened { get; set; }
            }
            //以上字段为树形控件中需要的属性
            //获得指定路径下所有文件名
            public static List<FileNames> getFileName(List<FileNames> list, string filepath)
            {
                var uri = "http://" + _config.ip + ":" + Painter.NetworkPort + "/";
                DirectoryInfo root = new DirectoryInfo(filepath);
                foreach (FileInfo f in root.GetFiles())
                {
                    var str = Path.GetFileName(f.FullName).ToLower();
                    if ("tileset.json" == str)
                    {
                        list.Add(new FileNames
                        {
                            uri = f.FullName.Replace(_config.path, uri).Replace("\\", "/"),
                            text = f.Name,
                            state = new state { opened = false },
                            icon = "jstree-file"
                        });
                    }
                }
                return list;
            }
            //获得指定路径下的所有子目录名
            // <param name="list">文件列表</param>
            // <param name="path">文件夹路径</param>
            public static List<FileNames> GetallDirectory(List<FileNames> list, string path)
            {
                DirectoryInfo root = new DirectoryInfo(path);
                var dirs = root.GetDirectories();
                if (dirs.Count() != 0)
                {
                    foreach (DirectoryInfo d in dirs)
                    {
                        list.Add(new FileNames
                        {
                            text = d.Name,
                            state = new state { opened = false },
                            children = GetallDirectory(new List<FileNames>(), d.FullName)
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
            public static List<FileNames> Get()
            {
                var _path = _config.path;
                if (String.IsNullOrEmpty(_path))
                {
                    return null;
                }

                Dictionary<String, String[]> fileName = new Dictionary<String, String[]>();
                Dictionary<String, String[]> pngfileName = new Dictionary<String, String[]>();
                string path = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(_path));// @"D:\Work\publish\MapData";


                List<FileNames> GetAllPath()
                {
                    //获取当前系统的根路径           
                    string rootpath = path;
                    var list = GetallDirectory(new List<FileNames>(), rootpath).ToArray();
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
                var uri = "http://" + _config.ip + ":" + Painter.NetworkPort + "/";
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
                            a.Add(fileList[i].Replace(_config.path, uri).Replace("\\", "/"));
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
    }
}
