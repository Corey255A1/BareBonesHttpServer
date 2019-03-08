using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
namespace SimpleHttpServer
{
    public delegate void HttpRequestDataCallback(HttpClientHandler client, HttpRequest packet);
    public class HttpClientHandler
    {

        public HttpRequestDataCallback HttpRequestReceived;
        public string ClientInfo
        {
            get { return _client.Client.RemoteEndPoint.ToString(); }
        }
        TcpClient _client;
        NetworkStream _stream;

        const int BUFFERSIZE = 1024;
        byte[] _buffer = new byte[BUFFERSIZE];
        
        public HttpClientHandler(TcpClient c)
        {
            _client = c;
            _stream = c.GetStream();
            BeginReadData();
        }
        public void Send(string text)
        {
            byte[] b = System.Text.Encoding.UTF8.GetBytes(text);
            _stream.Write(b, 0, b.Length);
        }
        public async void BeginReadData()
        {
            try
            {
                int bytesread = await _stream.ReadAsync(_buffer, 0, BUFFERSIZE);
                while (bytesread > 0)
                {
                    string msg = System.Text.Encoding.UTF8.GetString(_buffer);
                    //Console.WriteLine(msg);
                    HttpRequest h = new HttpRequest(msg);
                    HttpRequestReceived?.Invoke(this, h);
                    //HttpResponse resp = new HttpResponse("HTTP/1.1","200","OK");
                    //resp.AddProperty("Date", DateTime.Now.ToShortDateString());
                    //resp.AddProperty("Server", "WunderVision");
                    //resp.AddProperty("Content-Type", "text/html;charset=UTF-8");
                    //resp.SetData("THIS IS A RESPONSE FROM A TOTALLY LEGIT SERVER");

                    //string respstr = req.ToString();
                    //    //"HTTP/1.1 200 OK\n" +
                    //    //"Date: 7:34\n" +
                    //    //"Server: WunderVision\n" +
                    //    //"Content-Type: text/html;charset=UTF-8\n" +
                    //    //"Content-Length: 5\n" +
                    //    //"\n" +
                    //    //"Hello";
                    //_stream.Write(System.Text.Encoding.UTF8.GetBytes(respstr), 0, resp.Length);
                    bytesread = await _stream.ReadAsync(_buffer, 0, BUFFERSIZE);
                    
                }
            }
            catch
            {
                Console.WriteLine("Client Read Aborted");
            }
            Console.WriteLine("DONE");
        }
        public void Disconnect()
        {
            _client.Close();
        }
    }
}
