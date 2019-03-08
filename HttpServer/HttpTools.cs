using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleHttpServer
{
    public static class HttpTools
    {
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
    }
}
