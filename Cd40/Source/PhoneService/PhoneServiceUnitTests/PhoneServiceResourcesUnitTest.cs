using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Forms;


using u5ki.PhoneService;

using U5ki.Infrastructure;
using Utilities;

namespace PhoneServiceUnitTests
{
    [TestClass]
    public class PhoneServiceResourcesUnitTest
    {
        [TestMethod]
        public void StartStopTestCase()
        {
            IService phs = new PhoneService();

            phs.Start();

            Task.Delay(5000).Wait();

            phs.Stop();
        }
    }
}
