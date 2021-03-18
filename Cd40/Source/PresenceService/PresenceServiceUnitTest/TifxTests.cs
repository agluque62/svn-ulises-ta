using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Forms;
using System.Diagnostics;

//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using Newtonsoft.Json.Converters;

//using U5ki.PresenceService;
//using U5ki.PresenceService.Interfaces;
//using U5ki.PresenceService.Engines;
//using U5ki.PresenceService.Agentes;

using U5ki.Infrastructure;
using U5ki.TifxService;
using Utilities;

namespace PresenceServiceUnitTest
{
    [TestClass]
    public class TifxTests
    {
        [TestMethod]
        public void RM4763_test_01()
        {
#if DEBUG
            var jcfg = File.ReadAllText("u5ki.LastCfg.json");
            var cfg = JsonHelper.Parse<Cd40Cfg>(jcfg);
            var service = new TifxService();

            var prxInfo = new GwInfo()
            {
                GwId = "PROXIES",
                GwIp = "",
                Type = 4,
                NumRs = 9,
                Resources = new U5ki.TifxService.RsInfo[]
                 {
                     new U5ki.TifxService.RsInfo(){ RsId="10.12.60.126:5060", GwIp="", State=3, Type=5, Version=0, Priority=0, CallBegin=0, Steps=0},
                     new U5ki.TifxService.RsInfo(){ RsId="10.99.60.36:5060", GwIp="", State=3, Type=7, Version=0, Priority=0, CallBegin=0, Steps=0},
                     new U5ki.TifxService.RsInfo(){ RsId="10.99.60.37:5060", GwIp="", State=3, Type=8, Version=0, Priority=0, CallBegin=0, Steps=0},
                     new U5ki.TifxService.RsInfo(){ RsId="10.15.60.38:5060", GwIp="", State=3, Type=8, Version=0, Priority=0, CallBegin=0, Steps=0},
                     new U5ki.TifxService.RsInfo(){ RsId="10.99.60.36:5060", GwIp="", State=3, Type=7, Version=0, Priority=0, CallBegin=0, Steps=0},
                     new U5ki.TifxService.RsInfo(){ RsId="10.99.60.37:5060", GwIp="", State=3, Type=8, Version=0, Priority=0, CallBegin=0, Steps=0},
                     new U5ki.TifxService.RsInfo(){ RsId="10.99.60.36:5060", GwIp="", State=3, Type=7, Version=0, Priority=0, CallBegin=0, Steps=0},
                     new U5ki.TifxService.RsInfo(){ RsId="10.99.60.37:5060", GwIp="", State=3, Type=8, Version=0, Priority=0, CallBegin=0, Steps=0},
                     new U5ki.TifxService.RsInfo(){ RsId="192.168.0.56:5060", GwIp="", State=3, Type=8, Version=0, Priority=0, CallBegin=0, Steps=0},
                 },
                Version = 1,
                LastReceived = null
            };

            service.RM4763_Test(cfg, prxInfo);
#endif
        }
    }
}
