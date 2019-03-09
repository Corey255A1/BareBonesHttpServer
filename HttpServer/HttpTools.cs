// Corey Wunderlich - 2019
// www.wundervisionenvisionthefuture.com
//
// Static Functions to help with various http things
using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
namespace SimpleHttpServer
{
    public static class HttpTools
    {

        public static string ComputeWebSocketKeyHash(string websocketkey)
        {
            //Magic String
            string concat = websocketkey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            var s = SHA1.Create();
            byte[] hash = s.ComputeHash(System.Text.Encoding.UTF8.GetBytes(concat));
            return Convert.ToBase64String(hash);
        }

        public static string GetFileMimeType(string filename)
        {
            var dotoffset = filename.LastIndexOf('.');
            if(dotoffset == -1)
            {
                return "text/html";
            }
            switch(filename.Substring(dotoffset).ToLower())
            {
                case ".ico": return "image/x-icon";
                case ".jpg": return "image/jpeg";
                case ".png": return "image/png";
                case ".gif": return "image/gif";
                case ".css": return "text/css";
                case ".js": return "text/javascript";
                case ".json": return "application/json";
                case ".html":
                case ".htm":
                default:
                    return "text/html";

            }
        }
        public static bool IsFileBinary(string filename)
        {
            var dotoffset = filename.LastIndexOf('.');
            if (dotoffset == -1)
            {
                return true;
            }
            switch (filename.Substring(dotoffset).ToLower())
            {
                case ".css":
                case ".js":
                case ".json":
                case ".html":
                case ".htm":
                    return false;

                case ".ico":
                case ".jpg":
                case ".png":
                case ".gif":
                default:
                    return true;

            }
        }
    }
}
