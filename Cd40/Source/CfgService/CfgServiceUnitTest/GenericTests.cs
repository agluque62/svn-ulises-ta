using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Diagnostics;

using Newtonsoft.Json;

using Utilities;
using U5ki.Infrastructure;
using U5ki.CfgService;
using U5ki.CfgService.SoapCfg;

namespace CfgServiceUnitTest
{
    [TestClass]
    public class GenericTests
    {
        [TestMethod]
        public void EventQueueTest()
        {
            var wt = new EventQueue();
            wt.Start();
            bool taskstarted = true;

            var t1 = Task.Run(() =>
            {
                var ticks = 0;
                while (taskstarted == true)
                {
                    Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                    wt.Enqueue("T1 events", () =>
                    {
                        Debug.WriteLine($"T1 event {ticks}");
                        ticks++;
                    });
                }
                Debug.WriteLine("T1 ended");
            });

            var t2 = Task.Run(() =>
            {
                var ticks = 0;
                while (taskstarted == true)
                {
                    Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                    wt.Enqueue("T1 events", () =>
                    {
                        Debug.WriteLine($"T2 event {ticks++}");
                    });
                }
                Debug.WriteLine("T2 ended");
            });

            Task.Delay(TimeSpan.FromSeconds(15)).Wait();
            wt.Enqueue("Stop", () =>
            {
                Debug.WriteLine("Stopping...");
                wt.InternalStop();
            });
            Task.Delay(TimeSpan.FromSeconds(5)).Wait();
            taskstarted = false;
            Task.Delay(TimeSpan.FromSeconds(5)).Wait();
        }

        [TestMethod]
        public void SoapServerMethod()
        {
            using (InterfazSOAPConfiguracion soapSrv = new InterfazSOAPConfiguracion())
            {
                soapSrv.Timeout = (int)TimeSpan.FromSeconds(5).TotalMilliseconds;
                var entryDate = DateTime.Now;
                try
                {
                    //var mcp = soapSrv.GetParametrosMulticast("departamento");
                    var mcp = TrySoap.Get("departamento", soapSrv.GetParametrosMulticast);
                    if (mcp.First == false) return;

                    var hfd = TrySoap.Get("departamento", soapSrv.GetPoolHfElement);
                    var eep = TrySoap.Get("departamento", "1", soapSrv.GetPoolNMElements);
                }
                catch (Exception x)
                {
                    Debug.WriteLine($"{x.Message} Elapsed => {DateTime.Now - entryDate}");
                }
            }
        }

        class TrySoap
        {
            public static Pair<bool,T> Get<T>(string p, Func<string, T> method)
            {
                try
                {
                    Debug.WriteLine($"method => {method}, p => {p}");
                    return new Pair<bool, T>(true, method(p));
                }
                catch (Exception x)
                {
                    Debug.WriteLine($"method => {method}, p => {p}, Exception => {x.Message}");
                    var cont = !(x is WebException) || (x as WebException).Status != WebExceptionStatus.Timeout;
                    return new Pair<bool, T>(cont, default);
                }
            }
            public static T Get<T>(string p1, string p2, Func<string, string, T> method)
            {
                try
                {
                    Debug.WriteLine($"method => {method}, p1 => {p1}, p2 => {p2}");
                    return method(p1, p2);
                }
                catch (Exception x)
                {
                    Debug.WriteLine($"method => {method}, p => {p1}, p2 => {p2}, Exception => {x.Message}");
                    return default;
                }
            }
        }

        [TestMethod]
        public void CfgServiceStartStop()
        {
            var service = new CfgService();
            service.SimulatorForTesting = true;
            service.SimulatedCfg = NewConfig();

            service.Start();
            Task.Delay(TimeSpan.FromSeconds(1)).Wait();
            service.SimulateToSlave();
            Task.Delay(TimeSpan.FromSeconds(1)).Wait();
            service.SimulateToMaster();
            Task.Delay(TimeSpan.FromSeconds(20)).Wait();

            service.SimulateNewMcastMessage(NewConfig());
            Task.Delay(TimeSpan.FromSeconds(20)).Wait();

            service.Stop();

            Task.Delay(TimeSpan.FromSeconds(1)).Wait();
        }

        Cd40Cfg NewConfig()
        {
            var cfg = JsonConvert.DeserializeObject<Cd40Cfg>(File.ReadAllText("SimulatedCfg.json"));
            cfg.Version = DateTime.Now.ToString();
            return cfg;
        }

    }
}
