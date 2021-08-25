using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

using Utilities;

namespace UnitTestProject1
{
    [TestClass]
    public class NicMonitorTest
    {
        [TestMethod]
        public void IntelChipsetTest()
        {
            string filePath = "c:\\Users\\arturo.garcia\\Downloads\\CED_PICT03_eventos_sistema.evtx";
            string jconfig = "{ \"TeamingType\": \"Intel\", \"WindowsLog\": \"System\", \"EventSource\": \"iANSMiniport\", \"UpEventId\": [15], \"DownEventId\": [11], \"PropertyIndex\": 1 }";
            var mon = new NicEventMonitor(jconfig,
                    (lan, status) =>
                    {
                        Debug.WriteLine(String.Format("Notificado cambio en LAN {0} => {1}", lan, status));
                    }, (m, x) =>
                    {
                        Debug.WriteLine(String.Format("Error Message: {0}", m));
                    }, filePath);
            Debug.WriteLine("NetworkIFSupervisor Arrancado...");

            Task.Delay(TimeSpan.FromSeconds(5)).Wait();
            mon.EventSimulate(1, false);
            Task.Delay(TimeSpan.FromSeconds(2)).Wait();
            mon.EventSimulate(1, true);
            Task.Delay(TimeSpan.FromSeconds(2)).Wait();

            mon.Dispose();

            Task.Delay(TimeSpan.FromSeconds(2)).Wait();


        }
        [TestMethod]
        public void MarvellChipsetTest()
        {
            string filePath = "c:\\Users\\arturo.garcia\\Downloads\\EvSistema, Puesto chipsetMarvel.evtx";
            string jconfig = "{ \"TeamingType\": \"Marvell\", \"WindowsLog\": \"System\", \"EventSource\": \"yukonw7\", \"UpEventId\": [121,123], \"DownEventId\": [83], \"PropertyIndex\": 0 }";
            var mon = new NicEventMonitor(jconfig,
                    (lan, status) =>
                    {
                        Debug.WriteLine(String.Format("Notificado cambio en LAN {0} => {1}", lan, status));
                    }, (m, x) =>
                    {
                        Debug.WriteLine(String.Format("Error Message: {0}", m));
                    }, filePath);
            Debug.WriteLine("NetworkIFSupervisor Arrancado...");

            Task.Delay(TimeSpan.FromSeconds(5)).Wait();

            mon.Dispose();

            Task.Delay(TimeSpan.FromSeconds(2)).Wait();


        }
    }
}
