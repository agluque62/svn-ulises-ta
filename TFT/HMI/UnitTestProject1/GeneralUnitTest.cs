using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class GeneralUnitTest
    {
        [TestMethod]
        public void TestMethod1()
        {

            var hash1 = $"123456".GetHashCode();
            var hash2 = $"213456".GetHashCode();


        }
    }
}
