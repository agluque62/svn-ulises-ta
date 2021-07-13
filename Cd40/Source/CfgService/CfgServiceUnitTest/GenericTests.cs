using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Utilities;

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
    }
}
