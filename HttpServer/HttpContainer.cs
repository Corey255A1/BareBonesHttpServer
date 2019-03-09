// Corey Wunderlich - 2019
// www.wundervisionenvisionthefuture.com
//
// Classes to handle Request and Response Http Packets
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleHttpServer
{    

    public class HttpRequest: HttpContainer
    {
        public HttpRequest(string msg): base(msg)
        {

        }
        protected override bool SplitHeader(string header)
        {
            string[] pieces = header.Split(' ');
            if (pieces.Length < 3)
            {
                return false;
            }
            _properties.Add("Request", pieces[0]);
            _properties.Add("URI", pieces[1]);
            _properties.Add("Protocol", pieces[2]);
            return true;
        }

    }

    public class HttpResponse: HttpContainer
    {
        private byte[] _data;
        private string _code;
        private string _protocol;
        private string _respmessage;
        public HttpResponse(string protocol, string code, string respmsg)
        {
            _code = code;
            _respmessage = respmsg;
            _protocol = protocol;
        }
        public HttpResponse(string msg) : base(msg) { }

        public void AddProperty(string field, string data)
        {
            this._properties.Add(field, data);
        }
        public void SetData(byte[] bytes)
        {
            if (!_properties.ContainsKey("Content-Length"))
            {
                this._properties.Add("Content-Length", "");
            }
            this._properties["Content-Length"] = bytes.Length.ToString();
            this._data = bytes;
        }
        public void SetData(string data)
        {
            this.SetData(System.Text.Encoding.UTF8.GetBytes(data));
        }
        protected override bool SplitHeader(string header)
        {
            string[] pieces = header.Split(' ');
            if (pieces.Length < 3)
            {
                return false;
            }
            _protocol = pieces[0];
            _code = pieces[1];
            _respmessage = pieces[2];
            return true;
        }

        public override string ToString()
        {
            string header = String.Format("{0} {1} {2}\r\n", _protocol, _code, _respmessage);
            return header + base.ToString() + string.Format("\r\n{0}", _data!=null?System.Text.Encoding.UTF8.GetString(_data):"");
        }

        public override byte[] GetBytes()
        {
            string header = String.Format("{0} {1} {2}\r\n", _protocol, _code, _respmessage);
            byte[] hbytes = Encoding.UTF8.GetBytes(header + base.ToString() +"\r\n");
            byte[] totalbytes = hbytes;
            if (_data != null)
            {
                totalbytes = new byte[hbytes.Length + _data.Length];
                Array.Copy(hbytes, totalbytes, hbytes.Length);
                Array.Copy(_data, 0, totalbytes, hbytes.Length, _data.Length);
            }

            return totalbytes;
        }

    }


    public class HttpContainer
    {
        public Dictionary<string, string> _properties = new Dictionary<string, string>();
        static char[] msgSplit = new char[]{':'};
        static char[] linesplit = new char[] { '\n' };
        public HttpContainer() { }
        public HttpContainer(string msg)
        {
            try
            {
                string[] msglines = msg.Split(linesplit, StringSplitOptions.RemoveEmptyEntries);
                if (SplitHeader(msglines[0]))
                {
                    for (int i = 1; i < msglines.Length; i++)
                    {
                        string[] field = msglines[i].Split(msgSplit, 2);
                        if (field.Length == 2)
                        {
                            if (_properties.ContainsKey(field[0])) _properties[field[0]] = field[1].Trim();
                            else _properties.Add(field[0], field[1].Trim());
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public bool ContainsKey(string key)
        {
            return this._properties.ContainsKey(key);
        }

        public string this[string obj]
        {
            get
            {
                return _properties.ContainsKey(obj) ? _properties[obj] : "";
            }
            set
            {
                if (_properties.ContainsKey(obj)) { _properties[obj] = value; } else { _properties.Add(obj, value); }
            }
        }

        public override string ToString()
        {
            StringBuilder msg = new StringBuilder(256);
            foreach(var k in _properties.Keys)
            {
                msg.AppendFormat("{0}: {1}\r\n", k, _properties[k]);
            }
            return msg.ToString();

        }

        public virtual byte[] GetBytes()
        {
            StringBuilder msg = new StringBuilder(256);
            foreach (var k in _properties.Keys)
            {
                msg.AppendFormat("{0}: {1}\r\n", k, _properties[k]);
            }
            return Encoding.UTF8.GetBytes(msg.ToString());
        }

        protected virtual bool SplitHeader(string header)
        {
            return true;
        }
    }
}
