﻿using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Diagnostics;


using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

//using U5ki.PresenceService;
//using U5ki.PresenceService.Interfaces;
//using U5ki.PresenceService.Engines;
//using U5ki.PresenceService.Agentes;

using U5ki.Infrastructure;
using U5ki.TifxService;

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
            var cfg = JsonConvert.DeserializeObject<Cd40Cfg>(jcfg);
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
        byte GlobalVersion = 0;
        byte ResourceVersion = 0;
        int LastResourceStd = 0;
        void PrepareTestTifx(Action<TifxService, UdpClient, IPEndPoint> next)
        {
            var cfg = JsonConvert.DeserializeObject<Cd40Cfg>(File.ReadAllText("u5ki.LastCfg.json"));
            var service = new TifxService("224.10.10.1", "11.12.60.35", 1001, cfg);
            service.Start();
            var client = new UdpClient();
            var to = new IPEndPoint(IPAddress.Parse("224.10.10.1"), 1001);

            next(service, client, to);
        }
        void PrepareEmptyFrame(Action<byte[]> next)
        {
            var frame = new byte[]
            {
                0, 0, 0, 4,
                0x50, 0x52, 0x4f, 0x58, 0x49, 0x45, 0x53, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,0,0,0,0,
                0, 0, 0, 0,
                0, 0, 0, GlobalVersion
            };
            next(frame);
            GlobalVersion++;
        }
        void PrepareProxyActiveFrame(Action<byte[]> next, string ipp, int std)
        {
            ResourceVersion = (byte)(LastResourceStd != std ? ResourceVersion + 1 : ResourceVersion);
            var frame = new byte[]
            {
                0, 0, 0, 4,
                0x50, 0x52, 0x4f, 0x58, 0x49, 0x45, 0x53, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,0,0,0,0,
                0, 0, 0, 1,
                0, 0, 0, GlobalVersion
            };
            var ippb = Encoding.ASCII.GetBytes(ipp);
            Array.Resize<byte>(ref ippb, 36 + 24);
            ippb[36 + 3] = 8;
            ippb[36 + 4 + 3] = ResourceVersion;
            ippb[36 + 4 + 4 + 3] = (byte)std;//estado

            next(frame.Concat(ippb).ToArray());
            GlobalVersion++;
            LastResourceStd = std;
            Debug.WriteLine($"{ResourceVersion}, {LastResourceStd}");
        }

        [TestMethod]
        public void TifxForProxiesTest()
        {
            PrepareTestTifx((service, client, to) =>
            {
                Task.Delay(TimeSpan.FromSeconds(5)).Wait();
                for (int i=0; i< 30; i++)
                {
                    var std = i % 5 == 0 ? 3 : 0;
                    Debug.WriteLine($"Sending std => {std}");
                    PrepareProxyActiveFrame((frame) => client.Send(frame, frame.Count(), to), "10.99.60.36", std);
                    Task.Delay(TimeSpan.FromSeconds(5)).Wait();
                }

                //Task.Delay(TimeSpan.FromSeconds(5)).Wait();
                //PrepareProxyActiveFrame((frame) => client.Send(frame, frame.Count(), to), "10.99.60.36", 3);
                //Task.Delay(TimeSpan.FromSeconds(5)).Wait();
                //PrepareProxyActiveFrame((frame) => client.Send(frame, frame.Count(), to), "10.99.60.36", 1);
                //Task.Delay(TimeSpan.FromSeconds(5)).Wait();
                //PrepareProxyActiveFrame((frame) => client.Send(frame, frame.Count(), to), "10.99.60.36", 3);
                ////PrepareEmptyFrame((frame) => client.Send(frame, frame.Count(), to));
                //Task.Delay(TimeSpan.FromSeconds(5)).Wait();
                service.Stop();
            });
        }
    }
}
