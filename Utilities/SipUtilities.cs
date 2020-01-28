using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Utilities
{
    public class SipUtilities
    {
        private static Logger _Logger = LogManager.GetCurrentClassLogger();
        public class SipUriParser
        {
            private string _User = "";
            private string _Host = "";
            private string _Port = "5060";
            private string _DisplayName = "";
            private string _Resource = "";

            public SipUriParser(string uri_str)
            {
                string[] UriStrUser = null;
                string[] UriStrTmp = uri_str.Replace(">", "").Replace("<", "").Replace("sip:", "").Replace("tel:", "").Split(':');
                if (UriStrTmp[0].Contains('@'))
                {
                    UriStrUser = UriStrTmp[0].Split('@');
                    if (UriStrTmp.Length > 1)
                        _Port = UriStrTmp[1];                  
                    UriStrTmp = UriStrUser[0].Split('"');
                    _User = UriStrUser[0];
                    if (UriStrTmp.Length == 3)
                    {
                        _User = UriStrTmp[2];
                        _DisplayName = UriStrTmp[1];
                    }
                    _Host = UriStrUser[1].Split(';').First();
                    _Resource = UriStrUser[1].Split(';').Last().Replace ("cd40rs=", "");
                }
                else if ((UriStrTmp.Length > 1) && (UriStrTmp[1].Contains('@')))
                {
                    UriStrUser = UriStrTmp[1].Split('@');
                    if (UriStrTmp.Length > 2)
                        _Port = UriStrTmp[2];
                    _User = UriStrTmp[0];
                    if (UriStrTmp.Length == 3)
                    {
                        _User = UriStrTmp[2];
                        _DisplayName = UriStrTmp[1];
                    }
                    if (UriStrTmp.Length > 1)
                        _Host = UriStrUser[1].Split(';').First();
                }
                else
                {
                    _Host = UriStrTmp[0];
                    if (UriStrTmp.Length > 1)
                        _Port = UriStrTmp[1];
                }
            }
            public SipUriParser(string user, string ip, uint port)
            {
                _User = user;
                _Host = ip;
                _Port = port.ToString();
            }
            public SipUriParser(string user, string ip)
            {
                _User = user;
                _Host = ip;
                _Port = "5060";
            }

            public string User
            {
                get { return _User; }
                set { _User = value; }
            }

            public string DisplayName
            {
                get { return _DisplayName; }
                set { _DisplayName = value; }
            }
            public string Resource
            {
                get { return _Resource; }
                set { _Resource = value; }
            }
            public string HostPort
            {
                get
                {
                    return Host + ":" + Port;
                }
            }

            /** 20180625. AGL. */
            public string Host
            {
                get { return _Host; }
                set { _Host = value; }
            }
            public int Port
            {
                get
                {
                    int port;
                    Int32.TryParse(_Port, out port);
                    return port;
                }
                set { _Port = value.ToString(); }
            }
            public string UlisesFormat
            {
                get
                {
                    if (User.Length > 0)
                        return String.Format("<sip:{0}@{1}:{2}>", User, Host, Port);
                    else
                        return String.Format("<sip:{0}:{1}>", Host, Port);
                }
            }
        }
        public static bool EqualSIPIPAddress (string ip1, string ip2)
        {
           string defaultSipPort = ":5060";
           String[] ipFields1 = ip1.Split(':');
           if (ipFields1.Count() == 1)
               ip1 += defaultSipPort;
           String[] ipFields2 = ip2.Split(':');
           if (ipFields2.Count() == 1)
               ip2 += defaultSipPort;

            
            return (String.Compare(ip1, ip2) == 0);
        }
        public class SipEndPoint
        {
            /// <summary>
            /// Retorna un EndPoint en funcion de especificacion en STRING
            /// </summary>
            /// <param name="endpoint"></param>
            /// <returns></returns>
            public static IPEndPoint Parse(string endpoint)
            {
                string[] parts = endpoint.Split(':');
                string ip = parts[0];
                string port = parts.Count() == 2 ? parts[1] : "5060";

                IPAddress Ip;
                int Port;
                if (IPAddress.TryParse(ip, out Ip) == false ||
                    Int32.TryParse(port, out Port) == false)
                    return null;

                return new IPEndPoint(Ip, Port);
            }
            /// <summary>
            ///Compara dos cadenas que representan ip address con o sin endPoint. 
            ///Si no lleva endpoint se da por supuesto que es el 5060
            /// <param name="endPoint1"></param>
            /// <param name="endPoint2">/param>
            /// <returns>bool, true si son iguales, false si son diferentes</returns>
            public static bool EqualSipEndPoint(string endPoint1, string endPoint2)
            {
                try
                {
                    IPEndPoint ipEp1 = Parse(endPoint1);
                    return ipEp1.Equals(Parse(endPoint2));
                }
                catch (Exception exc)
                {
                    _Logger.Error(String.Format("Error SipUtilities EqualSipEndPoint {0} {1}", endPoint1, endPoint2));
                    return false;
                }
            }
        }
    }

}
