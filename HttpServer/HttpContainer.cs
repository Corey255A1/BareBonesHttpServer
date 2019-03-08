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
        private string _data;
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
        public void SetData(string data)
        {
            if(!_properties.ContainsKey("Content-Length"))
            {
                this._properties.Add("Content-Length", "");
            }
            this._properties["Content-Length"] = (data.Length+2).ToString();
            this._data = data;
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
            return header + base.ToString() + string.Format("\r\n{0}\r\n", _data);
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

        protected virtual bool SplitHeader(string header)
        {
            return true;
        }
    }
}
