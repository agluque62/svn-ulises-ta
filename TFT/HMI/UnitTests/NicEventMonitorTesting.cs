using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

using Utilities;

namespace UnitTests
{
    [TestClass]
    public class NicEventMonitorTesting
    {
        string filePath = "D:\\Datos\\Empresa\\work\\Proyectos\\Ulises5000\\Seguimiento e Incidencias\\v25x\\Incidencias\\20181025. Eventos LAN en puestos nuevos\\Eventos Sistema UCS02_URR_BCN.evtx";
        string jconfig = "{\"TeamingType\": \"Intel\", \"WindowsLog\": \"System\", \"EventSource\":\"iANSMiniport\", \"UpEventId\": 15, \"DownEventId\": 11, \"PropertyIndex\": 1 }";

        [TestMethod]
        public void TestMethod1()
        {
            //Debug.WriteLine("Start...");
            //Task task = Task.Factory.StartNew(() =>
            //{
            //    Stopwatch stopWatch = new Stopwatch();
            //    Debug.WriteLine($"Task Start ");
            //    stopWatch.Start();
            //    using (NicEventMonitor monitor = new NicEventMonitor(jconfig, /*filePath*/""))
            //    {
            //        NicEventMonitor.LanStatus lan1 = monitor.NICList.Count > 0 ? monitor.NICList[0].Status : NicEventMonitor.LanStatus.Unknown;
            //        NicEventMonitor.LanStatus lan2 = monitor.NICList.Count > 1 ? monitor.NICList[1].Status : NicEventMonitor.LanStatus.Unknown;
            //    }
            //    stopWatch.Stop();
            //    Debug.WriteLine($"Task End Elapsed: {stopWatch.ElapsedMilliseconds}");
            //    task = null;
            //});
            //while (task!=null)
            //{
            //    Stopwatch stopWatch = new Stopwatch();
            //    stopWatch.Start();
            //    Thread.Sleep(50);
            //    stopWatch.Stop();
            //    Debug.WriteLine($"Main Elapsed: {stopWatch.ElapsedMilliseconds}");
            //}
            //Debug.WriteLine("Stop...");
        }

        [TestMethod]
        public void GenerateEventTest()
        {
            Debug.WriteLine("Start...");

            Task monitorTask = null;
            NicEventMonitor mon = null;

            monitorTask = Task.Factory.StartNew(() =>
            {
                string jconfig = Properties.Settings.Default.LanTeamConfigs.Count > Properties.Settings.Default.LanTeamType ?
                    Properties.Settings.Default.LanTeamConfigs[Properties.Settings.Default.LanTeamType] : "";

                using (mon = new NicEventMonitor(jconfig,
                    (lan, status) =>
                    {
                        Debug.WriteLine($"Notificado cambio en LAN {lan} => {status}");
                    }, (m, x)=> 
                    {
                        Debug.WriteLine($"Error Message: {m}");
                    }/*, filePath*/))
                {
                    while (monitorTask != null)
                    {
                        Task.Delay(100).Wait();
                    }
                }
                Debug.WriteLine("Stopping...");
            });

            Task.Delay(5000).Wait();
            mon?.EventSimulate(0, false);

            Task.Delay(5000).Wait();
            mon?.EventSimulate(1, false);

            Task.Delay(5000).Wait();
            mon?.EventSimulate(0, true);

            Task.Delay(5000).Wait();
            mon?.EventSimulate(1, true);

            Task.Delay(5000).Wait();
            monitorTask = null;

            Task.Delay(1000).Wait();
            Debug.WriteLine("Stop...");
        }
    }
}
