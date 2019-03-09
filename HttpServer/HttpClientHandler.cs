// Corey Wunderlich - 2019
// www.wundervisionenvisionthefuture.com
//
// Handles receiving data from a client and notifying the server of the
// data that is available. Decodes regular Http and Websocktes
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
namespace SimpleHttpServer
{
    public delegate void HttpRequestDataCallback(HttpClientHandler client, HttpRequest packet);
    public delegate void HttpWebSocketDataCallback(HttpClientHandler client, WebSocketFrame packet);
    public delegate void HttpClientHandlerEvent(EndPoint end, HttpClientHandler client);
    public class HttpClientHandler
    {

        public HttpRequestDataCallback HttpRequestReceived;
        public HttpWebSocketDataCallback WebSocketDataReceived;
        public HttpClientHandlerEvent ClientDisconnected;

        private bool WebSocketUpgrade = false;
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

        private async void BeginReadData()
        {
            try
            {
                int bytesread = await _stream.ReadAsync(_buffer, 0, BUFFERSIZE);
                while (bytesread > 0)
                {
                    
                    if (!this.WebSocketUpgrade)
                    {
                        string msg = System.Text.Encoding.UTF8.GetString(_buffer);
                        HttpRequest h = new HttpRequest(msg);
                        HttpRequestReceived?.Invoke(this, h);
                    }
                    else
                    {
                        //Not handling Multiple Frames worth of data...
                        WebSocketFrame frame = new WebSocketFrame(_buffer);
                        WebSocketDataReceived?.Invoke(this, frame);
                        //Console.WriteLine(Encoding.UTF8.GetString(frame.Payload));
                    }
                    bytesread = await _stream.ReadAsync(_buffer, 0, BUFFERSIZE);
                }
            }
            catch
            {
                Console.WriteLine("Client Read Aborted");
            }
            Console.WriteLine("DONE");
            ClientDisconnected?.Invoke(_client.Client.RemoteEndPoint, this);
        }
        public void UpgradeToWebsocket()
        {
            this.WebSocketUpgrade = true;
        }
        public void Send(string text)
        {
            this.Send(System.Text.Encoding.UTF8.GetBytes(text));
        }
        public void Send(byte[] bytes)
        {
            _stream.Write(bytes, 0, bytes.Length);
        }
        
        public void Close()
        {
            if (_client.Connected)
            {
                _client.Close();
            }
        }
    }
}
