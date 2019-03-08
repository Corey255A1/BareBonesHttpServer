using System;
using SimpleHttpServer;
using System.IO;
namespace SimpleWebHost
{
    class Program
    {
        static HttpServer Server;
        static string Site = "I'M A BLANK SITE";
        static Uri Root = new Uri(@"D:\Documents\CodeProjects\Corey255A1.github.io");
        static void Main(string[] args)
        {
            Server = new HttpServer(80);
            Site = File.ReadAllText(Root.LocalPath + "\\index.html", System.Text.Encoding.UTF8);
            Server.MessageCallback += Status;
            Server.HttpRequestReceived += ClientRequest;
            Server.StartListening();
            Console.WriteLine("Listening...");


            Console.ReadKey();
            Server.Stop();
        }

        static void Status(HttpServer s, string msg)
        {
            Console.WriteLine(msg);

        }

        private static void ClientRequest(HttpClientHandler client, HttpRequest req)
        {
            HttpResponse resp = null;
            Console.WriteLine(req.ToString());
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
                    Uri requestedfile = new Uri(Root + uri);
                    Console.WriteLine(requestedfile.LocalPath);
                    if(File.Exists(requestedfile.LocalPath))
                    {
                        string mime = HttpTools.GetFileMimeType(uri);
                        Console.WriteLine(mime);
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
            if (resp == null)
            {
                resp = new HttpResponse("HTTP/1.1", "404", "NOT FOUND");
                resp.AddProperty("Date", DateTime.Now.ToShortDateString());
                resp.AddProperty("Server", "WunderVision");
                resp.AddProperty("Content-Type", "text/html;charset=UTF-8");
                resp.SetData("SORRY CAN'T DO WHAT YOU WANT ME TO");
            }
            client.Send(resp.GetBytes());
        }
    }
}
