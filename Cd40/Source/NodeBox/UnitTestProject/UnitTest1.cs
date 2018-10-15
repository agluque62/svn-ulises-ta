using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Forms;

using U5ki.NodeBox;
using Utilities;

namespace UnitTestProject
{
    [TestClass]
    public class NbxWebServerUnitTest
    {
        NbxWebServer _webServer = new NbxWebServer("d:\\Datos\\Empresa\\_SharedPrj\\UlisesV5000i-trunk\\ulises-ta\\Sources\\Cd40\\Source\\NodeBox");

        [TestMethod]
        public void NbxWebServerTestMethod1()
        {
            _webServer.WebSrvCommand += (cmd, datain) =>
            {
                return null;
            };
            _webServer.Start(14441);

            MessageBox.Show("WebServerArrancado");

            _webServer.Dispose();
        }
    }

    [TestClass]
    public class SipUtilitiesTest
    {
        [TestMethod]
        public void TestSipUriParser()
        {
            SipUtilities.SipUriParser sipuri = new SipUtilities.SipUriParser("<sip:192.168.0.129:7060>");
            string stdf = sipuri.UlisesFormat;
        }
    }
}
