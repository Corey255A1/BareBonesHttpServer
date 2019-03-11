// Corey Wunderlich - 2019
// www.wundervisionenvisionthefuture.com
//
// This program is serving as the example working project that is used to test
// and build various parts of the http server functionality
using System;
using SimpleHttpServer;
using System.IO;

namespace SimpleWebHost
{
    class Program
    {
        static HttpServer Server;
        static string Site = "I'M A BLANK SITE";
        //static Uri Root = new Uri(@"D:\Documents\CodeProjects\Corey255A1.github.io");
        //static Uri Root = new Uri(@"D:\Documents\CodeProjects\BareBonesHttpServer\ExampleSites\WebSocket");
        static Uri Root = new Uri(@"D:\Documents\CodeProjects\BareBonesHttpServer\ExampleSites\MultiFile");


        static void Main(string[] args)
        {
            //string concat = "dGhlIHNhbXBsZSBub25jZQ==" + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            //var s = SHA1.Create();
            //byte[] hash = s.ComputeHash(System.Text.Encoding.UTF8.GetBytes(concat));
            //Console.WriteLine(Convert.ToBase64String(hash)); //s3pPLMBiTxaQ9kYGzzhZRbK+xOo= - Demo From Mozilla

            Server = new HttpServer(80);
            Site = File.ReadAllText(Root.LocalPath + "\\index.html", System.Text.Encoding.UTF8);
            Server.MessageCallback += Status;
            Server.HttpRequestReceived += ClientRequest;
            Server.HttpWebSocketDataReceived += WebSocketData;
            Server.StartListening();
            Console.WriteLine("Listening...");


            Console.ReadKey();
            Server.Stop();
        }

        private static void Status(HttpServer s, string msg)
        {
            Console.WriteLine(msg);

        }

        private static void WebSocketData(HttpClientHandler client, WebSocketFrame data)
        {
            Console.WriteLine(System.Text.Encoding.UTF8.GetString(data.Payload));

            WebSocketFrame response = new WebSocketFrame("This is a WebSocket server response!");
            client.Send(response);

        }

        private static void ClientRequest(HttpClientHandler client, HttpRequest req)
        {
            HttpResponse resp = null;
            //Console.WriteLine("--- REQUEST ---");
            //Console.WriteLine(req.ToString());
            if (req["Request"] == "GET")
            {
                string uri = req["URI"];
                if (uri == "/")
                {
                    resp = new HttpResponse("HTTP/1.1", "200", "OK");
                    resp.AddProperty("Date", DateTime.Now.ToShortDateString());
                    resp.AddProperty("Server", "WunderVision");
                    resp.AddProperty("Content-Type", "text/html;charset=UTF-8");
                    resp.SetData(Site);
                }
                else
                {

                    if(req.ContainsKey("Sec-WebSocket-Key"))
                    {
                        resp = new HttpResponse("HTTP/1.1", "101", "Switching Protocols");
                        resp.AddProperty("Upgrade", "websocket");
                        resp.AddProperty("Connection", "Upgrade");
                        //Console.WriteLine(req["Sec-WebSocket-Key"]);
                        resp.AddProperty("Sec-WebSocket-Accept", HttpTools.ComputeWebSocketKeyHash(req["Sec-WebSocket-Key"]));
                        client.UpgradeToWebsocket();
                    }
                    else
                    {
                        Uri requestedfile = new Uri(Root + uri);
                        //Console.WriteLine(requestedfile.LocalPath);
                        if (File.Exists(requestedfile.LocalPath))
                        {
                            string mime = HttpTools.GetFileMimeType(uri);
                            //Console.WriteLine(mime);
                            byte[] data;
                            if (HttpTools.IsFileBinary(uri))
                            {
                                data = File.ReadAllBytes(requestedfile.LocalPath);
                            }
                            else
                            {
                                data = System.Text.Encoding.UTF8.GetBytes(File.ReadAllText(requestedfile.LocalPath));
                            }
                            resp = new HttpResponse("HTTP/1.1", "200", "OK");
                            resp.AddProperty("Date", DateTime.Now.ToShortDateString());
                            resp.AddProperty("Server", "WunderVision");
                            resp.AddProperty("Content-Type", mime);
                            resp.SetData(data);
                        }

                    }

                    
                    
                }
            }
            if (resp == null)
            {
                resp = new HttpResponse("HTTP/1.1", "404", "NOT FOUND");
                resp.AddProperty("Date", DateTime.Now.ToShortDateString());
                resp.AddProperty("Server", "WunderVision");
                resp.AddProperty("Content-Type", "text/html;charset=UTF-8");
                resp.SetData("SORRY CAN'T DO WHAT YOU WANT ME TO");
            }
            //Console.WriteLine("--- RESPONSE ---");
            //Console.WriteLine(resp.ToString());
            client.Send(resp);
        }
    }
}
