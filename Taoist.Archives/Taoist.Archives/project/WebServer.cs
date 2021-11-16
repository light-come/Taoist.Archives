using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Taoist.Archives.project
{
    public static class Painter {
        /// <summary>
        /// 端口号
        /// </summary>
        public static int NetworkPort { get; set; } = PortIsUsed();
        public static string NetworkIP { get; set; } = "127.0.0.1";
        /// <summary>        
        /// 获取操作系统已用的端口号        
        /// </summary>        
        /// <returns></returns>        
        public static int PortIsUsed()
        {
            var listener = new TcpListener(IPAddress.Any, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }

    public static class Windows_WebServer
    {
        public static bool running = false; // Is it running?

        private static int timeout = 8; // Time limit for data transfers.
        private static Encoding charEncoder = Encoding.UTF8; // To encode string
        private static Socket serverSocket; // Our server socket
        private static string contentPath; // Root path of our contents

        // Content types that are supported by our server
        // You can add more...
        // To see other types: 
        private static Dictionary<string, string> extensions = new Dictionary<string, string>()
        { 
            //{ "extension", "content type" }
            { "htm", "text/html" },
            { "b3dm", "application/zip" },
            { "cmpt", "application/zip" },
            { "js", "application/javascript" },
            { "json", "application/json" },
            { "html", "text/html" },
            { "xml", "text/xml" },
            { "txt", "text/plain" },
            { "css", "text/css" },
            { "png", "image/png" },
            { "gif", "image/gif" },
            { "jpg", "image/jpg" },
            { "jpeg", "image/jpeg" },
            { "zip", "application/zip"}
        };
        public static bool start(IPAddress ipAddress, int port, int maxNOfCon, string contentPath)
        {
            if (running) return false; // If it is already running, exit.

            try
            {
                // A tcp/ip socket (ipv4)
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream,
                               ProtocolType.Tcp);
                serverSocket.Bind(new IPEndPoint(ipAddress, port));
                serverSocket.Listen(maxNOfCon);
                serverSocket.ReceiveTimeout = timeout;
                serverSocket.SendTimeout = timeout;
                running = true;
                Windows_WebServer.contentPath = contentPath;//contentPath;
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"start webserver error({e.Message})");
                return false;
            }

            // Our thread that will listen connection requests
            // and create new threads to handle them.
            Thread requestListenerT = new Thread(() =>
            {
                while (running)
                {
                    Socket clientSocket;
                    try
                    {
                        clientSocket = serverSocket.Accept();
                        // Create new thread to handle the request and continue to listen the socket.
                        Thread requestHandler = new Thread(() =>
                        {
                            clientSocket.ReceiveTimeout = timeout;
                            clientSocket.SendTimeout = timeout;
                            try { handleTheRequest(clientSocket); }
                            catch
                            {
                                try { clientSocket.Close(); } catch { System.Console.WriteLine("requestHandler 异常退出"); }
                            }
                        });
                        requestHandler.Name = "监听服务器";
                        requestHandler.Start();
                    }
                    catch { }
                }
            });
            requestListenerT.Name = "Taoist.Archives Server";
            requestListenerT.Start();
            return true;
        }
        public static void stop()
        {
            if (running)
            {
                running = false;
                try { serverSocket.Close(); }
                catch (System.Exception e) { System.Console.WriteLine("serverSocket 关闭失败-" + e.Message); }
                serverSocket = null;
            }
        }
        private static void handleTheRequest(Socket clientSocket)
        {
            byte[] buffer = new byte[10240]; // 10 kb, just in case
            int receivedBCount = clientSocket.Receive(buffer); // Receive the request
            string strReceived = charEncoder.GetString(buffer, 0, receivedBCount);

            // Parse method of the request
            string httpMethod = strReceived.Substring(0, strReceived.IndexOf(" "));

            int start = strReceived.IndexOf(httpMethod) + httpMethod.Length + 1;
            int length = strReceived.LastIndexOf("HTTP") - start - 1;
            string requestedUrl = strReceived.Substring(start, length);

            string requestedFile;
            if (httpMethod.Equals("GET") || httpMethod.Equals("POST"))
                requestedFile = requestedUrl.Split('?')[0];
            else // You can implement other methods...
            {
                notImplemented(clientSocket);
                return;
            }
            requestedFile = System.Web.HttpUtility.UrlDecode(requestedFile.Replace("+", "%2B"), System.Text.Encoding.UTF8);
           
            requestedFile = requestedFile.Replace('/', '\\').Replace("\\..", "");
            //System.Console.WriteLine("requestedFile:" + requestedFile);
            start = requestedFile.LastIndexOf('.') + 1;
            if (start > 0)
            {
                length = requestedFile.Length - start;
                string extension = requestedFile.Substring(start, length).ToLower();
                if (extensions.ContainsKey(extension)) // Do we support this extension?
                    if (File.Exists(contentPath + requestedFile)) //If yes check existence of the file
                        sendOkResponse(clientSocket,
                          File.ReadAllBytes(contentPath + requestedFile), extensions[extension]);
                    else
                        notFound(clientSocket);

                else {

                    sendResponse(clientSocket, UTF16to8($"<!DOCTYPE html><html lang='zh'><head><meta charset='utf-8'><title>找不到文件</title></head><body><h1>404 - 傻宝 你忘记添加MIME类型了鸭！</h1></body></html>"), "404 Not Implemented", "text/html");
                }
                // We don't support this extension.
                // We are assuming that it doesn't exist.
            }
            else
            {
                if (File.Exists(contentPath + requestedFile + "index.htm"))
                    sendOkResponse(clientSocket,
                      File.ReadAllBytes(contentPath + requestedFile + "\\index.htm"), "text/html");
                else if (File.Exists(contentPath + requestedFile + "index.html"))
                    sendOkResponse(clientSocket,
                      File.ReadAllBytes(contentPath + requestedFile + "\\index.html"), "text/html");
                else if (Directory.Exists(contentPath + requestedFile)) 
                    notStaticFiles(clientSocket, contentPath ,requestedFile);
                else
                  notFound(clientSocket);
            }


        }
        private static void notStaticFiles(Socket clientSocket,string contentPath , string requestedFile) {
            var collection = Directorys.DirectoryStructure.Get(contentPath + requestedFile);
            string _html = Taoist.Archives.Resource.index;

            string index = "";
            foreach (var item in collection)
            {
                //<tr class="directory">
                //    <td class="name"><a href = "./8 18/" > 8 18/</a></td>
                //    <td></td>
                //    <td class="modified">2021/9/28 9:42:20 &#x2B;00:00</td>
                //</tr>

                //<tr class="file">
                //    <td class="name"><a href = "./cesar-louvre-museum.zip" > cesar - louvre - museum.zip </ a ></ td >
                //    <td class="length">48,124,019</td>
                //    <td class="modified">2021/9/7 12:36:17 &#x2B;00:00</td>
                //</tr>
                string uri = "/";
               
                var arr = requestedFile.Split(new char[1] { '\\' });

                for (int i = 0; i < arr.Length; i++)
                {
                    var str = arr[i];
                    if (i != arr.Length - 1) {
                        if (!string.IsNullOrEmpty(str))
                            uri += str.Replace(@"\", "") + "/";
                    }else if (!string.IsNullOrEmpty(str))
                        uri += str.Replace(@"\", "");

                }

                if ("/" != uri)
                    uri += "/" + (item.text.Replace("/", ""));
                else { uri += (item.text.Replace("/", "")); }
               
                if (item.type == "files")
                {
                    index += "<tr class='directory'>" +
                    $"<td class='name'><a href = '{uri}'>{item.text}/</a></td>" +
                    "<td></td> " +
                    $"<td class='modified'>{item.time}</td>" +
                    "</tr>";
                }
                else
                {
                    index += "<tr class='file'>" +
                         $"<td class='name'><a href = '{uri}' >{item.text} </a></td>" +
                         $"<td class='length'>{item.szie}</td>" +
                         $"<td class='modified'>{item.time}</td>" +
                     "</tr>";
                }
          
            }
            string a = "", url = "";
            for (int i = 0; i < requestedFile.Split(new char[1] { '\\' }).Length; i++)
            {
                var item = requestedFile.Split(new char[1] { '\\' })[i];
                if (item != "")
                {
                    url += "/" + item;
                    a += $"<a href='{url}'>{ item + "/"}</a>";
                }
            }
            
            _html = _html.Replace("{aaaa}", a);
            sendResponse(clientSocket, UTF16to8(_html.Replace("{&&&&}", index)), "200 Not Implemented", "text/html");
        }
        private static string UTF16to8(string str) {
            byte[] buffer = Encoding.Unicode.GetBytes(str);
            byte[] _temp = Encoding.Convert(Encoding.Unicode/*原始编码*/, Encoding.UTF8/*目标编码*/, buffer);
            string result = Encoding.UTF8.GetString(_temp);
            return result;
        }
        private static void notImplemented(Socket clientSocket)
        {

            sendResponse(clientSocket, "<html><head><meta " +
             "http -equiv=\"Content-Type\" content=\"text/html; " +
             "charset =utf-8\">" +
             "</head><body><h2>Atasoy Simple Web " +
             "Server </h2><div>501 - Method Not " +
             "Implemented </div></body></html>",
             "501 Not Implemented", "text/html");

        }
        private static void notFound(Socket clientSocket)
        {
            
            sendResponse(clientSocket, "<html><head><meta " +
             "http-equiv=\"Content-Type\" content=\"text/html; " +
             "charset =utf-8\"></head><body><h2>Atasoy Simple Web " +
             "Server </h2><div>404 - Not " +
             "Found </div></body></html>",
             "404 Not Found", "text/html");
        }

        private static void sendOkResponse(Socket clientSocket, byte[] bContent, string contentType)
        {
            sendResponse(clientSocket, bContent, "200 OK", contentType);
        }

        // For strings
        private static void sendResponse(Socket clientSocket, string strContent, string responseCode,
                                  string contentType)
        {
            byte[] bContent = charEncoder.GetBytes(strContent);
            sendResponse(clientSocket, bContent, responseCode, contentType);
        }

        // For byte arrays
        private static void sendResponse(Socket clientSocket, byte[] bContent, string responseCode,
                                  string contentType)
        {
            try
            {
                byte[] bHeader = charEncoder.GetBytes(
                                    "HTTP/1.1 " + responseCode + "\r\n"
                                  + "Server: Atasoy Simple Web Server\r\n"
                                  + "Content-Length: " + bContent.Length.ToString() + "\r\n"
                                  + "Connection: close\r\n"

                                  + "Access-Control-Allow-Methods: OPTIONS,POST,GET\r\n"
                                  + "Access-Control-Allow-Headers: x-requested-with,content-type\r\n"
                                  + "Access-Control-Allow-Origin: *\r\n"


                                  + "Content-Type: " + contentType + "\r\n\r\n");
                clientSocket.Send(bHeader);
                clientSocket.Send(bContent);
                clientSocket.Close();
            }
            catch { }
        }
    }
    public static class Linux_WebServer
    {
        public static bool running = false; // Is it running?

        private static int timeout = 8; // Time limit for data transfers.
        private static Encoding charEncoder = Encoding.UTF8; // To encode string
        private static Socket serverSocket; // Our server socket
        private static string contentPath; // Root path of our contents

        // Content types that are supported by our server
        // You can add more...
        // To see other types: 
        private static Dictionary<string, string> extensions = new Dictionary<string, string>()
        { 
            //{ "extension", "content type" }
            { "htm", "text/html" },
            { "b3dm", "application/zip" },
            { "cmpt", "application/zip" },
            { "js", "application/javascript" },
            { "json", "application/json" },
            { "html", "text/html" },
            { "xml", "text/xml" },
            { "txt", "text/plain" },
            { "css", "text/css" },
            { "png", "image/png" },
            { "gif", "image/gif" },
            { "jpg", "image/jpg" },
            { "jpeg", "image/jpeg" },
            { "zip", "application/zip"}
        };
        public static bool start(IPAddress ipAddress, int port, int maxNOfCon, string contentPath)
        {
            if (running) return false; // If it is already running, exit.

            try
            {
                // A tcp/ip socket (ipv4)
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream,
                               ProtocolType.Tcp);
                serverSocket.Bind(new IPEndPoint(ipAddress, port));
                serverSocket.Listen(maxNOfCon);
                serverSocket.ReceiveTimeout = timeout;
                serverSocket.SendTimeout = timeout;
                running = true;
                Linux_WebServer.contentPath = contentPath;//contentPath;
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"start webserver error({e.Message})");
                return false;
            }

            // Our thread that will listen connection requests
            // and create new threads to handle them.
            Thread requestListenerT = new Thread(() =>
            {
                while (running)
                {
                    Socket clientSocket;
                    try
                    {
                        clientSocket = serverSocket.Accept();
                        // Create new thread to handle the request and continue to listen the socket.
                        Thread requestHandler = new Thread(() =>
                        {
                            clientSocket.ReceiveTimeout = timeout;
                            clientSocket.SendTimeout = timeout;
                            try { handleTheRequest(clientSocket); }
                            catch
                            {
                                try { clientSocket.Close(); } catch { System.Console.WriteLine("requestHandler 异常"); }
                            }
                        });
                        requestHandler.Name = "监听服务器";
                        requestHandler.Start();
                    }
                    catch { }
                }
            });
            requestListenerT.Name = "Taoist.Archives Server";
            requestListenerT.Start();
            return true;
        }
        public static void stop()
        {
            if (running)
            {
                running = false;
                try { serverSocket.Close(); }
                catch (System.Exception e) { System.Console.WriteLine("serverSocket 关闭失败-" + e.Message); }
                serverSocket = null;
            }
        }
        private static void handleTheRequest(Socket clientSocket)
        {
            byte[] buffer = new byte[10240]; // 10 kb, just in case
            int receivedBCount = clientSocket.Receive(buffer); // Receive the request
            string strReceived = charEncoder.GetString(buffer, 0, receivedBCount);

            // Parse method of the request
            string httpMethod = strReceived.Substring(0, strReceived.IndexOf(" "));

            int start = strReceived.IndexOf(httpMethod) + httpMethod.Length + 1;
            int length = strReceived.LastIndexOf("HTTP") - start - 1;
            string requestedUrl = strReceived.Substring(start, length);

            string requestedFile;
            if (httpMethod.Equals("GET") || httpMethod.Equals("POST"))
                requestedFile = requestedUrl.Split('?')[0];
            else // You can implement other methods...
            {
                notImplemented(clientSocket);
                return;
            }

            requestedFile = System.Web.HttpUtility.UrlDecode(requestedFile.Replace("+", "%2B"), System.Text.Encoding.UTF8);
            requestedFile = requestedFile.Replace('/', '\\').Replace("\\..", "");
            System.Console.WriteLine("requestedFile:" + requestedFile);

            start = requestedFile.LastIndexOf('.') + 1;
            if (start > 0)
            {
                length = requestedFile.Length - start;
                string extension = requestedFile.Substring(start, length);
                if (extensions.ContainsKey(extension)) // Do we support this extension?
                    if (File.Exists(contentPath + requestedFile)) //If yes check existence of the file
                                                                  // Everything is OK, send requested file with correct content type:
                        sendOkResponse(clientSocket,
                          File.ReadAllBytes(contentPath + requestedFile), extensions[extension]);
                    else
                        notFound(clientSocket); // We don't support this extension.
                                                // We are assuming that it doesn't exist.
            }
            else
            {
                // If file is not specified try to send index.htm or index.html
                // You can add more (default.htm, default.html)
                if (requestedFile.Substring(length - 1, 1) != @"/")
                    requestedFile += @"/";
                if (File.Exists(contentPath + requestedFile + "index.htm"))
                    sendOkResponse(clientSocket,
                      File.ReadAllBytes(contentPath + requestedFile + "\\index.htm"), "text/html");
                else if (File.Exists(contentPath + requestedFile + "index.html"))
                    sendOkResponse(clientSocket,
                      File.ReadAllBytes(contentPath + requestedFile + "\\index.html"), "text/html");
                else
                    notFound(clientSocket);
            }


        }
        private static void notImplemented(Socket clientSocket)
        {

            sendResponse(clientSocket, "<html><head><meta " +
             "http -equiv=\"Content-Type\" content=\"text/html; " +
             "charset =utf-8\">" +
             "</head><body><h2>Atasoy Simple Web " +
             "Server </h2><div>501 - Method Not " +
             "Implemented </div></body></html>",
             "501 Not Implemented", "text/html");

        }

        private static void notFound(Socket clientSocket)
        {
            sendResponse(clientSocket, "<html><head><meta " +
             "http-equiv=\"Content-Type\" content=\"text/html; " +
             "charset =utf-8\"></head><body><h2>Atasoy Simple Web " +
             "Server </h2><div>404 - Not " +
             "Found </div></body></html>",
             "404 Not Found", "text/html");
        }

        private static void sendOkResponse(Socket clientSocket, byte[] bContent, string contentType)
        {
            sendResponse(clientSocket, bContent, "200 OK", contentType);
        }

        // For strings
        private static void sendResponse(Socket clientSocket, string strContent, string responseCode,
                                  string contentType)
        {
            byte[] bContent = charEncoder.GetBytes(strContent);
            sendResponse(clientSocket, bContent, responseCode, contentType);
        }

        // For byte arrays
        private static void sendResponse(Socket clientSocket, byte[] bContent, string responseCode,
                                  string contentType)
        {
            try
            {
                byte[] bHeader = charEncoder.GetBytes(
                                    "HTTP/1.1 " + responseCode + "\r\n"
                                  + "Server: Atasoy Simple Web Server\r\n"
                                  + "Content-Length: " + bContent.Length.ToString() + "\r\n"
                                  + "Connection: close\r\n"

                                  + "Access-Control-Allow-Methods: OPTIONS,POST,GET\r\n"
                                  + "Access-Control-Allow-Headers: x-requested-with,content-type\r\n"
                                  + "Access-Control-Allow-Origin: *\r\n"


                                  + "Content-Type: " + contentType + "\r\n\r\n");
                clientSocket.Send(bHeader);
                clientSocket.Send(bContent);
                clientSocket.Close();
            }
            catch { }
        }
    }

}
