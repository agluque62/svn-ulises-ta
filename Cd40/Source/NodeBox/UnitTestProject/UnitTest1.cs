using System;
using System.Collections.Generic;
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

    [TestClass]
    public class ServicesHelpersTest
    {
        [TestMethod]
        public void VersionsFileAdjustTest_Radio()
        {
            ///** Test Radio */
            ServicesHelpers.VersionsFileAdjust("RadioService", new List<string>() { "CfgService", "RdService", "RemoteControlService" });
        }

        [TestMethod]
        public void VersionsFileAdjustTest_Phone()
        {
            ///** Test Phone */
            ServicesHelpers.VersionsFileAdjust("PhoneService", new List<string>() { "TifxService", "PresenceService", "PhoneService" });
        }

        [TestMethod]
        public void VersionsFileAdjustTest_Mixed()
        {
            ///** Test Mixed */
            ServicesHelpers.VersionsFileAdjust("Nodebox", null);
        }
    }

}
