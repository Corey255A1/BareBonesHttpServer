using System;
using System.Net.Sockets;
using System.Net;
using System.Collections.Concurrent;
namespace SimpleHttpServer
{
    public delegate void HttpServerMessageCallback(HttpServer o, string msg);
    public delegate bool HttpClientConnected(TcpClient t);
    public class HttpServer
    {
        public HttpServerMessageCallback MessageCallback;
        public HttpRequestDataCallback HttpRequestReceived;
        public HttpClientConnected HttpNewClientConnected;
        private void Message(string msg) { MessageCallback?.Invoke(this, msg); }
        TcpListener _server;
        private int _port;
        private ConcurrentBag<HttpClientHandler> _clients = new ConcurrentBag<HttpClientHandler>();
        private bool _running = false;
        public HttpServer(int port)
        {
            this._port = port;
            this._server = new TcpListener(IPAddress.Any, port);            
        }
        public async void StartListening()
        {            
            try
            {
                this._server.Start();
                _running = true;
                while (_running)
                {                    
                    Message("Waiting For Connection");
                    var client = await this._server.AcceptTcpClientAsync();
                    if ((HttpNewClientConnected!=null && HttpNewClientConnected.Invoke(client)) || HttpNewClientConnected==null)
                    {
                        var httpc = new HttpClientHandler(client);
                        httpc.HttpRequestReceived += HttpRequestReceived;
                        _clients.Add(httpc);
                    }
                }
            }
            catch
            {
                Message("Exception While Accepting Clients");
            }
        }
        public void Stop()
        {
            foreach (var client in _clients)
            {
                client.Disconnect();
            }
            _running = false;
            _server.Stop();
        }

        //private void ClientRequest(HttpClientHandler client, HttpRequest req)
        //{
        //    Message(req.ToString() + "\nfrom " + client.ClientInfo);
        //    HttpResponse resp = new HttpResponse("HTTP/1.1", "200", "OK");
        //    resp.AddProperty("Date", DateTime.Now.ToShortDateString());
        //    resp.AddProperty("Server", "WunderVision");
        //    resp.AddProperty("Content-Type", "text/html;charset=UTF-8");
        //    resp.SetData("THIS IS A RESPONSE FROM A TOTALLY LEGIT SERVER");
        //    client.Send(resp.ToString());
        //}
    }
}
