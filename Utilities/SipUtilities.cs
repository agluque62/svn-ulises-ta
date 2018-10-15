using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class SipUtilities
    {
        public class SipUriParser
        {
            public SipUriParser(string uri_str)
            {
                UriStr = uri_str.Replace("<", "").Replace(">", "").Split(':');
            }
            public string User
            {
                get
                {
                    if (UriStr.Length > 1)
                    {
                        string[] user_fields = UriStr[1].Split('@');
                        if (user_fields.Length > 1)
                            return user_fields[0];
                        return "";
                    }

                    return "????";
                }
            }
            public string Dominio
            {
                get
                {
                    if (UriStr.Length > 1)
                    {
                        string[] user_fields = UriStr[1].Split('@');
                        if (user_fields.Length == 1)
                        {
                            return UriStr.Length > 2 ? user_fields[0] + ":" + UriStr[2] : user_fields[0];
                        }
                        else if (user_fields.Length == 2)
                        {
                            return UriStr.Length > 2 ? user_fields[1] + ":" + UriStr[2] : user_fields[1];
                        }
                        return "";
                    }

                    return "????";
                }
            }

            /** 20180625. AGL. */
            public string Host
            {
                get
                {
                    string[] fields = Dominio.Split(':');
                    return fields.Length >= 0 ? fields[0] : "???";
                }
            }
            public int Port
            {
                get
                {
                    string[] fields = Dominio.Split(':');
                    int port = 0;
                    return (fields.Length > 1 && Int32.TryParse(fields[1], out port)) ? port : 5060;
                }
            }
            public string UlisesFormat
            {
                get
                {
                    return String.Format("<sip:{0}@{1}:{2}>", User, Host, Port);
                }
            }


            string[] UriStr { get; set; }
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
                    return false;
                }
            }
        }
    }

}
