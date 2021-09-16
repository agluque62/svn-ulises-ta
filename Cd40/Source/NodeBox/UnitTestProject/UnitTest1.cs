using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

using U5ki.NodeBox;
using U5ki.NodeBox.WebServer;
using Utilities;

namespace UnitTestProject
{
    [TestClass]
    public class NbxWebServerUnitTest
    {
        //const string RootDirectory = "c:\\Users\\arturo.garcia\\source\\repos\\nucleocc\\dev-branches\\ulises-ta\\Cd40\\Source\\NodeBox";
        const string RootDirectory = "..\\..\\..\\..\\NodeBox";
        void PrepareTest(Action<WebAppServer> execute)
        {
            using (var server = new WebAppServer(RootDirectory))
            {
                server.Start(1444, new Dictionary<string, WebAppServer.wasRestCallBack>());
                execute(server);
                server.Stop();
            }
        }
        [TestMethod]
        public void NbxWebServerTestMethod1()
        {
            PrepareTest((server) =>
            {
                Task.Delay(TimeSpan.FromSeconds(60)).Wait();
            });
        }
    }

    [TestClass]
    public class SipUtilitiesTest
    {
        [TestMethod]
        public void TestSipUriParser()
        {
            SipUtilities.SipUriParser sipuri = new SipUtilities.SipUriParser("<sip:\"pp\"192.168.0.129:7060>");
            string stdf = sipuri.UlisesFormat;

            sipuri = new SipUtilities.SipUriParser("<sip:\"pp\"bb@192.168.0.129:7060>");
            Assert.AreEqual(sipuri.User, "bb");
            Assert.AreEqual(sipuri.Port, 7060);
            Assert.AreEqual(sipuri.Host, "192.168.0.129");
            Assert.AreEqual(sipuri.DisplayName, "pp");

            sipuri = new SipUtilities.SipUriParser("<sip:bb@192.168.0.129:7060>");
            Assert.AreEqual(sipuri.User, "bb");
            Assert.AreEqual(sipuri.Port, 7060);
            Assert.AreEqual(sipuri.Host, "192.168.0.129");
            Assert.AreEqual(sipuri.DisplayName, "");

            sipuri = new SipUtilities.SipUriParser("<sip:\"pp\"bb@192.168.0.129>");
            Assert.AreEqual(sipuri.User, "bb");
            Assert.AreEqual(sipuri.Port, 5060);
            Assert.AreEqual(sipuri.Host, "192.168.0.129");
            Assert.AreEqual(sipuri.DisplayName, "pp");

            sipuri = new SipUtilities.SipUriParser("sip:0682696141@192.168.2.18;cd40rs=RTB-REC");
            Assert.AreEqual(sipuri.User, "0682696141");
            Assert.AreEqual(sipuri.Port, 5060);
            Assert.AreEqual(sipuri.Host, "192.168.2.18");
            Assert.AreEqual(sipuri.DisplayName, "");
            Assert.AreEqual(sipuri.Resource, "RTB-REC");
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

    [TestClass]
    public class JsonTests
    {
        [TestMethod]
        public void JobjectTest()
        {
            string json = "[{\"nombre\": \"arturo\", \"entero\": 22 }]";

            var json_object = JsonConvert.DeserializeObject(json) as JObject;

            var json_array = JsonConvert.DeserializeObject(json) as JArray;


        }

        [TestMethod]
        public void NLogTest()
        {
            NLog.LogManager.GetCurrentClassLogger().Info("Prueba....");

            var config = NLog.LogManager.Configuration;
            foreach (var target in config.AllTargets)
            {
                if (target.Name == "csvfile")
                {
                    var found = target;
                }
            }

        }
    }

    [TestClass]
    public class GenericTest
    {
        [TestMethod]
        public void DictionaryFind()
        {
            Dictionary<string, string> Frecuencies = new Dictionary<string, string>()
            {
                {"Uno","Uno" },
                {"Dos","Dos" }
            };

            var fid = "Dos";
            var fitem = Frecuencies.Where(f => fid == f.Key).FirstOrDefault();
            Debug.WriteLine(fitem.Key == null ? "Not found" : fitem.Value);
        }
    }

}
