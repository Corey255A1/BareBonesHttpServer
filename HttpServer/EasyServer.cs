// Corey Wunderlich - 2019
// www.wundervisionenvisionthefuture.com

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
namespace SimpleHttpServer
{
    public struct HttpRequestHandler
    {
        public byte[] Data;
        public string Mime;
    }
    public class EasyServer
    {
        private HttpServer _server;
        private int _port;
        private Dictionary<string, Func<HttpRequestHandler>> _responseHandlers = new Dictionary<string, Func<HttpRequestHandler>>();
        public string Root
        {
            get; set;
        }
        public EasyServer(int port)
        {
            this._port = port;
        }
        public void Start()
        {
            this._server = new HttpServer(this._port);
            this._server.MessageCallback += Status;
            this._server.HttpRequestReceived += ClientRequest;
            this._server.HttpWebSocketDataReceived += WebSocketData;
            this._server.StartListening();
        }

        public void AddResponseHandler(string uri, Func<HttpRequestHandler> handler)
        {
            if(_responseHandlers.ContainsKey(uri))
            {
                _responseHandlers[uri] = handler;
            }
            else
            {
                _responseHandlers.Add(uri, handler);
            }
        }

        private void Status(HttpServer s, string msg)
        {
            Console.WriteLine(msg);
        }

        private void WebSocketData(HttpClientHandler client, WebSocketFrame data)
        {
            Console.WriteLine(System.Text.Encoding.UTF8.GetString(data.Payload));

            WebSocketFrame response = new WebSocketFrame("This is a WebSocket server response!");
            client.Send(response);
        }

        private void ClientRequest(HttpClientHandler client, HttpRequest req)
        {
            HttpResponse resp = null;
            if (req["Request"] == "GET")
            {
                if (req.ContainsKey("Sec-WebSocket-Key"))
                {
                    resp = new HttpResponse("HTTP/1.1", "101", "Switching Protocols");
                    resp.AddProperty("Upgrade", "websocket");
                    resp.AddProperty("Connection", "Upgrade");
                    resp.AddProperty("Sec-WebSocket-Accept", HttpTools.ComputeWebSocketKeyHash(req["Sec-WebSocket-Key"]));
                    client.UpgradeToWebsocket();
                }
                else
                {
                    string uri = req["URI"];
                    if (_responseHandlers.ContainsKey(uri))
                    {
                        var handlerdata = _responseHandlers[uri]();
                        resp = new HttpResponse("HTTP/1.1", "200", "OK");
                        resp.AddProperty("Date", DateTime.Now.ToShortDateString());
                        resp.AddProperty("Server", "WunderVision");
                        resp.AddProperty("Content-Type", handlerdata.Mime);
                        resp.SetData(handlerdata.Data);
                    }
                    else if (this.Root != null && this.Root != "")
                    {
                        Uri requestedfile = new Uri(Root + uri);
                        if (File.Exists(requestedfile.LocalPath))
                        {
                            string mime = HttpTools.GetFileMimeType(uri);
                            byte[] data = File.ReadAllBytes(requestedfile.LocalPath);
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
                resp.SetData("SORRY, I CAN'T DO THAT DAVE");
            }
            client.Send(resp);
        }

    }
}
